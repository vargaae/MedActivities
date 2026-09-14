# MedActivities – EgészségÚt

Egészségügyi páciens-, esemény- és időpontkezelő vizsgaremek, magyar nyelvű webes felülettel és Windows Forms pácienskezelő alkalmazással. A két kliens közös ASP.NET Core API-n keresztül használja az SQL Server adatbázist, így ugyanazokkal az adatokkal és szerveroldali jogosultságokkal dolgoznak.

Az alapértelmezett adatbázis **Microsoft SQL Server**. A SQLite külön fejlesztési profilként maradt meg; az aktív Windows Forms kliens SQL Serverhez kapcsolódó API-t igényel.

## Tartalom

- [Funkciók és jogosultságok](#funkciók-és-jogosultságok)
- [Technológiák és projektstruktúra](#technológiák-és-projektstruktúra)
- [Előfeltételek](#előfeltételek)
- [Első indítás SQL Serverrel](#első-indítás-sql-serverrel)
- [Windows Forms és Visual Studio Designer](#windows-forms-és-visual-studio-designer)
- [Demóadatok és belépés](#demóadatok-és-belépés)
- [Konfiguráció és adatbázis](#konfiguráció-és-adatbázis)
- [Fordítás és ellenőrzés](#fordítás-és-ellenőrzés)
- [Kiadás](#kiadás)
- [Hibaelhárítás](#hibaelhárítás)
- [Ismert korlátozások](#ismert-korlátozások)

## Funkciók és jogosultságok

- **Pácienskezelés:** listázás, keresés, adatlap, létrehozás, módosítás és törlés; páciens–felhasználó összekapcsolás és kezelői hozzáférések kiosztása.
- **TAJ-kezelés:** szöveges tárolás, pontosan 9 ASCII számjegy, kezdő nullák megőrzése és duplikációellenőrzés a páciens-, illetve kezelőprofilok között, külön-külön. A rendszer formai és egyediségi ellenőrzést végez, nem hatósági TAJ-ellenőrzést.
- **Kezelők:** profilok, szakterület, helyszín, heti munkaidő és foglalhatóság kezelése; egy felhasználói fiókhoz nem hozható létre több kezelőprofil.
- **Események:** létrehozás, listázás, részletek, szerkesztés, törlés, páciens- és kezelőhozzárendelés, valamint páciens és kezelő szerinti szűrés.
- **Időpontok:** szabad időpontok lekérdezése, foglalás, átfoglalás, lemondás, státuszváltás és törlés. A rendszer ellenőrzi a munkaidőt, az ütközéseket és a páciens napi foglalását.
- **Egészségügyi feljegyzések:** páciensenkénti megjegyzések létrehozása, olvasása, módosítása és törlése; dokumentumok feltöltése, letöltése, címének módosítása és törlése.
- **Eseménychat:** SignalR-alapú, adatbázisban tárolt üzenetek a weben, MediatR-alapú üzenetkezeléssel és eseményhez kötött hozzáféréssel.
- **Felhasználókezelés:** Identity-alapú regisztráció és tokenes bejelentkezés, adminisztráció, szerepkörök és kijelentkezéskor szerveroldali munkamenet-érvénytelenítés.
- **Törlési archívum:** SQL Server-triggerek tárolják a támogatott üzleti táblákból törölt rekordok állapotát, a törlés idejét és végrehajtóját.

| Szerepkör | Fő jogosultságok |
| --- | --- |
| `Admin` | Teljes üzleti adatkezelés; felhasználók, szerepkörök, kezelőprofilok, munkaidő és foglalhatóság adminisztrációja. |
| `AdmissionsOffice` – felvételi iroda | Páciensek, kezelői hozzáférések, események és időpontok kezelése; páciensdokumentumok és megjegyzések kezelése. |
| `Practitioner` – kezelő | Hozzárendelt páciensek és események elérése, engedélyezett eseménymódosítások, saját foglalások státuszkezelése; hozzáférhető feljegyzések és chat. |
| `Patient` – páciens | Saját adatlap, elérhetőségek, események, feljegyzések és chat; saját időpont foglalása, átfoglalása és lemondása a weben. |

A részletes jogosultságokat az API minden kérésnél ellenőrzi. Megjegyzést és dokumentumot a szerző/feltöltő vagy az admin/felvételi iroda módosíthat, ha a pácienshez is van hozzáférése. Kapcsolt páciens vagy kezelő törlését az API elutasíthatja; a felület ilyenkor jelzi a fennálló kapcsolatot.

## Technológiák és projektstruktúra

| Réteg | Technológia |
| --- | --- |
| Web | React 19, TypeScript 5.9, Vite 7, Material UI, React Router, TanStack Query, Axios |
| API és alkalmazási réteg | C#, .NET 10, ASP.NET Core, ASP.NET Core Identity, MediatR, SignalR |
| Adatelérés | Entity Framework Core 10, SQL Server; külön SQLite fejlesztési profil |
| Asztali kliens | .NET 10 Windows Forms, külön `.Designer.cs` és `.resx` fájlok |
| Ellenőrzés | Saját SQL Server/API/WinForms integrációs tesztprogram, TypeScript, ESLint, Vite build |

```text
React web ─────────┐
                  ├── ASP.NET Core API ── Application / Persistence / Domain ── SQL Server
Windows Forms ────┘
```

A desktop kliensben nincs közvetlen SQL-kapcsolat: HTTP-n hívja az API-t, és nem kap adatbázis-jelszót.

```text
API/                              API-indítás, kontrollerek, hitelesítés, SignalR
Application/                      Alkalmazási műveletek és MediatR-kezelők
Domain/                           Üzleti entitások
Persistence/                      EF Core modellek és adatbázis-konfiguráció
  Migrations/                     SQLite-migrációk
  Migrations/SqlServer/            SQL Server-migrációk
client/                           Aktív React webes alkalmazás
Desktop_Pacienskezelo/             Aktív Windows Forms megoldás és projekt
IntegrationTests/                 SQL Server/API/WinForms integrációs tesztprogram
Desktop.IntegrationTests/         Korábbi SQLite desktop tesztprojekt; hiányzó függőséggel
scripts/                          API-indítás, migráció és demóadmin segédszkriptek
docs/                             Kiegészítő dokumentáció
archive/web-tutorial/              Megőrzött, az aktív frontendből kivont példakód
```

**A jelenlegi gyökérsolution korlátozása:** a `MedActivities.slnx` és a régi `Desktop.IntegrationTests` projekt a már nem szereplő `Desktop/MedActivities.Patient.Sqlite.WinForms.csproj` fájlra hivatkozik. Emiatt a teljes gyökérsolution fordítása jelenleg nem megfelelő ellenőrzőparancs. Az alábbi útmutató az aktív projektek külön fordítását használja; a WinForms saját [solutionje](Desktop_Pacienskezelo/Desktop_Pacienskezelo.slnx) a meglévő projektre mutat.

## Előfeltételek

- Windows az SQL Server LocalDB, a WinForms és az integrációs tesztprogram futtatásához.
- .NET 10 SDK.
- Visual Studio 2026, a **.NET desktop development / .NET asztali fejlesztés** munkaterheléssel, a Designer használatához.
- Node.js és npm. A repo Vite-verziójának Node-követelménye: `^20.19.0 || >=22.12.0`.
- SQL Server, SQL Server Express vagy SQL Server Express LocalDB. A helyi példák a `(localdb)\MSSQLLocalDB` példányt használják.
- PowerShell 7 a `pwsh` parancsokhoz; internetkapcsolat az első NuGet- és npm-csomagletöltéshez.

## Első indítás SQL Serverrel

Az alábbi parancsokat **PowerShellben, a repo gyökeréből** futtasd, kivéve ahol külön `client` munkakönyvtár szerepel. A repo helyi mappájának neve lehet `MedAcitivities`; a parancsok nem igényelnek konkrét meghajtót vagy felhasználónevet.

Csak a kódblokkok tartalmát másold be, a terminál `PS C:\...>` előtagját ne. A környezeti változók nevében két aláhúzás van: `__`, fordított perjel nélkül. A PowerShell `$env:...` szintaxisa Git Bashben nem működik.

### 1. Előkészítés és API-fordítás

Előbb állítsd le a korábban elindított API-t `Ctrl+C`-vel, illetve Visual Studio esetén a **Stop Debugging** paranccsal. Windows alatt a futó API zárolhatja a fordításkor cserélendő DLL-eket.

Telepített LocalDB esetén:

```powershell
SqlLocalDB start MSSQLLocalDB
dotnet dev-certs https --trust
dotnet tool restore

$env:Database__Provider = "SqlServer"
$env:ConnectionStrings__SqlServerConnection = "Server=(localdb)\MSSQLLocalDB;Database=MedActivities;Integrated Security=True;Encrypt=True;TrustServerCertificate=True"

dotnet build API/API.csproj
```

Más SQL Server-példánynál a LocalDB-indítást hagyd ki, és a saját connection stringedet add meg. A `dotnet build` a NuGet-csomagokat is helyreállítja. Csak sikeres fordítás után folytasd.

### 2. Adatbázisséma létrehozása vagy frissítése

Ugyanabban a terminálban:

```powershell
dotnet ef database update --no-build --context SqlServerDbContext --project Persistence/Persistence.csproj --startup-project API/API.csproj
```

A parancs a konfigurált `MedActivities` adatbázis migrációit alkalmazza. A `No migrations were applied. The database is already up to date.` üzenet sikeres, naprakész állapotot jelent. A `--no-build` itt az előző lépésben elkészült fordítást használja; forrásmódosítás után előbb ismét fordíts.

Az API alapbeállítás szerint nem futtat automatikus migrációt és demófeltöltést induláskor.

### 3. Demóadatok és jelszavas demóadmin

A bemutatóadatok feltöltése és darabszámaik lekérdezése:

```powershell
dotnet run --project API/API.csproj --no-build --launch-profile https -- --seed-demo-data
dotnet run --project API/API.csproj --no-build --launch-profile https -- --demo-data-status
```

A Windows Forms belépéshez állítsd be a helyi demóadmint:

```powershell
pwsh -File scripts/Configure-DemoAdmin.ps1
```

A szkript rejtetten bekéri a jelszót. Alapértelmezett fiókja `admin@example.undefined`; a jelszót te választod, nincs közös beégetett jelszó. Ugyanez a fiók a webes jelszavas belépéshez is használható. A szkript ismételt futtatása az ehhez tartozó meglévő demóadmin jelszavát újra beállítja.

Ezek a parancsok befejeződnek, nem indítanak folyamatosan futó webszervert. A demófeltöltés és a demóadmin konfigurálása csak `Development` környezetben engedélyezett; a megadott launch profile, illetve szkript ezt beállítja.

### 4. API indítása

Az adatbázis előkészítése után, ugyanebben a terminálban:

```powershell
pwsh -File scripts/Start-Api.ps1
```

A szkript fordít, majd elindítja az API-t a `https` launch profile-lal. Hagyd futni ezt a terminált. A már futó projektpéldányt a szkript jelzi; szándékos újraindításhoz használható a `-Restart` kapcsoló.

Másik PowerShell-terminálban ellenőrizd az API-t:

```powershell
Invoke-RestMethod https://localhost:5001/api/health
```

Elérhető SQL Server esetén a válasz `status: ok`, `database: SqlServer`. Ez a kapcsolat ellenőrzése; a migrációk állapotát a 2. lépés ellenőrzi.

### 5. Webes kliens indítása

Új terminálban, a repo gyökeréből:

```powershell
cd client
npm ci
npm run dev
```

Nyisd meg: **[https://localhost:3000](https://localhost:3000)**. A Vite a `/api` kéréseket, köztük a SignalR-kapcsolatot, a `https://localhost:5001` API-ra továbbítja. A webes HTTPS-t a `vite-plugin-mkcert` készíti elő; első indításkor tanúsítványtelepítésre lehet szükség.

A kezdőlapon válassz DEMÓ szerepkört, vagy jelentkezz be a 3. lépésben beállított fiókkal.

## Windows Forms és Visual Studio Designer

Nyisd meg Visual Studio 2026-ban a [Desktop_Pacienskezelo.slnx](Desktop_Pacienskezelo/Desktop_Pacienskezelo.slnx) fájlt. A startup projekt a `Desktop_Pacienskezelo`.

Parancssori fordítás és indítás a repo gyökeréből:

```powershell
dotnet build Desktop_Pacienskezelo/Desktop_Pacienskezelo/Desktop_Pacienskezelo.csproj
dotnet run --project Desktop_Pacienskezelo/Desktop_Pacienskezelo/Desktop_Pacienskezelo.csproj --no-build
```

Az API-nak már futnia kell. Az alkalmazás induláskor a `/api/health` végponton ellenőrzi az SQL Server-kapcsolatot, majd jelszavas bejelentkezést kér. Az alapértelmezett API-cím `https://localhost:5001/api`; a bejelentkezési felületen vagy indítás előtt a `MEDACTIVITIES_API_URL` környezeti változóval módosítható.

A desktop kliens `Admin`, `AdmissionsOffice` és `Practitioner` fiókokat fogad el; a páciensfiókok a webes felületet használják. A desktop felület pácienskeresést és adatlapot, páciens- és eseménykezelést, kezelőhozzárendelést, időpontkezelést, dokumentumokat és megjegyzéseket biztosít a belépett szerepkör engedélyei szerint. A kezelői munkaidő, a felhasználó-adminisztráció és az eseménychat webes felülethez tartozik.

A webes arculathoz igazított űrlapok:

- `Form1`: bejelentkezés, pácienslista, események és időpontok.
- `PatientEditForm`: páciens létrehozása és szerkesztése.
- `ActivityEditForm`: eseményadatok és hozzárendelések.
- `AppointmentForm`: foglalás és átfoglalás.
- `RecordsForm`: dokumentumok és megjegyzések.

A Solution Explorerben az adott űrlap `.cs` fájlján válaszd a **View Designer / Tervező megtekintése** parancsot. Minden felsorolt űrlaphoz külön `.Designer.cs` és `.resx` tartozik. A konstruktorok nem indítanak hálózati vagy adatbázis-műveletet, így a tervező megnyitásához nem kell futó API.

## Demóadatok és belépés

A feltöltő céldarabszámai: **100 páciens, 50 kezelő és 1000 esemény**. A meglévő rekordokat beleszámítja, a hiányzó adatokat pótolja, és nem írja felül a korábbi rekordokat. Ha valamelyik darabszám már nagyobb a célnál, nem töröl adatot. A feltöltés ismételhető.

A generált adatok fiktívek. A kezelők hétköznapi 08:00–16:00 munkaidőt és engedélyezett foglalhatóságot kapnak. Az 1000 esemény betegút-előzményt jelent; a foglalások külön, a foglalási felületen hozhatók létre.

| Webes DEMÓ gomb | Felhasználónév | Szerepkör |
| --- | --- | --- |
| ADMIN | `demo.egeszsegut.admin` | `Admin` |
| Felvételi iroda | `demo.egeszsegut.admissionsoffice` | `AdmissionsOffice` |
| Kezelőorvos | `demo.egeszsegut.practitioner` | `Practitioner` |
| Páciens | `demo.egeszsegut.patient` | `Patient` |

Ezek jelszó nélküli bemutatófiókok, a kezdőlap gombjai valódi Identity-munkamenetet hoznak létre. A végpont csak helyi, `Development` környezetben futó API-n használható. A demófiókok – a külön jelszavas desktop demóadmint is beleértve – `Production` környezetben tiltottak.

A webes munkamenet `sessionStorage`-ban tárolódik, így az oldalfrissítést túléli. A belépésváltás és kijelentkezés azonos böngészőeredeten a nyitott lapok között `BroadcastChannel` segítségével szinkronizálódik. A kijelentkezés az adott felhasználó korábban kiadott tokenjeit is érvényteleníti; ez a másik kliensben is új belépést igényelhet.

További részletek: [Demóadatok és demóbelépés](docs/DEMO-DATA.md).

## Konfiguráció és adatbázis

Az alapbeállítások az [API/appsettings.json](API/appsettings.json) fájlban találhatók. Környezeti változóval felülírhatók; a PowerShellben beállított változókat csak az abból indított folyamatok öröklik. Másik terminálban a saját beállításaidat ismét add meg, ha eltérnek az alapértékektől.

| Környezeti változó | Jelentés / alapérték |
| --- | --- |
| `Database__Provider` | `SqlServer`; másik választható érték: `Sqlite`. |
| `ConnectionStrings__SqlServerConnection` | SQL Server-kapcsolat; alapból helyi LocalDB, `MedActivities` adatbázissal és Windows-hitelesítéssel. |
| `ConnectionStrings__DefaultConnection` | SQLite-kapcsolat; alapból `Data Source=activities.db`. |
| `Database__ApplyMigrations` | Automatikus induláskori migráció; alapból `false`. |
| `Database__SeedDemoData` | Korábbi induláskori seed; alapból `false`. A 100/50/1000-es feltöltéshez a `--seed-demo-data` parancsot használd. |
| `ASPNETCORE_ENVIRONMENT` | A `https` launch profile `Development` értéket állít be. |
| `Cors__Origins__0`, `Cors__Origins__1` | Engedélyezett webes eredetek; alapból `http://localhost:3000` és `https://localhost:3000`. |
| `MEDACTIVITIES_API_URL` | WinForms API-cím; alapból `https://localhost:5001/api`. |
| `VITE_API_URL` | Webes API-alapcím; alapból `/api`. A frontend indításakor vagy fordításakor olvasódik be. |
| `BootstrapAdmin__Email`, `BootstrapAdmin__Password` | Új, nem demó adminisztrátor létrehozása az API indulásakor. |

A bootstrap csak még nem létező e-mail-címhez hoz létre admint; meglévő fiókot nem emel adminná. A jelszót a futtatási környezet titokkezelésével add át, és a létrehozás után távolítsd el a bootstrap-beállításokat. Jelszó ne kerüljön verziókezelt konfigurációba vagy a beadandóba. A helyi példában használt `TrustServerCertificate=True` helyett éles telepítéshez ellenőrizhető SQL Server-tanúsítványt használj.

### SQL Server-migrációk és törlési archívum

A SQL Server kontextusa `SqlServerDbContext`, migrációi a [Persistence/Migrations/SqlServer](Persistence/Migrations/SqlServer) mappában vannak:

- `20260913164236_SqlServerInitial`: alap üzleti és Identity-séma.
- `20260913224530_ChatAndDeletionArchive`: eseménychat és törlési archívum.

A dokumentumok legfeljebb 5 MB méretű PDF-, PNG-, JPEG- vagy UTF-8 TXT-fájlok lehetnek. Tartalmuk SQL Serveren `varbinary(max)` mezőbe kerül; nincs külön dokumentumfájlszerver.

A `DeletedRecords` tábla a támogatott üzleti táblákból törölt rekordok JSON-pillanatképét őrzi. A törlési triggerek a kapcsolatok kaszkádolt törlésekor is ugyanabban a tranzakcióban futnak. Az archívum UPDATE/DELETE műveleteit trigger tiltja; a chat/archívum migráció automatikus visszavonása szintén tiltott. Az archívumhoz nincs alkalmazásbeli visszaállító felület, és nem helyettesíti az adatbázismentést.

Segédszkript és konfigurációs minta: [Initialize-SqlServer.ps1](scripts/Initialize-SqlServer.ps1), [appsettings.SqlServer.example.json](API/appsettings.SqlServer.example.json). A szkript migrál, de a megadott kapcsolatot nem menti el tartósan az API számára.

### SQLite fejlesztési profil

A web/API SQLite-tal is indítható külön fejlesztési munkamenetben. A jelenlegi WinForms kliens ehhez a profilhoz nem használható.

Leállított API mellett, a repo gyökeréből:

```powershell
$env:Database__Provider = "Sqlite"
$env:ConnectionStrings__DefaultConnection = "Data Source=activities.db"
dotnet ef database update --context AppDbContext --project Persistence/Persistence.csproj --startup-project API/API.csproj
dotnet run --project API/API.csproj --no-build --launch-profile https
```

SQLite esetén a kontextus `AppDbContext`; a SQL Server törlési triggerei nem részei ennek a profilnak. Az SQL Serverre való visszatéréskor állítsd le az API-t, majd állítsd vissza a `Database__Provider` értékét `SqlServer`-re.

A providerváltás nem másolja át a meglévő SQLite-adatokat. Adatátvitelhez mentés, célzott import, valamint TAJ-, kapcsolat- és darabszámellenőrzés szükséges. Részletes háttér: [SQL Server útmutató](docs/SQL-SERVER.md).

## Fordítás és ellenőrzés

A build előtt állítsd le az API-t és a futó WinForms alkalmazást. A parancsokat a repo gyökeréből futtasd:

```powershell
dotnet build IntegrationTests/IntegrationTests.csproj
dotnet run --project IntegrationTests/IntegrationTests.csproj --no-build -- (Get-Location).Path
```

Az integrációs projekt az aktív API-t, az adatelérési réteget és a WinForms klienst is lefordítja. Saját konzolos tesztprogram, ezért `dotnet run` indítja, nem `dotnet test`.

A teszt telepített LocalDB-t igényel. Minden futáskor külön `MedActivities_Test_<azonosító>` adatbázist és külön helyi API-folyamatot használ; a végén a létrehozott tesztadatbázist törli. A szokásos `MedActivities` adatbázist nem célozza. Ellenőrzi többek között a CRUD-műveleteket, jogosultságokat, TAJ-egyediséget, foglalási ütközéseket, dokumentumokat, demóbelépést és ismételt adatfeltöltést, chatet, törlési archívumot és a WinForms API-kliensét.

Az űrlapokat API nélkül is példányosítja, és képeket készít róluk az `artifacts/verification` mappába. Ez kiegészíti, de nem helyettesíti a Visual Studio Designer kézi megnyitását. Sikeres futás végén `SUCCESS: ... checks passed.` jelenik meg.

Frontend-ellenőrzés:

```powershell
cd client
npm ci
npm run lint
npm run build
```

Az `npm run build` TypeScript-ellenőrzést és Vite kiadási fordítást végez; az eredmény a `client/dist` mappába kerül. A README nem helyettesít egy új környezetben elvégzett tesztfutást.

## Kiadás

A webes és API-kiadást külön kell elkészíteni:

```powershell
npm --prefix client run build
dotnet publish API/API.csproj -c Release -o artifacts/publish/API
dotnet publish Desktop_Pacienskezelo/Desktop_Pacienskezelo/Desktop_Pacienskezelo.csproj -c Release -o artifacts/publish/WinForms
```

Az API publish nem fordítja és nem csomagolja automatikusan a frontendet. Közös kiszolgáláshoz másold a `client/dist` **tartalmát** a publikált API `wwwroot` mappájába; az API statikus kiszolgálást és kliensoldali útvonalkezeléshez fallbacket biztosít. Külön webes tárhelynél add meg a megfelelő `VITE_API_URL` értéket még a frontend build előtt, és konfiguráld a CORS-eredeteket, valamint a SignalR továbbítását.

A fenti publish parancsok keretrendszerfüggő kimenetet készítenek: az API-hoz .NET 10 ASP.NET Core Runtime, a WinForms futtatásához .NET 10 Desktop Runtime szükséges. Telepítéskor külön állítsd be az SQL Server-kapcsolatot, alkalmazd a migrációkat, és használj nem demó felhasználói fiókokat.

Forráskódos beadásnál a README, a projektfájlok, a `dotnet-tools.json`, a `client/package-lock.json`, a migrációk és a szkriptek is legyenek benne a csomagban. Valós betegadat, jelszó, adatbázisfájl, mentés, `.vs`, `node_modules`, `bin`, `obj` vagy tesztartefaktum ne kerüljön a forráscsomagba.

## Hibaelhárítás

| Jelenség | Teendő |
| --- | --- |
| `MSB3021`, `MSB3027`, ismételt DLL-másolási hiba; a napló futó `API` folyamatot nevez meg | Állítsd le a futó API-t `Ctrl+C`-vel vagy Visual Studio-ban, majd fordíts újra. Friss, sikeres build után az EF-parancshoz használható a `--no-build`. |
| `bash: ... command not found` a `$env:...` soroknál | A parancsokat PowerShellben futtasd. Git Bashben más a környezeti változók szintaxisa. |
| A bemásolt `PS C:\...>` sort a shell parancsként értelmezi | Csak a parancsot másold be, a terminál promptját és a korábbi kimenetet ne. |
| `No migrations were applied. The database is already up to date.` | Sikeres eredmény: nincs alkalmazandó migráció. |
| `/api/health` nem érhető el, vagy adatbázishibát jelez | Ellenőrizd az API terminálját, a futó SQL Server-példányt, a connection stringet és a HTTPS-tanúsítványt. |
| A WinForms nem fogadja el a kapcsolatot | Az API health-válaszában `SqlServer` szerepeljen; a cím végén legyen `/api`. Jelszavas dolgozói fiókot használj. |
| „A demóbelépés most nem sikerült” / `/api/dev-session/Admin` 500 | Előbb migrálj és töltsd fel a demóadatokat, majd indítsd az API-t a `https` launch profile-lal. A 500 pontos okát az API termináljában keresd; ellenőrizd, hogy az aktuális forrásból készült build fut-e. |
| `pwsh` vagy `SqlLocalDB` nem található | Az adott eszköz nincs telepítve vagy nincs a PATH-ban. A példák PowerShell 7-et és telepített LocalDB-t feltételeznek. |
| A teljes `MedActivities.slnx` buildje hiányzó `Desktop` projektre hivatkozik | Használd a fenti aktív projektparancsokat és a külön WinForms solutiont; a gyökérsolution régi hivatkozása rendezendő. |

## Ismert korlátozások

- A hiányzó régi `Desktop` projekt miatt a gyökérsolution és a `Desktop.IntegrationTests` jelenleg nem teljes; a külön SQL Server/API/WinForms projektútvonalak használhatók.
- A repo SQLite-függőségei között `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 szerepel; a korábbi restore/build kimenet ehhez `NU1903` biztonsági figyelmeztetést jelzett. A SQL Server profil választása nem távolítja el ezt a csomagfüggőséget. A figyelmeztetés rendezéséhez külön függőségfrissítés és ellenőrzés szükséges.
- A dokumentumfeltöltés méret-, fájlnév- és alapvető tartalomellenőrzést végez; nincs beépített víruskereső.
- Nincs automatikus SQLite → SQL Server adatimport vagy felületi archívum-visszaállítás.

## Bemutatás előtti ellenőrzés

- [ ] A migráció naprakész, az API health-válasza `SqlServer`.
- [ ] A demófeltöltés lefutott; a darabszámokat a státuszparancs visszaadja.
- [ ] Mind a négy webes DEMÓ belépés és a szerepkör szerinti adatláthatóság kipróbálva.
- [ ] Páciens, esemény, időpont, dokumentum, megjegyzés és chat kipróbálva.
- [ ] A WinForms jelszavas belépése működik, és a weben módosított adat frissítés után megjelenik benne.
- [ ] Az űrlapok Visual Studio Designerben megnyithatók.
- [ ] Az aktív projektek integrációs tesztje, a frontend lint és a frontend build sikeres.
- [ ] A csomag nem tartalmaz titkokat vagy valós páciensadatokat; a gyökérsolution korlátozása rendezve vagy feltüntetve.

Kiegészítő dokumentáció: [Demóadatok](docs/DEMO-DATA.md), [SQL Server](docs/SQL-SERVER.md), [WinForms](Desktop_Pacienskezelo/README.md), [kiadási ellenőrzőlista](docs/RELEASE-CHECKLIST.md).

## Szerző

**Varga András Ernő**
