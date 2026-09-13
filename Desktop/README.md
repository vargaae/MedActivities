# EgészségÚt Windows Forms – SQLite fejlesztési csomag

A kliens a webes rendszerrel közös `Persistence.AppDbContext` és `Domain` modelleket használja. Ebben a változatban közvetlenül a helyi SQLite-fájlhoz kapcsolódik; nem szükséges hozzá futó API, ha az adatbázis már létrejött az API migrációival.

## Indítás

A repository gyökeréből:

```powershell
dotnet build MedActivities.slnx
dotnet run --project Desktop/MedActivities.Patient.Sqlite.WinForms.csproj
```

Visual Studióban a `MedActivities.Patient.Sqlite.WinForms` projektet válaszd indítási projektnek.

Az alkalmazás a projektben az `API/activities.db` fájlt keresi. Másik, az API által migrált SQLite-fájl az **Adatbázis kiválasztása** gombbal nyitható meg. A csatlakozás ellenőrzi a közös modell tábláit, mezőit és a migrációkat. Hibás fájlnál az előző működő kapcsolat marad aktív.

Ha még nincs adatbázis, először indítsd az API-t a fejlesztői konfigurációval. A táblákat a `Persistence/Migrations` migrációi hozzák létre; a desktop nem készít külön táblákat.

## Elkészült funkciók

- Páciens létrehozása, listázása, módosítása és törlése.
- Kötelező, egyedi TAJ-szám: pontosan 9 ASCII számjegy, kezdő nulla megőrzésével. Ez az API jelenlegi formátumellenőrzése, nem ellenőrzőszám-vizsgálat.
- Keresés név, TAJ, e-mail, telefon és lakcím alapján.
- Név-, dátum-, e-mail- és mezőhossz-ellenőrzés a felületen és az adatkezelőben.
- Hibás mentéskor az űrlap nyitva marad a kitöltött adatokkal.
- Események kezelése `PatientActivities` pácienskapcsolatokkal és `ActivityPractitioners` kezelőkapcsolatokkal.
- Új eseményhez pontosan egy páciens és legalább egy meglévő kezelőorvos szükséges.
- Kezelőválasztás és az API-val azonos eseménystátuszok.
- A páciens felhasználói kapcsolata és létrehozási ideje szerkesztéskor megmarad; az esemény orvosi megjegyzését sem írja felül a desktop.
- Tranzakciók és bekapcsolt idegenkulcs-ellenőrzés.
- Eseményhez vagy foglaláshoz kapcsolt páciens nem törölhető.

Foglalási esemény közvetlen módosítása és törlése tiltott: ezt a webes időpontkezelőben kell elvégezni, ahol az időpontütközés és státuszszinkron ellenőrzése működik. Több pácienshez kapcsolt régi esemény hozzárendelése szintén a webes felületen rendezhető.

## Régi desktop-kapcsolatok

Ha egy egyébként kompatibilis API-adatbázisban a régi desktop `Activities.PatientId` oszlopa kitöltött értékeket tartalmaz:

1. Csatlakozáskor SQLite online biztonsági másolat készül az adatbázis mellé: `activities.db.desktop-links-<azonosító>.db`.
2. A létező páciensre mutató, nem ütköző kapcsolat bekerül a `PatientActivities` táblába.
3. Csak a sikeres tranzakció végén ürül a régi oszlop. Az oszlop megmarad, de az új kliens már nem használja.
4. Ütköző kapcsolat vagy hiányzó páciens esetén a teljes átemelés visszagördül; a hibaüzenetben megjelenik a biztonsági másolat helye.

Az átemelés ismételt csatlakozáskor nem hoz létre duplikált vagy korábban eltávolított kapcsolatot. A mentés a SQLite [BackupDatabase](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/backup) eljárásával készül.

A régi, önálló, TAJ és API-táblák nélküli desktop-adatbázist ez a csomag nem alakítja át automatikusan. Változatlanul hagyja, mert a hiányzó TAJ-számokat és kezelőhozzárendeléseket nem lehet kitalálni. Az ilyen adatok átviteléhez külön adatpótlás és import szükséges.

## Ellenőrzések

```powershell
dotnet run --project Desktop.IntegrationTests/Desktop.IntegrationTests.csproj
```

A csomag külön, a valódi EF-migrációkkal létrehozott tesztadatbázisokon ellenőrzi a CRUD-ot, TAJ-validációt és egyediséget, kapcsolatokat, törlési tiltásokat, foglalások védelmét, régi kapcsolatok átemelését és visszagörgetését. A formok létrehozását, néhány elrendezési feltételt és bitmap-renderét is ellenőrzi; ez nem teljes interaktív felületteszt.

A tesztprogram a bin könyvtárán belüli `TestResults/<azonosító>` mappában hagyja a tesztadatbázisokat és a formképeket. A felhasználói `API/activities.db` fájlt nem nyitja meg. Siker: 0 kilépési kód.

## Következő csomag

A közvetlen helyi kliensben még nincs bejelentkezés és szerepkörkezelés. A tervezett API-üzemmód hozza majd a közös hitelesítést; az MS SQL Serverre átállás külön fejlesztési lépés. A meglévő NuGet-biztonsági figyelmeztetéseket ez a csomag nem szünteti meg.
