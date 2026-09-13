# EgészségÚt / MedActivities

Egészségügyi esemény-, páciens- és időpontkezelő rendszer, amely közös REST API-n keresztül támogatja a páciensek, kezelőorvosok és egészségügyi adminisztrátorok munkáját webes és Windows Forms kliensből.

## Projektállapot

Elkészült vagy részben elkészült:

- ASP.NET Core Web API .NET 10 alapon
- Domain, Application és Persistence rétegek
- ASP.NET Identity alapú autentikáció és szerepkörkezelés
- Páciens, kezelőorvos és egészségügyi esemény CRUD API
- Páciens–orvos hozzáférések, munkaidő és foglalási beállítások
- Időpontfoglalás, szabad időpontok és lemondás
- React + TypeScript webes kliens és adminisztrációs oldalak
- SQLite alapú fejlesztői adatbázis és EF Core migrationök

Következő feladatok:

- Időpont-módosítás és teljes időpont CRUD lezárása
- Dokumentum- és megjegyzéskezelés teljes CRUD-ja
- React felületek egységesítése
- Windows Forms kliens létrehozása
- Integrációs tesztek
- SQLite kiváltása Microsoft SQL Serverrel

## Fő funkciók

### Páciens

- Regisztráció és bejelentkezés
- Saját profil kezelése
- Egészségügyi események megtekintése
- Időpontfoglalás és lemondás
- Dokumentumok és megjegyzések megtekintése

### Kezelőorvos és egészségügyi dolgozó

- Páciensek, kezelőorvosok és események kezelése
- Páciens- és kezelőhozzárendelések kezelése
- Munkaidő és foglalási szabályok kezelése
- Időpontok és státuszok kezelése
- Dokumentumok és megjegyzések kezelése

## Technológiák

- Backend: ASP.NET Core Web API, .NET 10, Entity Framework Core 10, ASP.NET Identity, MediatR, AutoMapper
- Web: React 19, TypeScript, Vite, React Router, React Query, React Hook Form, Zod, Material UI
- Desktop: Windows Forms, .NET 10
- Adatbázis: jelenleg SQLite, a végleges cél Microsoft SQL Server

## Architektúra

```text
React webes kliens ─────┐
                        ├── ASP.NET Core Web API ─── Persistence ─── Adatbázis
Windows Forms kliens ───┘              │
                                      └── Domain / Application
```

A kliensek nem közvetlenül kapcsolódnak az adatbázishoz. Az autentikáció, jogosultság-ellenőrzés és üzleti logika az API-ban található.

## Projektstruktúra

```text
API/          ASP.NET Core Web API és kontrollerek
Application/  Use case-ek, DTO-k, lekérdezések és parancsok
Domain/       Entitások és domain típusok
Persistence/  EF Core DbContext, Identity és migrationök
client/       React webes kliens
client-dev/   Fejlesztői frontend változat
Desktop/      Tervezett Windows Forms kliens
```

## Szükséges környezet

- .NET 10 SDK
- Node.js és npm
- Visual Studio 2022 vagy újabb
- Microsoft SQL Server
- SQL Server Management Studio vagy Azure Data Studio

## Indítás fejlesztői SQLite-tal

API:

```powershell
dotnet restore
dotnet build
dotnet run --project API/API.csproj
```

React kliens:

```powershell
cd client
npm install
npm run dev
```

A fejlesztői SQLite connection string az `API/appsettings.Development.json` fájlban található; az adatbázis neve `activities.db`.

## EF Core migrationök

```powershell
dotnet ef database update --project Persistence/Persistence.csproj --startup-project API/API.csproj
dotnet ef migrations add MigrationName --project Persistence/Persistence.csproj --startup-project API/API.csproj
```

## Windows Forms kliens

A WinForms projekt a következő fejlesztési ütemben kerül a solutionbe. Az alkalmazás az API-t használja, és a következő képernyőket tartalmazza:

1. Bejelentkezés
2. Főmenü/dashboard
3. Páciensek kezelése
4. Kezelőorvosok kezelése
5. Egészségügyi események kezelése
6. Időpontok és státuszok kezelése
7. Dokumentumok és megjegyzések

## MS SQL Serverre átállás

Az átállás a CRUD-funkciók és a WinForms kliens stabilizálása után történik:

1. `Microsoft.EntityFrameworkCore.SqlServer` hozzáadása.
2. SQLite provider és SQLite-specifikus kódok eltávolítása.
3. `UseSqlite` lecserélése `UseSqlServer` hívásra.
4. SQL Server connection string beállítása.
5. SQL Serverhez illeszkedő migrationök létrehozása.
6. Adatbázis, seed adatok és szerepkörök ellenőrzése.
7. API-, React- és WinForms-integrációs tesztelés.

Példa:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MedActivities;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Connection stringet ne tölts fel verziókezelésbe; használj User Secrets-t vagy környezeti változót.

## 12 órás befejezési sorrend

1. API CRUD-kiegészítések és validációk
2. Időpontkezelés lezárása
3. Dokumentum- és megjegyzéskezelés
4. React felületek véglegesítése
5. Windows Forms projekt, bejelentkezés és navigáció
6. WinForms páciens-, orvos-, esemény- és időpontkezelés
7. Integrációs tesztelés
8. MS SQL Server provider, konfiguráció és migrationök
9. Regressziós teszt és dokumentációfrissítés

## Kiadás előtti ellenőrzés

- `dotnet build` és React production build sikeres
- Minden CRUD végpont jogosultságot és bemenetet ellenőriz
- Kapcsolt rekordok törlése megfelelően kezelt
- Foglalási ütközések tesztelve
- A webes és a WinForms kliens ugyanazt az API-t használja
- SQL Serveren minden migration sikeresen lefut
- Tesztfelhasználók és szerepkörök dokumentálva vannak

## Fejlesztési konvenciók

Commit formátum:

```text
MED-XX type: rövid leírás
```

Típusok: `feat`, `fix`, `refactor`, `test`, `docs`, `chore`.

## Készítő

**Varga András Ernő**
