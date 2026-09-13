# MedActivities kiadási ellenőrzőlista

## Fordítás és adatbázis

- dotnet restore
- dotnet build MedActivities.slnx
- SQL Server migration alkalmazva staging adatbázisra
- GET /api/health válasza SqlServer
- nincs pending EF migration
- connection string és bootstrap jelszó nem kerül ZIP-be

## Funkciók

- Admin belépés
- páciens létrehozás / listázás / szerkesztés / törlés
- TAJ: 9 ASCII számjegy, kezdő nulla és duplikáció ellenőrzése
- kezelő és munkaidő
- esemény és páciens–kezelő hozzárendelés
- időpont foglalás, átfoglalás, lemondás, státusz, törlés
- dokumentum feltöltés, letöltés, cím módosítás, törlés
- megjegyzés létrehozás, szerkesztés, törlés
- saját páciens / más páciens hozzáférési teszt
- WinForms belépés és adatfrissítés
- Visual Studio Designer megnyitás

## Parancsok

~~~powershell
dotnet run --project IntegrationTests/IntegrationTests.csproj -- (Get-Location).Path
dotnet run --project Desktop.IntegrationTests/Desktop.IntegrationTests.csproj
cd client
npm run build
~~~
