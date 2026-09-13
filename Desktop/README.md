# Windows Forms kliensek

A megoldásban két kliens található:

- Desktop_Pacienskezelo/Desktop_Pacienskezelo/ – az új, beadásra szánt API/SQL Server kliens.
- Desktop/ – korábbi közvetlen SQLite fejlesztői kliens, amely a régi adatfájl-kompatibilitás és regressziós ellenőrzés miatt megmaradt.

## Új SQL Server/API kliens

Visual Studio 2026-ban nyisd meg:

~~~text
Desktop_Pacienskezelo/Desktop_Pacienskezelo.slnx
~~~

A projekt a webes API-n keresztül kapcsolódik. Az API-cím alapértéke:

~~~text
https://localhost:5001/api
~~~

A MEDACTIVITIES_API_URL környezeti változóval másik cím adható meg. Az alkalmazás induláskor nem olvas SQL-adatbázist; a bejelentkezés után tokenes API-kapcsolatot használ. Az SQL Server connection string kizárólag az API konfigurációjában van.

Elérhető funkciók:

- páciens CRUD és TAJ szerinti keresés;
- esemény CRUD páciens- és kezelőhozzárendeléssel;
- időpont foglalás, átfoglalás, lemondás, státusz és jogosultság szerinti törlés;
- dokumentum feltöltés, letöltés, átnevezés és törlés;
- megjegyzés létrehozás, szerkesztés és törlés.

A Form1.cs, PatientEditForm.cs, ActivityEditForm.cs, AppointmentForm.cs és RecordsForm.cs fájlokhoz külön Designer.cs és resx tartozik. A formok konstruktora csak az InitializeComponent() hívást végzi, így a Visual Studio Designerben megnyithatók.

Fordítás:

~~~powershell
dotnet build Desktop_Pacienskezelo/Desktop_Pacienskezelo/Desktop_Pacienskezelo.csproj
dotnet run --project Desktop_Pacienskezelo/Desktop_Pacienskezelo/Desktop_Pacienskezelo.csproj
~~~

## Korábbi SQLite kliens

Ez a kliens közvetlenül az API által migrált SQLite fájlt használja. A használata csak fejlesztési/kompatibilitási célra javasolt:

~~~powershell
dotnet run --project Desktop/MedActivities.Patient.Sqlite.WinForms.csproj
~~~

A kliens ellenőrzi a szükséges táblákat és migrationöket, megőrzi a kezdő nullás TAJ-számot, és a régi Activities.PatientId kapcsolatokat külön biztonsági másolat után képes átemelni a kanonikus kapcsolótáblába. Önálló, hiányos régi adatbázist nem alakít át automatikusan.

Regressziós ellenőrzés:

~~~powershell
dotnet run --project Desktop.IntegrationTests/Desktop.IntegrationTests.csproj
~~~

A teszt csak saját, véletlen nevű SQLite-fájlokat használ, a felhasználó API/activities.db fájlját nem nyitja meg.
