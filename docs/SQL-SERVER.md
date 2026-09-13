# SQL Server telepítési útmutató

Ez a projekt két adatbázis-profilt támogat. A végleges beadási profil a SqlServer, a Sqlite csak helyi fejlesztéshez és a korábbi adatok ellenőrzéséhez marad.

## 1. SQL Server előkészítése

Használható SQL Server, SQL Server Express vagy LocalDB. LocalDB esetén a fejlesztői alapértelmezés:

~~~text
Server=(localdb)\\MSSQLLocalDB;Database=MedActivities;Integrated Security=True;Encrypt=True;TrustServerCertificate=True
~~~

Másik példány esetén a connection stringet környezeti változóban add meg:

~~~powershell
$env:Database__Provider = "SqlServer"
$env:ConnectionStrings__SqlServerConnection = "Server=.\\SQLEXPRESS;Database=MedActivities;Integrated Security=True;Encrypt=True;TrustServerCertificate=True"
~~~

A connection stringbe ne kerüljön verziókezelt jelszó. SQL-hitelesítéshez használj User Secrets-t vagy a futtatási környezet titkos változókezelését.

## 2. Sémalétrehozás

A migrációt a repository gyökeréből kell futtatni:

~~~powershell
dotnet tool restore
dotnet ef database update --context SqlServerDbContext --project Persistence/Persistence.csproj --startup-project API/API.csproj
~~~

A célzott SQL Server migrationök itt vannak:

- Persistence/Migrations/SqlServer/20260913164236_SqlServerInitial.cs
- Persistence/Migrations/SqlServer/SqlServerDbContextModelSnapshot.cs

A migrationek tartalmazzák az Identity táblákat, pácienseket, kezelőket, eseményeket, kapcsolatokat, időpontokat, megjegyzéseket és dokumentumokat. A dokumentumtartalom SQL Serveren varbinary(max).

Ellenőrzés:

~~~powershell
dotnet ef migrations list --context SqlServerDbContext --project Persistence/Persistence.csproj --startup-project API/API.csproj
~~~

Az API alapból nem futtat migrationt induláskor. Konténeres vagy CI környezetben külön adatbázis-release lépésben futtasd az update-et; csak átmeneti fejlesztési környezetben állítsd Database__ApplyMigrations=true értékre.

## 3. Első admin és szerepkörök

Az API minden induláskor létrehozza a hiányzó szerepköröket. Új admin csak akkor jön létre, ha a megadott e-mail-cím még nem létezik:

~~~powershell
$env:BootstrapAdmin__Email = "admin@example.invalid"
$env:BootstrapAdmin__Password = "Erős-egyszeri-jelszó"
dotnet run --project API/API.csproj
Remove-Item Env:BootstrapAdmin__Email,Env:BootstrapAdmin__Password
~~~

A bootstrap értékeket a futtatási folyamat végén töröld a PowerShell-munkamenetből. A demo seed alapból ki van kapcsolva, mert induláskor meglévő üzemi adatokat módosítana.

## 4. API és web

~~~powershell
dotnet run --project API/API.csproj
~~~

Másik terminálban:

~~~powershell
cd client
npm install
npm run dev
~~~

Fejlesztéskor a Vite /api proxyja az API-ra mutat. Beadási vagy külön szerveres kiadásnál a CORS Cors:Origins értékét a tényleges webcímre szűkítsd.

Az egészség-ellenőrzés: GET /api/health. Elérhető adatbázis esetén a válasz database: SqlServer értéket tartalmaz.

## 5. WinForms

Nyisd meg a Desktop_Pacienskezelo/Desktop_Pacienskezelo.slnx fájlt Visual Studio 2026-ban. A kliens alapértelmezett API-címe https://localhost:5001/api; ezt a login ablakban vagy a MEDACTIVITIES_API_URL változóval lehet felülírni.

A kliens nem ismeri és nem menti az SQL connection stringet. A hitelesített API-n keresztül kezeli a pácienseket és TAJ-számot, az eseményeket és kezelőkapcsolatokat, az időpontokat, valamint a dokumentumokat és megjegyzéseket.

A Designer megnyitásához a form .cs fájlján válaszd a View Designer parancsot. A vezérlők statikus .Designer.cs fájlban vannak; a konstruktor nem indít hálózati vagy adatbázis-műveletet.

## 6. SQLite-ról való átállás előtt

A projekt nem módosítja automatikusan a meglévő SQLite-adatbázist. Átállás előtt:

1. készíts fájl- és adatbázismentést;
2. futtasd a régi SQLite integrációs teszteket;
3. ellenőrizd az egyedi TAJ-kat, hiányzó UserId-kat és kapcsolatokat;
4. alkalmazd az SQL Server migrationöket üres vagy külön staging adatbázison;
5. célzott importtal másold át az adatokat és ellenőrizd a darabszámokat;
6. csak validáció után váltsd át a klienseket az SQL Server API-címére.

A céladatbázist automatikusan törlő importot ne futtass. A jelenlegi tesztprogram minden futáskor új, MedActivities_Test_ előtagú LocalDB-adatbázist használ, és csak azt törli.
