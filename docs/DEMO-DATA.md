# Demóadatok és demóbelépés

A helyi SQL Server céldarabszámai: **100 páciens, 50 kezelő, 1000 esemény**.
A feltöltő a meglévő, más forrásból származó rekordokat is beleszámítja. Ezeket nem módosítja;
csak a hiányzó demórekordokat hozza létre, stabil azonosítóval, egy tranzakcióban.
Ha már több adat van valamelyik céldarabszámnál, nem töröl rekordot a szám csökkentéséhez.

A fiktív páciensekhez események, kezelői hozzáférések, a kezelőkhöz hétköznapi
08:00–16:00 munkaidő és bekapcsolt foglalási beállítás tartozik. Az események
betegút-előzmények; új időpontok a foglalási felületen hozhatók létre.

## Feltöltés és ellenőrzés

A projekt gyökeréből, PowerShellben és Git Bashben egyaránt:

```text
dotnet run --project API/API.csproj --launch-profile https -- --seed-demo-data
dotnet run --project API/API.csproj --launch-profile https -- --demo-data-status
```

A konfigurált adatbázison futnak; az alapérték `(localdb)\MSSQLLocalDB`, `MedActivities`.
A feltöltéshez előbb alkalmazni kell a migrációkat. A `--seed-demo-data` csak Development
környezetben engedélyezett. A parancs befejeződik, nem indít HTTP-szervert, ezért
nem foglalja el az API portját. Újrafuttatáskor megmaradnak a demóadatokon végzett módosítások.

## Webes demóbelépés

Indítás:

```text
dotnet run --project API/API.csproj --launch-profile https
```

A `client` mappában futtasd az `npm run dev` parancsot. A kezdőlap négy DEMÓ gombja
az alábbi valódi Identity-fiókokkal jelentkezik be:

| Gomb | Felhasználónév | Szerepkör |
|---|---|---|
| ADMIN | demo.egeszsegut.admin | Admin |
| Felvételi iroda | demo.egeszsegut.admissionsoffice | AdmissionsOffice |
| Kezelőorvos | demo.egeszsegut.practitioner | Practitioner |
| Páciens | demo.egeszsegut.patient | Patient |

A gombos belépéshez nem kell jelszó. Ezek jelszó nélküli bemutatófiókok;
a hagyományos webes és WinForms jelszavas belépéshez saját admin/dolgozói fiókot használj.
A demófiókok mindegyike egyedi `@demo.example.invalid` e-mail-címet kap.

A kezelő a hozzá rendelt betegeket/eseményeket, a páciens a saját adatlapját és eseményeit látja.
A demóvégpont csak Development környezetben, helyi kérésből használható.
Production módban a végpont és a korábban kiadott demómunkamenetek is tiltottak.

## Javított hiba

Az Identity `RequireUniqueEmail=true` beállítása mellett az e-mail nélküli
demófiók létrehozása meghiúsult, ezért a `/api/dev-session/Admin` 500-at adott.
A javítás egyedi fiktív e-mail-címeket rendel a fiókokhoz, és a régi, e-mail nélküli
demófiókokat is kiegészíti. A kezdőlap külön üzenettel jelzi a szerverhibát
és a fejlesztői demóbelépés elérhetetlenségét.

Az `IntegrationTests` ellenőrzi a négy belépést, szerepköröket, adatláthatóságot,
a kétszer lefuttatott feltöltés darabszámait, kapcsolatait és a production tiltást.
