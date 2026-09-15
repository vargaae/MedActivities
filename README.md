# MedActivities – EgészségÚt

Egészségügyi páciens-, esemény- és időpontkezelő vizsgaremek, magyar nyelvű React webalkalmazással és Windows Forms pácienskezelővel. A két alkalmazás közös ASP.NET Core API-n keresztül dolgozik ugyanazon Microsoft SQL Server adatbázison.

## Gyorsindítás klónozás után – Windows, PowerShell

### 1. Szükséges programok és a repo letöltése

Telepítsd:

- Git;
- .NET 10 SDK;
- Node.js 22 LTS, legalább 22.12-es verzió, npm-mel;
- SQL Server Express LocalDB; mentés visszaállításához a mentést készítő SQL Serverrel azonos vagy újabb főverzió szükséges;
- Microsoft `sqlcmd` parancssori eszköz;
- PowerShell 7 (`pwsh`);
- Visual Studio 2026 és a **.NET asztali fejlesztés** munkaterhelés, ha a Windows Forms Designert is használni szeretnéd.

Az alábbi parancsok **PowerShell-parancsok**, nem Git Bash-parancsok. Csak a kódblokkok tartalmát másold be, a terminál `PS C:\...>` előtagját ne. A környezeti változókban két aláhúzás szerepel: `__`.

```powershell
git clone https://github.com/vargaae/MedAcitivities.git
cd MedAcitivities

dotnet --version
node --version
sqlcmd -?
SqlLocalDB start MSSQLLocalDB
dotnet dev-certs https --trust
dotnet tool restore
dotnet restore MedActivities.slnx
```

Ha a LocalDB-példány még nem létezik, egyszer futtasd a `SqlLocalDB create MSSQLLocalDB` parancsot, majd indítsd el.

### 2. A repóhoz mellékelt végleges adatbázis-export

Az aktuális adatbázis 2026. szeptember 15-i exportjai a `database/exports/20260915/` mappában találhatók. A Git figyelmen kívül hagyási szabályai ezeket a konkrét fájlokat engedélyezik. **Klónozáskor csak a már commitolt és feltöltött fájlok érkeznek meg**; a helyi export elkészítése önmagában nem jelent GitHub-feltöltést.

| Fájl | Tartalom |
| --- | --- |
| `MedActivities.bak` | Teljes, COPY_ONLY és CHECKSUM mentés; ez az ajánlott visszaállítási forrás. |
| `MedActivities.sql` | UTF-8 SQL-dump sémával, migrációtörténettel és adatokkal, üres céladatbázishoz. |
| `MedActivities.mdf` | A mentésből visszaállított és leválasztott másolat adatfájlja. |
| `MedActivities_log.ldf` | Az MDF-hez tartozó naplófájl; az MDF-fel együtt kezelendő. |
| `schema.sql` | SQL Server-séma; külön önmagában nem tartalmaz üzleti adatokat. |

A pillanatképben **22 páciens, 17 kezelőorvos, 1004 esemény, 6 foglalás és 172 felhasználó** található, a kapcsolatokkal, megjegyzésekkel, dokumentumokkal és archívummal együtt. Az export nem cserélte le és nem generálta újra az adatokat.

Forrás SQL Server-verzió: **17.0.4025.3** (17-es főverzió). BAK-visszaállításhoz és MDF/LDF csatoláshoz ezzel kompatibilis, azonos vagy újabb SQL Server szükséges. Régebbi szerverhez a SQL-dump kompatibilitását külön ellenőrizni kell.

Ellenőrzés: a BAK `RESTORE VERIFYONLY WITH CHECKSUM` ellenőrzése és külön másolatba visszaállítása sikeres; az SQL-dump külön üres LocalDB-adatbázisba importálva, `DBCC CHECKDB` ellenőrzéssel és a fő rekorddarabszámok egyeztetésével sikeres. Az eredeti adatbázist nem módosítottuk.

**Adatvédelem:** a teljes mentések felhasználói jelszóhasheket, dokumentumokat és törlési archívumot is tartalmaznak. Nyilvános feltöltés előtt ellenőrizd az összes adatot és a fiókokat; valós betegadatot vagy újrahasznált hitelesítési adatot ne publikálj. Az export nem anonimizálás.

A korábbi `before-profile-replacement.bak` megmaradt helyi biztonsági mentésként, de nincs engedélyezve a Gitben, és az alábbi telepítés nem azt használja.

### 3. Teljes BAK-mentés visszaállítása

Az API még ne fusson. A repo gyökerében, **PowerShell 7-ben** futtasd az alábbi blokkot. A kód a mentésből olvassa ki a logikai fájlneveket, ezért nem igényli a készítő számítógépének elérési útjait.

A visszaállítás csak új `MedActivities` adatbázisra engedélyezett. Meglévő adatbázist és adatfájlt nem ír felül.

```powershell
$ErrorActionPreference = "Stop"
$backupPath = (Resolve-Path "database/exports/20260915/MedActivities.bak").Path
$dataDirectory = Join-Path $env:LOCALAPPDATA "MedActivities/SqlData"
New-Item -ItemType Directory -Force -Path $dataDirectory | Out-Null
$masterConnection = [System.Data.SqlClient.SqlConnection]::new(
    "Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=True;Encrypt=True;TrustServerCertificate=True"
)
function SqlLiteral([string]$value) { return "N'" + $value.Replace("'", "''") + "'" }
$masterConnection.Open()
try {
    $command = $masterConnection.CreateCommand()
    $command.CommandTimeout = 180
    $command.CommandText = "SELECT DB_ID(N'MedActivities')"
    if ($command.ExecuteScalar() -isnot [DBNull]) {
        throw "A MedActivities adatbázis már létezik. A visszaállítás nem írja felül."
    }
    $command.CommandText = "RESTORE VERIFYONLY FROM DISK=$(SqlLiteral $backupPath) WITH CHECKSUM"
    [void]$command.ExecuteNonQuery()
    $command.CommandText = "RESTORE FILELISTONLY FROM DISK=$(SqlLiteral $backupPath)"
    $adapter = [System.Data.SqlClient.SqlDataAdapter]::new($command)
    $fileList = [System.Data.DataTable]::new()
    try { [void]$adapter.Fill($fileList) } finally { $adapter.Dispose() }
    $moves = @()
    $index = 0
    foreach ($file in $fileList.Rows) {
        $index++
        $extension = if ($file.Type -eq "L") { "ldf" } else { "mdf" }
        $targetPath = Join-Path $dataDirectory "MedActivities_$index.$extension"
        if (Test-Path -LiteralPath $targetPath) { throw "Már létező adatfájl: $targetPath" }
        $moves += "MOVE $(SqlLiteral $file.LogicalName) TO $(SqlLiteral $targetPath)"
    }
    $command.CommandText = "RESTORE DATABASE [MedActivities] FROM DISK=$(SqlLiteral $backupPath) WITH " + ($moves -join ", ") + ", RECOVERY"
    [void]$command.ExecuteNonQuery()
    Write-Host "Az adatbázis visszaállítása sikeres."
} finally {
    $masterConnection.Dispose()
}
```

Ez a példa helyi LocalDB-re készült. Külön SQL Server-szolgáltatás esetén a mentés és a célmappa a szerver számára is elérhető legyen, megfelelő fájljogosultságokkal.

**Ha teljes SQL-dumpot kaptál BAK helyett:** a fenti visszaállítást hagyd ki, és kizárólag üres céladatbázison futtasd:

```powershell
if (!(Test-Path "database/exports/20260915/MedActivities.sql")) { throw "Hiányzik a teljes SQL-dump." }
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -C -b -d master -Q "IF DB_ID(N'MedActivities') IS NOT NULL THROW 51000, 'A celadatbazis mar letezik.', 1; CREATE DATABASE [MedActivities];"
if ($LASTEXITCODE -ne 0) { throw "Az üres céladatbázis létrehozása sikertelen." }
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -C -b -f 65001 -d MedActivities -i "database/exports/20260915/MedActivities.sql"
if ($LASTEXITCODE -ne 0) { throw "Az adatimport sikertelen; ne folytasd az API indításával." }
```

A `schema.sql` nem helyettesíti a teljes dumpot. A BAK és SQL-dump két alternatíva: **nem kell mindkettőt importálni**. MDF/LDF átadásakor mindkét leválasztott adatfájl szükséges; a hordozható telepítéshez a BAK-visszaállítás ajánlott. Futó adatbázis MDF-jét ne másold és ne csatold újra.

### 4. API fordítása, migráció és indítás

A sikeres adat-visszaállítás után, ugyanebben a terminálban:

```powershell
$env:Database__Provider = "SqlServer"
$env:ConnectionStrings__SqlServerConnection = "Server=(localdb)\MSSQLLocalDB;Database=MedActivities;Integrated Security=True;Encrypt=True;TrustServerCertificate=True"
$env:Database__ApplyMigrations = "false"
$env:Database__SeedDemoData = "false"

dotnet build API/API.csproj
if ($LASTEXITCODE -ne 0) { throw "Az API fordítása sikertelen." }
dotnet ef database update --no-build --context SqlServerDbContext --project Persistence/Persistence.csproj --startup-project API/API.csproj
if ($LASTEXITCODE -ne 0) { throw "A migráció sikertelen." }

sqlcmd -S "(localdb)\MSSQLLocalDB" -E -C -b -d MedActivities -Q "SELECT 'Patients' AS Tabla, COUNT(*) AS Darab FROM Patients UNION ALL SELECT 'Practitioners', COUNT(*) FROM Practitioners UNION ALL SELECT 'Activities', COUNT(*) FROM Activities; SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId;"
```

A migráció a sémát frissíti, **nem tölti vissza a bemutatóadatokat**. Az ellenőrző lekérdezésben az export visszaállítása után 22 páciens, 17 kezelőorvos és 1004 esemény várható, amíg nem történik újabb adatmódosítás.

A desktophoz szükséges jelszavas helyi demóadmin beállítása:

```powershell
pwsh -File scripts/Configure-DemoAdmin.ps1
```

A szkript rejtetten bekéri a választott jelszót. Fiókja alapból `admin@example.undefined`; ismételt futtatással ennek a demófióknak a jelszava újra beállítható. Ez a lépés módosítja a visszaállított adatbázis demóadminját. Ha ismered egy meglévő dolgozói fiók belépési adatait, a lépés kihagyható. Nincs a README-be írt közös jelszó.

Ezután indítsd az API-t, és hagyd futni:

```powershell
dotnet run --project API/API.csproj --no-build --launch-profile https
```

API: **https://localhost:5001**. Másik terminálból:

```powershell
Invoke-RestMethod https://localhost:5001/api/health
```

Elvárt válasz: `status: ok`, `database: SqlServer`.

### 5. Webalkalmazás indítása – második terminál

A repo gyökeréből:

```powershell
cd client
npm ci
npm run dev
```

Nyisd meg: **[https://localhost:3000](https://localhost:3000)**. A Vite a `/api` kéréseket és a SignalR-kapcsolatot a futó API-ra továbbítja. Első indításkor a helyi HTTPS-tanúsítvány telepítésének jóváhagyása szükséges lehet.

A kezdőlapon használható az e-mailes/felhasználóneves belépés, illetve a négy DEMÓ szerepkör. A demóbelépés csak helyi, `Development` módban futó API mellett engedélyezett. Bejelentkezés nélkül az Események és az Időpontfoglalás nem indít üzleti adatletöltést.

### 6. Windows Forms indítása – harmadik terminál

A repo gyökeréből, már futó API mellett:

```powershell
$env:MEDACTIVITIES_API_URL = "https://localhost:5001/api"
dotnet run --project Desktop_Pacienskezelo/Desktop_Pacienskezelo/Desktop_Pacienskezelo.csproj
```

Jelentkezz be egy adminisztrátor, felvételi irodai dolgozó vagy kezelőorvos jelszavas fiókjával. Páciensfiókhoz a webes felület használható.

**A desktop nem közvetlenül SQL Serverhez kapcsolódik:** HTTP-n az API-t használja. Emiatt az API-nak és a LocalDB-nek a desktop teljes használata alatt futnia kell. A webes és asztali felületen ugyanazok az adatok érhetők el; módosítás után a másik kliensben frissítsd a listát.

Visual Studio 2026-ban nyisd meg a `Desktop_Pacienskezelo/Desktop_Pacienskezelo.slnx` megoldást. Startup projekt: `Desktop_Pacienskezelo`. Az űrlapok külön `.Designer.cs` és `.resx` fájlokat használnak; a Solution Explorerben az űrlap **View Designer / Tervező megnyitása** parancsával szerkeszthetők. Az API külön indítandó.

### Következő indítások

A csomagtelepítést, az adat-visszaállítást és a demóadmin-beállítást nem kell minden alkalommal megismételni.

1. `SqlLocalDB start MSSQLLocalDB`.
2. Repo gyökere: `dotnet run --project API/API.csproj --launch-profile https`.
3. Második terminál, `client` mappa: `npm run dev`.
4. Harmadik terminál, repo gyökere: `dotnet run --project Desktop_Pacienskezelo/Desktop_Pacienskezelo/Desktop_Pacienskezelo.csproj`.

Az alapkonfiguráció a helyi `MedActivities` SQL Server-adatbázist használja. Egyéni környezeti változókat minden új terminálban ismét be kell állítani. Leállítás: a szerverterminálokban `Ctrl+C`, a desktop ablakát zárd be.

## A vizsgaremek célja és működése

Az EgészségÚt egy egészségügyi ügyviteli bemutatórendszer: összekapcsolja a páciensek adatlapját, a kezelőorvosokat, a betegút eseményeit, az időpontfoglalást és a kapcsolódó dokumentumokat. A webalkalmazás szerepkör szerint jeleníti meg az elérhető funkciókat; az asztali alkalmazás a dolgozói pácienskezelést támogatja.

Oktatási célú vizsgaremek, nem minősített egészségügyi nyilvántartó rendszer. Bemutatásához fiktív adatok használhatók.

### Funkciók

- Páciensadatok listázása, keresése, létrehozása, olvasása, jogosultság szerinti szerkesztése és törlése.
- Magyar dátumformátum; hiányzó vagy érvénytelen születési dátum esetén biztonságos helyettesítő jel.
- TAJ szöveges tárolása, kilenc számjegyes formai és egyediségi ellenőrzése, kezdő nullák megőrzése; megjelenítés háromjegyű csoportokban. Nem hatósági TAJ-ellenőrzés.
- Kezelőorvosi profilok, szakterület, helyszín, munkaidő és foglalhatóság kezelése.
- Eseménykártyák kategóriaképekkel, részletező nézet, dátumtartomány- és szerepkör szerinti szűrés, harmincas lapozás és gyorsítótárazás.
- Időpontfoglalás, átfoglalás, lemondás és státuszkezelés; szerveroldali munkaidő-, ütközés- és napi foglalásellenőrzés.
- Páciensekhez kapcsolódó megjegyzések és dokumentumok kezelése. A feltölthető PDF, PNG, JPEG és UTF-8 TXT fájlok legfeljebb 5 MB méretűek.
- SignalR-alapú eseménychat, adatbázisban tárolt üzenetekkel.
- Identity-alapú felhasználók és szerepkörök; tokenes munkamenet és kijelentkezés.
- SQL Serveres törlési archívum: támogatott üzleti rekordok törlés előtti állapotának tárolása. Nem helyettesíti a biztonsági mentést.

### Szerepkörök

| Szerepkör | Felhasználás |
| --- | --- |
| Admin | Felhasználók, szerepkörök, páciensek, kezelőorvosok, hozzáférések, események, foglalások és foglalhatóság adminisztrációja. |
| Felvételi iroda (`AdmissionsOffice`) | Páciens- és adatlapkezelés, hozzáférések kiosztása, események és foglalások kezelése. |
| Kezelőorvos (`Practitioner`) | Az összes páciens alapadatainak olvasása, páciens magához rendelése; az **Adatlapkezelő - hozzám rendelt páciensek** nézetben csak saját hozzárendelései jelennek meg. A hozzárendelt páciensek eseménytörténete olvasható, időpont számukra foglalható. Más orvos eseményének olvasása önmagában nem ad szerkesztési jogot. |
| Páciens (`Patient`) | Saját adatlap és események elérése, engedélyezett saját adatok szerkesztése, időpontfoglalás és saját foglalások kezelése a weben. |

Az API szerveroldalon is ellenőrzi a hozzáférést. A menük elrejtése nem helyettesíti a jogosultságvizsgálatot. Hozzárendelés után a kapcsolódó webes listák automatikusan frissülnek. Nem hozzárendelt páciens betegútja és dokumentumai nem tölthetők le pusztán a pácienslista olvasási joga alapján.

## Felépítés és technológiák

```text
React web ───────┐
                ├── ASP.NET Core API ── Application / Persistence / Domain ── SQL Server
Windows Forms ──┘
```

- Frontend: React 19, TypeScript, Vite, Material UI, React Router, TanStack Query, Axios.
- Backend: .NET 10, ASP.NET Core, Identity, MediatR, SignalR.
- Adatelérés: Entity Framework Core 10, Microsoft SQL Server.
- Desktop: .NET 10 Windows Forms, Visual Studio Designer-támogatással.

| Mappa / megoldás | Tartalom |
| --- | --- |
| `MedActivities.slnx` | API, alkalmazási rétegek, aktív desktop és integrációs tesztek. |
| `API/Controllers` | HTTP-végpontok. |
| `API/Services`, `API/Models`, `API/Infrastructure` | API-szolgáltatások, bemeneti modellek és konfiguráció. |
| `Application` | Alkalmazási műveletek, lekérdezések és MediatR-kezelők. |
| `Domain` | Üzleti entitások. |
| `Persistence` | Adatbázis-kontextusok és migrációk. |
| `client` | Aktív webes frontend. |
| `Desktop_Pacienskezelo` | Aktív Windows Forms alkalmazás. |
| `IntegrationTests` | Konzolos SQL Server/API/WinForms tesztprogram. |
| `database` | Exportáló és ellenőrző szkriptek, valamint a mellékelt adatbázis-exportok. |
| `scripts`, `docs` | Üzemeltetési segédszkriptek és kiegészítő dokumentáció. |

## Konfiguráció és adatmegőrzés

Az API alapbeállításai: `API/appsettings.json`. A `https` indítási profil `Development` környezetet és az 5001-es HTTPS-portot állítja be.

| Beállítás | Jelentés |
| --- | --- |
| `Database__Provider=SqlServer` | Aktív SQL Server-provider. |
| `ConnectionStrings__SqlServerConnection` | SQL Server kapcsolati karakterlánc. |
| `Database__ApplyMigrations=false` | A migrációt külön parancs indítja. |
| `Database__SeedDemoData=false` | Ne induljon automatikus adatfeltöltés. |
| `MEDACTIVITIES_API_URL` | Desktop API-címe, alapból `https://localhost:5001/api`. |
| `VITE_API_URL` | Frontend API-alapcíme, alapból `/api`. |

A SQL Server-kontextus `SqlServerDbContext`. A migrációk az alap sémát, a chatet/törlési archívumot és a születési hely mezőt is tartalmazzák.

A SQLite megmaradt külön fejlesztési profilként, de **nem a bemutató alapértelmezett adatbázisa**, és az aktív desktop SQL Serveres API-t ellenőriz. A providerváltás nem másol át adatokat.

**Visszaállított export mellett ne futtasd rutinszerűen** a `--seed-demo-data` parancsot vagy a `database/replace-demo-profiles.sql` szkriptet: ezek módosítják a bemutató adatkészletét, nem az export visszaállítását végzik. A korábbi 100/50/1000-es demófeltöltési cél nem bizonyítja a végleges export darabszámait.

Az exportot a `database/Export-Database.ps1` segédszkript támogatja, megfelelő, naprakész `schema.sql` mellett. A mellékelt exportok elkészültek és visszaállítási ellenőrzésen estek át. Új exporthoz új célmappát adj meg: a szkript nem írja felül a meglévő mentést. A dump újraellenőrizhető a `pwsh -File database/Verify-Export.ps1` paranccsal; ez külön ideiglenes tesztadatbázist használ és a végén eltávolítja.

## Ellenőrzés és ismert állapot

Frontend:

```powershell
cd client
npm run build
npm run lint
node --experimental-strip-types --test tests/date-format.test.mjs
```

A dátumteszthez TypeScript-típuseltávolítást támogató Node.js 22.6+ szükséges; a fenti 22.12+ előfeltétel ezt teljesíti.

Integrációs tesztek a repo gyökeréből, leállított API és desktop mellett:

```powershell
dotnet build IntegrationTests/IntegrationTests.csproj
dotnet run --project IntegrationTests/IntegrationTests.csproj --no-build -- (Get-Location).Path
```

Csak a kezelőorvosi tesztág:

```powershell
dotnet run --project IntegrationTests/IntegrationTests.csproj --no-build -- (Get-Location).Path --practitioner
```

Ez konzolos tesztprogram, nem `dotnet test` projekt. Külön `MedActivities_Test_<azonosító>` LocalDB-adatbázist hoz létre, majd a tesztadatbázist eltávolítja; a bemutató-adatbázist nem használja.

A legutóbbi ellenőrzések során az API-fordítás, a frontend TypeScript-ellenőrzése, az érintett fájlok lintellenőrzése és a dátumtesztek sikeresek voltak. SQL Serveren működött az önmagához rendelés, a hozzárendelt páciens eseménytörténetének lekérése és az orvosi foglalás.

**Nyitott ellenőrzési pont:** az integrációs futás eseménytörlésnél, illetve kezelői hozzáférés visszavonásánál 500-as hibát jelzett. A teljes tesztcsomag sikeressége ezért nem állítható. A build a `SQLitePCLRaw.lib.e_sqlite3 2.1.11` csomagra NU1903 biztonsági figyelmeztetést is ad. Ezeket a végleges átadás előtt rendezni és újratesztelni kell.

## Hibaelhárítás

- **Git Bash: `command not found` az `$env:` soroknál:** válts PowerShellre.
- **Üres adatbázis migráció után:** a migráció sémát hoz létre, nem állít vissza adatokat. Teljes BAK vagy SQL-dump szükséges.
- **Hiányzó mentés klónozás után:** ellenőrizd, hogy az exportfájlokat tartalmazó commitot feltöltötték-e, és a megfelelő ágat klónoztad-e.
- **A céladatbázis már létezik:** a visszaállítás szándékosan megáll. Ne töröld a meglévőt és ne használj vakon `WITH REPLACE` kapcsolót; előbb készíts mentést és dönts a megőrzéséről.
- **A mentés újabb SQL Server-verzióból származik:** használj kompatibilis újabb példányt, vagy kérj a célverzióra ellenőrzött SQL-dumpot.
- **API build: fájl használatban:** állítsd le a korábbi API-t vagy Visual Studio hibakeresést.
- **HTTPS-hiba:** futtasd a `dotnet dev-certs https --trust` parancsot; a frontend tanúsítványának telepítését is engedélyezd.
- **Demóbelépés sikertelen:** az API a `https` profilban, helyben fusson; ellenőrizd az API terminálját és a health-választ.
- **Desktop nem csatlakozik:** az API fusson, a cím végén `/api` legyen, a health-válaszban pedig `SqlServer`.
- **Nincs foglalható időpont:** legyen engedélyezve a kezelő foglalhatósága és az adott napi munkaideje; a dátum legyen jövőbeli és az időpont szabad.

## Beadási csomag és bemutatás

A beadás része legyen a forráskód, ez a README, a projekt- és solutionfájlok, migrációk, csomagzárak, valamint **a `database/exports/20260915` mappa végleges adatbázis-exportjai**. Ne csomagolj `node_modules`, `bin`, `obj`, `.vs` vagy titkos konfigurációs fájlokat.

A bemutató előtt új gépen ellenőrizd a visszaállítást, a négy webes szerepkört, az orvosi hozzárendelést és foglalást, a desktop belépést és közös adatait, valamint a Designer megnyitását. A dokumentáció nem helyettesíti ezt a telepítési próbát.

## Szerző

**Varga András Ernő**
