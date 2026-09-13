# EgészségÚt / MedActivities

Egészségügyi páciens-, esemény- és időpontkezelő rendszer React webes klienssel és Designer-kompatibilis Windows Forms pácienskezelővel. A két kliens ugyanazt a jogosultságvédett REST API-t használja, az API pedig SQL Serverhez vagy fejlesztési célból SQLite-hoz kapcsolódik.

## Aktuális állapot

Az első véglegesítési csomag elkészült:

- SQL Server EF Core provider és külön SQL Server migration-készlet.
- Konfigurálható SQL Server/SQLite adatkapcsolat; induláskor nincs automatikus production migration.
- ASP.NET Identity tokenes bejelentkezés és szerepkörök: Admin, AdmissionsOffice, Practitioner, Patient.
- Páciens teljes CRUD, egyedi 9 számjegyű TAJ-kezelés, kezdő nullák megőrzésével.
- Kezelő, hozzárendelés, esemény és időpont CRUD, ütközés- és munkaidő-ellenőrzéssel.
- Páciensenkénti megjegyzés- és dokumentum-CRUD. A támogatott fájlok PDF, PNG, JPEG és UTF-8 TXT, legfeljebb 5 MB; a tartalom SQL Serveren `varbinary(max)` mezőben tárolódik.
- React felületek a páciens-adatlaphoz, megjegyzésekhez, dokumentumokhoz és időpontok átfoglalásához.
- Új Windows Forms kliens a `Desktop_Pacienskezelo` mappában. Az űrlapokhoz külön `.Designer.cs` és `.resx` tartozik, ezért Visual Studio 2026-ban a Designer megnyitható és szerkeszthető.
- A WinForms kliens API-bejelentkezést használ, nem tárol SQL-jelszót, és ugyanazokat az SQL Server-adatokat látja, mint a web.
- 64 lépéses, izolált SQL Server–API–WinForms integrációs ellenőrzés; a régi SQLite klienshez külön regressziós teszt maradt.

A projektben nincs becsomagolt üzemi jelszó vagy felhasználói adatbázis.

## Architektúra

```text
React + TypeScript ───────┐
                          ├── ASP.NET Core REST API ── Persistence / Domain ── SQL Server
Windows Forms ────────────┘                                     └────────────── SQLite (dev)
```

A desktop kliens szándékosan az API-n keresztül működik. Így a páciensadatok, TAJ, dokumentumok és jogosultságok nem kerülnek megkerülhető, külön asztali adatkezelési logikába.

## Projektstruktúra

```text
API/                         ASP.NET Core API és kontrollerek
Application/                 alkalmazási réteg
Domain/                      domain entitások
Persistence/                 EF Core DbContext és migrationök
Persistence/Migrations/      SQLite migrationök
Persistence/Migrations/SqlServer/ SQL Server migrationök
client/                      aktív React + TypeScript kliens
Desktop_Pacienskezelo/       Designer-kompatibilis SQL Server/API WinForms kliens
Desktop/                     korábbi, közvetlen SQLite fejlesztői kliens
IntegrationTests/            SQL Server/API/WinForms integrációs ellenőrzés
Desktop.IntegrationTests/   SQLite és régi desktop regressziós ellenőrzés
archive/web-tutorial/       korábbi, nem használt webes sablonkód megőrzött másolata
```

A Visual Studio 2026-ban megnyitható külön asztali megoldás:
[Desktop_Pacienskezelo/Desktop_Pacienskezelo.slnx](Desktop_Pacienskezelo/Desktop_Pacienskezelo.slnx).

## Előfeltételek

- .NET 10 SDK.
- Visual Studio 2026 Windows Forms és .NET asztali fejlesztési workloaddal.
- Node.js 20 vagy újabb és npm.
- SQL Server vagy SQL Server Express/LocalDB. A fejlesztői példa LocalDB-t használ.
- Első futtatáskor a HTTPS fejlesztői tanúsítvány: `dotnet dev-certs https --trust`.

## SQL Server beállítása

A gyökér `API/appsettings.json` fejlesztői LocalDB-példát tartalmaz:

```text
Server=(localdb)\MSSQLLocalDB;Database=MedActivities;Integrated Security=True;Encrypt=True;TrustServerCertificate=True
```

A `TrustServerCertificate=True` csak helyi fejlesztéshez való. Üzemi SQL Serveren használj hitelesített tanúsítványt és a saját connection stringet.

1. Állítsd be a connection stringet környezeti változóban vagy User Secrets-ben; ne írd jelszóval a verziókezelt fájlba.

2. Futtasd a migrációt explicit módon:

```powershell
dotnet tool restore
$env:Database__Provider = "SqlServer"
$env:ConnectionStrings__SqlServerConnection = "Server=(localdb)\MSSQLLocalDB;Database=MedActivities;Integrated Security=True;Encrypt=True;TrustServerCertificate=True"
dotnet ef database update --context SqlServerDbContext --project Persistence/Persistence.csproj --startup-project API/API.csproj
```

Ugyanez a repository gyökeréből futtatható a `scripts/Initialize-SqlServer.ps1` segédszkripttel is; a script a connection stringet csak a futó folyamat környezetében használja, és befejezéskor visszaállítja a korábbi környezeti változókat.

3. Az első admin-fiókhoz csak új e-mail-cím esetén használható bootstrap:

```powershell
$env:BootstrapAdmin__Email = "admin@example.invalid"
$env:BootstrapAdmin__Password = "Adj-meg-egy-erős-jelszót"
dotnet run --project API/API.csproj
```

A bootstrap értékeket a futtatási folyamat végén töröld a PowerShell-munkamenetből. A `Database:ApplyMigrations` alapértéke `false`; éles környezetben a migrációt külön release-lépésként futtasd. Részletes beállítási példák: [API/appsettings.SqlServer.example.json](API/appsettings.SqlServer.example.json) és [docs/SQL-SERVER.md](docs/SQL-SERVER.md).

## SQLite fejlesztési mód

SQLite-hoz explicit módon válts:

```powershell
$env:Database__Provider = "Sqlite"
$env:ConnectionStrings__DefaultConnection = "Data Source=activities.db"
dotnet ef database update --project Persistence/Persistence.csproj --startup-project API/API.csproj
dotnet run --project API/API.csproj
```

A SQLite migrationök a `Persistence/Migrations` mappában vannak. A SQL Server és SQLite sémát nem keverd ugyanabban az adatbázisban.

## Webes kliens indítása

Külön terminálban:

```powershell
cd client
npm install
npm run dev
```

A Vite fejlesztői szerver a `https://localhost:3000` címet használja, és az API `https://localhost:5001` címére proxyz. Kiadási build:

```powershell
cd client
npm run build
```

Az elkészült `client/dist` tartalmát webes kiadáskor az API `wwwroot` mappájába vagy külön statikus tárhelyre telepítsd. A CORS eredeteket a `Cors:Origins` konfigurációban állítsd.

## Windows Forms kliens

Visual Studio 2026-ban nyisd meg a [Desktop_Pacienskezelo.slnx](Desktop_Pacienskezelo/Desktop_Pacienskezelo.slnx) fájlt, vagy a gyökér [MedActivities.slnx](MedActivities.slnx) megoldást. Indítsd a `Desktop_Pacienskezelo` projektet.

Parancssorból:

```powershell
dotnet build Desktop_Pacienskezelo/Desktop_Pacienskezelo/Desktop_Pacienskezelo.csproj
dotnet run --project Desktop_Pacienskezelo/Desktop_Pacienskezelo/Desktop_Pacienskezelo.csproj
```

Az API-cím alapértéke `https://localhost:5001/api`, illetve a `MEDACTIVITIES_API_URL` környezeti változóval módosítható. Bejelentkezés után az alkalmazás a következőket kezeli:

- páciens lista, TAJ szerinti keresés, létrehozás, szerkesztés, törlés;
- események és kezelőhozzárendelések;
- foglalások létrehozása, átfoglalása, lemondása, státusza és törlése jogosultság szerint;
- páciensenkénti dokumentumfeltöltés/letöltés/átnevezés/törlés;
- páciensenkénti megjegyzés létrehozása/szerkesztése/törlése.

A `Form1.cs`, `PatientEditForm.cs`, `ActivityEditForm.cs`, `AppointmentForm.cs` és `RecordsForm.cs` mellett az azonos nevű Designer fájlok statikus vezérlődefiníciókat tartalmaznak. A konstruktorok nem kapcsolódnak adatbázishoz, ezért a Designerben biztonságosan megnyithatók.

## Tesztelés

A teljes SQL Server ellenőrzéshez LocalDB szükséges:

```powershell
dotnet build MedActivities.slnx
dotnet run --project IntegrationTests/IntegrationTests.csproj -- "C:\IT2026-VIZSGA\00_VIZSGAREMEK_02_WEB\MedAcitivities"
```

A teszt minden futáskor `MedActivities_Test_<azonosító>` nevű új adatbázist használ, majd csak ezt az adatbázist törli. A valódi `MedActivities` adatbázist, a felhasználó `activities.db` fájlját és más meglévő adatot nem érinti. A WinForms formok bitmap-renderelése is ellenőrzött.

SQLite regresszió:

```powershell
dotnet run --project Desktop.IntegrationTests/Desktop.IntegrationTests.csproj
```

A jelenlegi környezetben az SQL Server integrációs futás 64 ellenőrzése sikeres volt. A frontend Vite kiadási buildje elkészült; a bundle mérete miatt Vite csak optimalizálási figyelmeztetést ad.

## Ismert korlátozások

- A dokumentumok mérete és alapvető fájltartalma ellenőrzött, de a csomag nem vírusirtó és nem teljes orvosi dokumentum-validáló rendszer.
- A SQLite fejlesztési útvonalhoz a `SQLitePCLRaw.lib.e_sqlite3` csomag upstream biztonsági figyelmeztetése fennáll; végleges telepítéshez SQL Server profilt használj, és a SQLite függőség frissítését külön kompatibilitási feladatként kezeld.
- A projektben maradt régi tutorial kód az `archive/web-tutorial` mappában van, nem része az aktív frontend fordításnak.
- A SQL Serverre váltás után egy meglévő SQLite üzemi adatbázis adatainak automatikus konverzióját ne indítsd el ellenőrzés nélkül. Készíts mentést, ellenőrizd a TAJ- és kapcsolatadatokat, majd az SQL Server migration után célzott importot végezz.

## Leadás előtti ellenőrzőlista

- [ ] SQL Server adatbázis és migration sikeres.
- [ ] Bootstrap admin létrehozása, majd szerepkörök ellenőrzése.
- [ ] Webes belépés, páciens-adatlap, dokumentum és időpont kipróbálása.
- [ ] WinForms Designer megnyitása Visual Studio 2026-ban.
- [ ] WinForms belépés ugyanazzal az API-val és SQL Serverrel.
- [ ] `dotnet build MedActivities.slnx`, SQL Server integrációs teszt és `npm run build` sikeres.
- [ ] Connection string, jelszó, SQLite fájl és tesztartefaktum nincs a beadandó ZIP-ben.

## Szerző

**Varga András Ernő**
