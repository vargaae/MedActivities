using System.ComponentModel.DataAnnotations;
using System.Drawing.Imaging;
using Domain;
using MedActivities.Patient.Sqlite.WinForms;
using MedActivities.Patient.Sqlite.WinForms.Models;
using MedActivities.Patient.Sqlite.WinForms.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Identity;

internal static class Program
{
    private static int passed;
    private static int failed;
    private static readonly string Results = Path.Combine(AppContext.BaseDirectory, "TestResults", Guid.NewGuid().ToString("N"));

    [STAThread]
    private static int Main()
    {
        Directory.CreateDirectory(Results);
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Run("API migrations and connection without schema drift", Schema);
        Run("Patient CRUD, leading-zero TAJ, restart and protected fields", PatientCrud);
        Run("TAJ validation, duplicate insert/update and invalid birth date", Validation);
        Run("Activity CRUD and API-visible patient/practitioner relationships", ActivityCrud);
        Run("Invalid assignments roll back without orphan activities", InvalidAssignments);
        Run("Linked patients cannot be deleted; unlinked grants cascade", DeleteRules);
        Run("Appointments are readable but protected from direct edits/deletes", AppointmentRules);
        Run("Legacy links imported once with a restorable backup", LegacyImport);
        Run("Conflicting legacy links roll back all changes", LegacyConflict);
        Run("Dangling legacy links are rejected", LegacyDangling);
        Run("Incompatible databases stay unchanged; previous connection survives", IncompatibleDatabase);
        Run("Missing files are not created", MissingDatabase);
        // Last: form construction may install a WindowsForms synchronization context.
        try { FormLayout(); passed++; Console.WriteLine("PASS: WinForms construction, TAJ field and layout renders"); }
        catch (Exception ex) { failed++; Console.WriteLine("FAIL: WinForms layout: " + ex); }
        Console.WriteLine($"Result: {passed} passed, {failed} failed. Test artifacts: {Results}");
        return failed == 0 ? 0 : 1;
    }

    private static void Run(string name, Func<Task> test)
    {
        try { test().GetAwaiter().GetResult(); passed++; Console.WriteLine("PASS: " + name); }
        catch (Exception ex) { failed++; Console.WriteLine("FAIL: " + name + Environment.NewLine + ex); }
    }

    private static async Task<Fixture> FixtureAsync()
    {
        var path = Path.Combine(Results, Guid.NewGuid().ToString("N") + ".db");
        var fixture = new Fixture(path);
        await using var db = fixture.Db(create: true);
        await db.Database.MigrateAsync(); // Real Persistence migrations, never a hand-written test schema.
        db.Users.AddRange(new AppUser { Id = "doctor-user-1", UserName = "doctor1" },
            new AppUser { Id = "doctor-user-2", UserName = "doctor2" },
            new AppUser { Id = "patient-user", UserName = "patient" });
        db.Practitioners.AddRange(
            new PractitionerProfile { Id = "doctor-1", UserId = "doctor-user-1", Name = "Teszt Orvos Egy", TajNumber = "900000001" },
            new PractitionerProfile { Id = "doctor-2", UserId = "doctor-user-2", Name = "Teszt Orvos Kettő", TajNumber = "900000002" });
        await db.SaveChangesAsync();
        await fixture.Service.ConnectAsync(path);
        return fixture;
    }

    private static PatientRecord Patient(string taj = "012345678") => new()
    {
        Name = "Teszt Páciens", TajNumber = taj, BirthDate = new DateTime(1990, 2, 3)
    };

    private static ActivityRecord ActivityFor(string patientId) => new()
    {
        PatientId = patientId, Title = "Teszt vizsgálat", Description = "Vizsgálat leírása",
        Category = "Vizsgálat", City = "Budapest", Venue = "Teszt rendelő",
        Date = DateTime.Today.AddDays(1).AddHours(10), PractitionerIds = ["doctor-1"]
    };

    private static async Task Schema()
    {
        var f = await FixtureAsync();
        var before = await f.Scalar("SELECT group_concat(sql) FROM sqlite_master;");
        Check(await f.Service.ConnectAsync(f.Path) is null, "No legacy backup needed");
        Equal(before, await f.Scalar("SELECT group_concat(sql) FROM sqlite_master;"), "Connection must not change schema");
        Equal(0L, await f.Scalar("SELECT COUNT(*) FROM pragma_table_info('Activities') WHERE name='PatientId';"), "No parallel foreign key");
        Equal(1L, await f.Scalar("PRAGMA foreign_keys;"), "Foreign keys enabled");
        Equal(0, (await f.Service.GetPatientsAsync()).Count, "Empty list");
    }

    private static async Task PatientCrud()
    {
        var f = await FixtureAsync();
        var p = Patient();
        await f.Service.InsertPatientAsync(p);
        DateTime createdAt;
        await using (var db = f.Db())
        {
            var entity = await db.Patients.SingleAsync();
            Equal("012345678", entity.TajNumber, "TAJ leading zero");
            Check(entity.Email is null && entity.Phone is null, "Empty optional values stored as NULL");
            entity.UserId = "patient-user";
            createdAt = entity.CreatedAt;
            Check(createdAt > DateTime.UtcNow.AddMinutes(-1), "CreatedAt populated");
            await db.SaveChangesAsync();
        }
        p.Name = "Módosított Páciens"; p.Email = "test@example.test";
        await f.Service.UpdatePatientAsync(p);
        var reconnect = new SqliteDatabaseService();
        await reconnect.ConnectAsync(f.Path);
        Equal(p.Id, (await reconnect.GetPatientsAsync("012345")).Single().Id, "Search by TAJ after restart");
        Equal(0, (await reconnect.GetPatientsAsync("' OR 1=1 --")).Count, "Search is parameterized");
        Equal(0, (await reconnect.GetPatientsAsync("%")).Count, "Search wildcard treated literally");
        await using (var db = f.Db())
        {
            var entity = await db.Patients.SingleAsync();
            Equal("patient-user", entity.UserId, "UserId preserved");
            Equal(createdAt, entity.CreatedAt, "CreatedAt preserved");
            Equal(p.Name, entity.Name, "API model reads desktop update");
        }
        await reconnect.DeletePatientAsync(p.Id);
        Equal(0, (await reconnect.GetPatientsAsync()).Count, "Delete persisted");
        await using var check = f.Db();
        Check(await check.Users.AnyAsync(u => u.Id == "patient-user"), "Patient delete preserves Identity account");
    }

    private static async Task Validation()
    {
        var f = await FixtureAsync();
        foreach (var taj in new[] { "", "12345678", "1234567890", "1234x6789", "１２３４５６７８９" })
            await Throws<ValidationException>(() => f.Service.InsertPatientAsync(Patient(taj)));
        var invalidDate = Patient(); invalidDate.BirthDate = DateTime.Today.AddDays(1);
        await Throws<ValidationException>(() => f.Service.InsertPatientAsync(invalidDate));
        invalidDate.BirthDate = default;
        await Throws<ValidationException>(() => f.Service.InsertPatientAsync(invalidDate));
        var invalidEmail = Patient(); invalidEmail.Email = "invalid";
        await Throws<ValidationException>(() => f.Service.InsertPatientAsync(invalidEmail));
        var blankName = Patient(); blankName.Name = " ";
        await Throws<ValidationException>(() => f.Service.InsertPatientAsync(blankName));
        var p1 = Patient(); var p2 = Patient("112345678");
        await f.Service.InsertPatientAsync(p1); await f.Service.InsertPatientAsync(p2);
        await Throws<InvalidOperationException>(() => f.Service.InsertPatientAsync(Patient()), "TAJ");
        p2.Name = "Must not persist"; p2.TajNumber = p1.TajNumber;
        await Throws<InvalidOperationException>(() => f.Service.UpdatePatientAsync(p2), "TAJ");
        var saved = (await f.Service.GetPatientsAsync("112345678")).Single();
        Equal("Teszt Páciens", saved.Name, "Duplicate update did not partially persist");
        Equal(2, (await f.Service.GetPatientsAsync()).Count, "Invalid records not inserted");
    }

    private static async Task ActivityCrud()
    {
        var f = await FixtureAsync(); var p = Patient(); await f.Service.InsertPatientAsync(p);
        var a = ActivityFor(p.Id);
        await f.Service.InsertActivityAsync(a);
        await using (var db = f.Db())
        {
            var saved = await db.Activities.Include(x => x.PatientActivities).Include(x => x.ActivityPractitioners).SingleAsync();
            Equal(p.Id, saved.PatientActivities.Single().PatientId, "Canonical patient link");
            Equal("doctor-1", saved.ActivityPractitioners.Single().PractitionerId, "Canonical doctor link");
            saved.MedicalNotes = "API-only note"; saved.CreatedByUserId = "doctor-user-1";
            await db.SaveChangesAsync();
        }
        var loaded = (await f.Service.GetActivitiesForPatientAsync(p.Id)).Single();
        Equal("doctor-1", loaded.PractitionerIds.Single(), "Desktop reads API relationships");
        a.Title = "Módosított vizsgálat"; a.Status = "Cancelled"; a.PractitionerIds = ["doctor-2"];
        await f.Service.UpdateActivityAsync(a);
        await using (var db = f.Db())
        {
            var saved = await db.Activities.Include(x => x.ActivityPractitioners).SingleAsync();
            Check(saved.IsCancelled && saved.Status == "Cancelled", "Cancellation fields consistent");
            Equal("doctor-2", saved.ActivityPractitioners.Single().PractitionerId, "Doctor assignment replaced");
            Equal("API-only note", saved.MedicalNotes, "MedicalNotes preserved");
            Equal("doctor-user-1", saved.CreatedByUserId, "Creator preserved");
            Check(saved.UpdatedAt.HasValue, "Update timestamp set");
            saved.Title = "Webes módosítás"; await db.SaveChangesAsync();
        }
        Equal("Webes módosítás", (await f.Service.GetActivitiesForPatientAsync(p.Id)).Single().Title, "Desktop reads web changes");
        await f.Service.DeleteActivityAsync(a.Id, p.Id);
        await using var final = f.Db();
        Check(!await final.Activities.AnyAsync() && !await final.PatientActivities.AnyAsync() &&
            !await final.ActivityPractitioners.AnyAsync(), "Delete cascades relationships");
        await Throws<InvalidOperationException>(() => f.Service.UpdateActivityAsync(a));
    }

    private static async Task InvalidAssignments()
    {
        var f = await FixtureAsync(); var p = Patient(); await f.Service.InsertPatientAsync(p);
        var a = ActivityFor(p.Id); a.PractitionerIds = [];
        await Throws<ValidationException>(() => f.Service.InsertActivityAsync(a));
        a.PractitionerIds = ["missing-doctor"];
        await Throws<InvalidOperationException>(() => f.Service.InsertActivityAsync(a));
        a.PractitionerIds = ["doctor-1"]; a.PatientId = "missing-patient";
        await Throws<InvalidOperationException>(() => f.Service.InsertActivityAsync(a));
        a.PatientId = p.Id; a.Status = "invalid";
        await Throws<ValidationException>(() => f.Service.InsertActivityAsync(a));
        await using var db = f.Db();
        Equal(0, await db.Activities.CountAsync(), "No orphan activities");
        Equal(0, await db.PatientActivities.CountAsync(), "No orphan links");
    }

    private static async Task DeleteRules()
    {
        var f = await FixtureAsync(); var p = Patient(); await f.Service.InsertPatientAsync(p);
        var a = ActivityFor(p.Id); await f.Service.InsertActivityAsync(a);
        await Throws<InvalidOperationException>(() => f.Service.DeletePatientAsync(p.Id), "kapcsolt");
        Equal(1, (await f.Service.GetActivitiesForPatientAsync(p.Id)).Count, "Blocked delete keeps relationship");
        var p2 = Patient("112345678"); await f.Service.InsertPatientAsync(p2);
        await using (var db = f.Db())
        {
            db.PatientActivities.Add(new() { PatientId = p2.Id, ActivityId = a.Id });
            await db.SaveChangesAsync();
        }
        await Throws<InvalidOperationException>(() => f.Service.DeleteActivityAsync(a.Id, p.Id), "több páciens");
        await Throws<InvalidOperationException>(() => f.Service.UpdateActivityAsync(a), "több páciens");
        await using (var db = f.Db())
        {
            db.PatientActivities.Remove(await db.PatientActivities.SingleAsync(x => x.PatientId == p2.Id));
            db.PatientPractitionerAccesses.Add(new() { PatientId = p.Id, PractitionerId = "doctor-1" });
            await db.SaveChangesAsync();
        }
        await f.Service.DeleteActivityAsync(a.Id, p.Id);
        await f.Service.DeletePatientAsync(p.Id);
        await using var final = f.Db();
        Equal(0, await final.PatientPractitionerAccesses.CountAsync(), "Access links cascade");
    }

    private static async Task AppointmentRules()
    {
        var f = await FixtureAsync(); var p = Patient(); await f.Service.InsertPatientAsync(p);
        var a = ActivityFor(p.Id); await f.Service.InsertActivityAsync(a);
        await using (var db = f.Db())
        {
            db.Appointments.Add(new()
            {
                PatientId = p.Id, PractitionerId = "doctor-1", ActivityId = a.Id,
                BookingDate = DateOnly.FromDateTime(a.Date), StartTime = a.Date, EndTime = a.Date.AddHours(1)
            });
            await db.SaveChangesAsync();
        }
        Check((await f.Service.GetActivitiesForPatientAsync(p.Id)).Single().IsAppointment, "Appointment flagged in list");
        await Throws<InvalidOperationException>(() => f.Service.DeletePatientAsync(p.Id));
        a.Date = a.Date.AddHours(1);
        await Throws<InvalidOperationException>(() => f.Service.UpdateActivityAsync(a), "Foglalási");
        await Throws<InvalidOperationException>(() => f.Service.DeleteActivityAsync(a.Id, p.Id), "Foglalási");
        await using var check = f.Db();
        Equal(a.Date.AddHours(-1), (await check.Appointments.SingleAsync()).StartTime, "Booking unchanged");
        Equal(a.Date.AddHours(-1), (await check.Activities.SingleAsync()).Date, "Activity unchanged");
    }

    private static async Task LegacyImport()
    {
        var f = await FixtureAsync(); var p = Patient(); await f.Service.InsertPatientAsync(p);
        var a = ActivityFor(p.Id); await f.Service.InsertActivityAsync(a);
        await using (var db = f.Db())
        {
            db.PatientActivities.RemoveRange(db.PatientActivities);
            await db.SaveChangesAsync();
            await db.Database.ExecuteSqlRawAsync("ALTER TABLE Activities ADD COLUMN PatientId TEXT NULL;");
            await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE Activities SET PatientId={p.Id} WHERE Id={a.Id};");
        }
        var backup = await f.Service.ConnectAsync(f.Path);
        Check(backup is not null && File.Exists(backup), "Backup created");
        Equal(1, (await f.Service.GetActivitiesForPatientAsync(p.Id)).Count, "Legacy event visible via canonical table");
        Equal(0L, await f.Scalar("SELECT COUNT(*) FROM Activities WHERE PatientId IS NOT NULL;"), "Old source consumed");
        var backupFixture = new Fixture(backup!);
        Equal(1L, await backupFixture.Scalar("SELECT COUNT(*) FROM Activities WHERE PatientId IS NOT NULL;"), "Backup keeps old source");
        Equal(0L, await backupFixture.Scalar("SELECT COUNT(*) FROM PatientActivities;"), "Backup is pre-import snapshot");
        Check(await f.Service.ConnectAsync(f.Path) is null, "Repeated connect is idempotent");
    }

    private static async Task LegacyConflict()
    {
        var f = await FixtureAsync(); var p1 = Patient(); var p2 = Patient("112345678");
        await f.Service.InsertPatientAsync(p1); await f.Service.InsertPatientAsync(p2);
        var a = ActivityFor(p1.Id); await f.Service.InsertActivityAsync(a);
        await using (var db = f.Db())
        {
            await db.Database.ExecuteSqlRawAsync("ALTER TABLE Activities ADD COLUMN PatientId TEXT NULL;");
            await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE Activities SET PatientId={p2.Id} WHERE Id={a.Id};");
        }
        await Throws<InvalidOperationException>(() => new SqliteDatabaseService().ConnectAsync(f.Path), "ütközés");
        Equal(1L, await f.Scalar("SELECT COUNT(*) FROM Activities WHERE PatientId IS NOT NULL;"), "Conflicting value preserved");
        Equal(1, (await f.Service.GetActivitiesForPatientAsync(p1.Id)).Count, "Canonical value preserved");
        Equal(0, (await f.Service.GetActivitiesForPatientAsync(p2.Id)).Count, "No wrong link created");
    }

    private static async Task LegacyDangling()
    {
        var f = await FixtureAsync(); var p = Patient(); await f.Service.InsertPatientAsync(p);
        var a = ActivityFor(p.Id); await f.Service.InsertActivityAsync(a);
        await using (var db = f.Db())
        {
            await db.Database.ExecuteSqlRawAsync("ALTER TABLE Activities ADD COLUMN PatientId TEXT NULL;");
            await db.Database.ExecuteSqlRawAsync("UPDATE Activities SET PatientId='missing';");
        }
        await Throws<InvalidOperationException>(() => new SqliteDatabaseService().ConnectAsync(f.Path), "hiányzó");
        Equal(1L, await f.Scalar("SELECT COUNT(*) FROM PatientActivities;"), "No link loss on failure");
    }

    private static async Task IncompatibleDatabase()
    {
        var f = await FixtureAsync();
        var legacy = new Fixture(Path.Combine(Results, "old-desktop.db"));
        await using (var db = legacy.Db(create: true))
            await db.Database.ExecuteSqlRawAsync("CREATE TABLE Patients (Id TEXT PRIMARY KEY, Name TEXT);");
        var before = await legacy.Scalar("SELECT group_concat(sql) FROM sqlite_master;");
        await Throws<InvalidOperationException>(() => f.Service.ConnectAsync(legacy.Path), "Nem kompatibilis");
        Equal(f.Path, f.Service.DatabasePath, "Previous connection preserved");
        Equal(before, await legacy.Scalar("SELECT group_concat(sql) FROM sqlite_master;"), "Legacy schema unchanged");
    }

    private static async Task MissingDatabase()
    {
        var path = Path.Combine(Results, "missing.db");
        var service = new SqliteDatabaseService();
        await Throws<FileNotFoundException>(() => service.ConnectAsync(path));
        Check(!File.Exists(path) && service.DatabasePath is null, "Missing file not created");
    }

    private static void FormLayout()
    {
        using var patient = new PatientEditForm(Patient());
        var eventInput = ActivityFor("test-patient");
        eventInput.Status = "Completed";
        using var activity = new ActivityEditForm("test-patient", [new("doctor-1", "Teszt Orvos")], eventInput);
        using var main = new MainForm();
        foreach (var (form, name) in new (Form, string)[] { (patient, "patient"), (activity, "activity"), (main, "main") })
        {
            // CreateHandle/Layout/DrawToBitmap only: never Show(), so MainForm.Shown cannot open the user's DB.
            _ = form.Handle;
            foreach (var control in Descendants(form))
            {
                _ = control.Handle;
                control.PerformLayout();
            }
            form.PerformLayout();
            using var bitmap = new Bitmap(form.Width, form.Height);
            form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.Size));
            bitmap.Save(Path.Combine(Results, name + ".png"), ImageFormat.Png);
        }
        var taj = Descendants(patient).OfType<TextBox>().Single(t => t.AccessibleName == "TAJ-szám");
        Equal("012345678", taj.Text, "TAJ loads into form");
        Check(taj.Width > 100 && taj.Height > 0, "TAJ textbox has visible size");
        Equal("Befejezve", Descendants(activity).OfType<ComboBox>().Single().SelectedItem?.ToString(), "Existing status displayed");
        var split = Descendants(main).OfType<SplitContainer>().Single();
        Check(split.Panel1.Width >= 420 && split.Panel2.Width >= 360, "Both grids have usable width");
        foreach (var form in new Form[] { patient, activity })
        {
            var save = Descendants(form).OfType<Button>().Single(b => b.Text == "Mentés");
            var bounds = form.RectangleToClient(save.RectangleToScreen(save.ClientRectangle));
            Check(bounds.Bottom <= form.ClientSize.Height && bounds.Right <= form.ClientSize.Width, "Save button is inside client area");
        }
    }

    private static IEnumerable<Control> Descendants(Control root) =>
        root.Controls.Cast<Control>().SelectMany(c => new[] { c }.Concat(Descendants(c)));

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    private static void Equal<T>(T expected, T actual, string message) =>
        Check(EqualityComparer<T>.Default.Equals(expected, actual), $"{message}: expected {expected}, got {actual}");

    private static async Task Throws<T>(Func<Task> action, string? text = null) where T : Exception
    {
        try { await action(); }
        catch (T ex) { if (text != null) Check(ex.Message.Contains(text), "Unexpected error: " + ex.Message); return; }
        throw new Exception("Expected " + typeof(T).Name);
    }

    private sealed record Fixture(string Path)
    {
        public SqliteDatabaseService Service { get; } = new();
        public AppDbContext Db(bool create = false) => new(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(
            new SqliteConnectionStringBuilder
            {
                DataSource = Path, Mode = create ? SqliteOpenMode.ReadWriteCreate : SqliteOpenMode.ReadWrite,
                ForeignKeys = true, Pooling = false
            }.ToString()).Options);

        public async Task<object?> Scalar(string sql)
        {
            await using var db = Db();
            await db.Database.OpenConnectionAsync();
            await using var command = db.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;
            return await command.ExecuteScalarAsync();
        }
    }
}
