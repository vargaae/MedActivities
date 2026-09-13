using Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MedActivities.Patient.Sqlite.WinForms.Models;
using Persistence;

namespace MedActivities.Patient.Sqlite.WinForms.Services;

/// <summary>Az API-val közös EF-modell, műveletenként rövid életű SQLite-kapcsolattal.</summary>
public sealed class SqliteDatabaseService
{
    public string? DatabasePath { get; private set; }

    private static AppDbContext Context(string path) => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlite(new SqliteConnectionStringBuilder
        {
            DataSource = path, Mode = SqliteOpenMode.ReadWrite, ForeignKeys = true,
            DefaultTimeout = 5, Pooling = false
        }.ToString()).Options);

    private AppDbContext Context() => Context(DatabasePath
        ?? throw new InvalidOperationException("Nincs csatlakoztatva SQLite adatbázis."));

    // Nem hoz létre párhuzamos sémát. A migrációkat továbbra is az API/Persistence kezeli.
    public async Task<string?> ConnectAsync(string path)
    {
        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath)) throw new FileNotFoundException("Az adatbázisfájl nem található.", fullPath);
        await using var db = Context(fullPath);
        await db.Database.OpenConnectionAsync();
        await ValidateSchemaAsync(db);
        var backup = await LegacyPatientLinks.ImportAsync(db, fullPath);
        DatabasePath = fullPath; // Sikertelen csatlakozás nem írja felül a működő kapcsolatot.
        return backup;
    }

    private static async Task ValidateSchemaAsync(AppDbContext db)
    {
        var connection = db.Database.GetDbConnection();
        // Minden mezőt a közös EF-modellből ellenőrzünk; nem olvasunk páciensadatot.
        foreach (var entity in db.Model.GetEntityTypes())
        {
            var table = entity.GetTableName();
            if (table is null) continue;
            var identifier = StoreObjectIdentifier.Table(table, entity.GetSchema());
            var expected = entity.GetProperties().Select(p => p.GetColumnName(identifier)!).ToHashSet();
            await using var command = connection.CreateCommand();
            command.CommandText = $"PRAGMA table_info(\"{table.Replace("\"", "\"\"")}\");";
            var actual = new HashSet<string>();
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync()) actual.Add(reader.GetString(1));
            if (!expected.IsSubsetOf(actual))
                throw new InvalidOperationException(
                    $"Nem kompatibilis adatbázis: hiányzó tábla vagy mező ({table}). " +
                    "Válaszd az API által migrált activities.db fájlt. A régi, önálló desktop-adatbázis nem módosult.");
        }
        if ((await db.Database.GetPendingMigrationsAsync()).Any())
            throw new InvalidOperationException("Az adatbázis migrációi hiányosak. Először indítsd el az API-t a frissítéshez.");
    }

    public async Task<List<PatientRecord>> GetPatientsAsync(string? search = null)
    {
        await using var db = Context();
        var pattern = "%" + (search ?? "").Trim().Replace("!", "!!").Replace("%", "!%").Replace("_", "!_") + "%";
        return await db.Patients.AsNoTracking()
            .Where(p => EF.Functions.Like(p.Name, pattern, "!") || EF.Functions.Like(p.TajNumber, pattern, "!") ||
                EF.Functions.Like(p.Email!, pattern, "!") || EF.Functions.Like(p.Phone!, pattern, "!") ||
                EF.Functions.Like(p.Address!, pattern, "!"))
            .OrderBy(p => p.Name).Select(p => new PatientRecord
            {
                Id = p.Id, Name = p.Name, TajNumber = p.TajNumber,
                BirthDate = p.BirthDate.ToDateTime(TimeOnly.MinValue),
                Email = p.Email ?? "", Phone = p.Phone ?? "", Address = p.Address ?? "", Notes = p.Notes ?? ""
            }).ToListAsync();
    }

    public async Task<List<PractitionerOption>> GetPractitionersAsync()
    {
        await using var db = Context();
        return await db.Practitioners.AsNoTracking().OrderBy(p => p.Name)
            .Select(p => new PractitionerOption(p.Id, p.Name)).ToListAsync();
    }

    public Task InsertPatientAsync(PatientRecord patient)
    {
        RecordValidation.Patient(patient);
        return WriteAsync(async db =>
        {
            await CheckTajAsync(db, patient);
            var entity = new Domain.Patient { Id = patient.Id, Name = patient.Name, TajNumber = patient.TajNumber };
            Apply(entity, patient);
            db.Patients.Add(entity);
        });
    }

    public Task UpdatePatientAsync(PatientRecord patient)
    {
        RecordValidation.Patient(patient);
        return WriteAsync(async db =>
        {
            var entity = await db.Patients.FindAsync(patient.Id)
                ?? throw new InvalidOperationException("A páciens már nem található. Frissítsd a listát.");
            await CheckTajAsync(db, patient);
            Apply(entity, patient); // UserId és CreatedAt változatlan.
        });
    }

    private static async Task CheckTajAsync(AppDbContext db, PatientRecord patient)
    {
        if (await db.Patients.AnyAsync(p => p.TajNumber == patient.TajNumber && p.Id != patient.Id))
            throw new InvalidOperationException("Ezzel a TAJ-számmal már létezik páciens.");
    }

    public Task DeletePatientAsync(string id) => WriteAsync(async db =>
    {
        var patient = await db.Patients.FindAsync(id) ?? throw new InvalidOperationException("A páciens nem található.");
        if (await db.PatientActivities.AnyAsync(p => p.PatientId == id) ||
            await db.Appointments.AnyAsync(a => a.PatientId == id))
            throw new InvalidOperationException("Eseményhez vagy foglaláshoz kapcsolt páciens nem törölhető.");
        db.Patients.Remove(patient);
    });

    public async Task<List<ActivityRecord>> GetActivitiesForPatientAsync(string patientId)
    {
        await using var db = Context();
        var rows = await db.Activities.AsNoTracking()
            .Where(a => a.PatientActivities.Any(p => p.PatientId == patientId))
            .OrderByDescending(a => a.Date)
            .Select(a => new
            {
                Entity = a,
                IsAppointment = db.Appointments.Any(x => x.ActivityId == a.Id),
                Practitioners = a.ActivityPractitioners.OrderBy(p => p.Practitioner.Name)
                    .Select(p => new PractitionerOption(p.PractitionerId, p.Practitioner.Name)).ToList()
            }).ToListAsync();
        return rows.Select(row => new ActivityRecord
        {
            Id = row.Entity.Id, PatientId = patientId, Title = row.Entity.Title, Date = row.Entity.Date,
            Description = row.Entity.Description, Category = row.Entity.Category, Status = row.Entity.Status,
            City = row.Entity.City, Venue = row.Entity.Venue, Latitude = row.Entity.Latitude,
            Longitude = row.Entity.Longitude, IsAppointment = row.IsAppointment,
            PractitionerIds = row.Practitioners.Select(p => p.Id).ToList(),
            PractitionerNames = string.Join(", ", row.Practitioners.Select(p => p.Name))
        }).ToList();
    }

    public Task InsertActivityAsync(ActivityRecord activity)
    {
        RecordValidation.Activity(activity);
        return WriteAsync(async db =>
        {
            await CheckAssignmentsAsync(db, activity);
            var entity = new Activity
            {
                Id = activity.Id, Title = activity.Title, Description = activity.Description,
                Category = activity.Category, City = activity.City, Venue = activity.Venue
            };
            Apply(entity, activity);
            entity.PatientActivities.Add(new() { PatientId = activity.PatientId!, ActivityId = entity.Id });
            foreach (var id in activity.PractitionerIds.Distinct())
                entity.ActivityPractitioners.Add(new() { PractitionerId = id, ActivityId = entity.Id });
            db.Activities.Add(entity);
        });
    }

    public Task UpdateActivityAsync(ActivityRecord activity)
    {
        RecordValidation.Activity(activity);
        return WriteAsync(async db =>
        {
            var entity = await db.Activities.Include(a => a.PatientActivities).Include(a => a.ActivityPractitioners)
                .SingleOrDefaultAsync(a => a.Id == activity.Id)
                ?? throw new InvalidOperationException("Az esemény már nem található.");
            await CheckEditableActivityAsync(db, entity, activity.PatientId!);
            await CheckAssignmentsAsync(db, activity);
            Apply(entity, activity);
            entity.UpdatedAt = DateTime.UtcNow;
            // Nincs bejelentkezett desktop-felhasználó; nem tulajdonítjuk az írást egy régi API-felhasználónak.
            entity.UpdatedByUserId = null;
            var ids = activity.PractitionerIds.ToHashSet();
            db.ActivityPractitioners.RemoveRange(entity.ActivityPractitioners.Where(p => !ids.Contains(p.PractitionerId)));
            foreach (var id in ids.Where(id => !entity.ActivityPractitioners.Any(p => p.PractitionerId == id)))
                entity.ActivityPractitioners.Add(new() { PractitionerId = id, ActivityId = entity.Id });
        });
    }

    public Task DeleteActivityAsync(string id, string patientId) => WriteAsync(async db =>
    {
        var entity = await db.Activities.Include(a => a.PatientActivities).SingleOrDefaultAsync(a => a.Id == id)
            ?? throw new InvalidOperationException("Az esemény nem található.");
        await CheckEditableActivityAsync(db, entity, patientId);
        db.Activities.Remove(entity); // A közös modell idegen kulcsai törlik a kapcsolótábla sorait.
    });

    private static async Task CheckEditableActivityAsync(AppDbContext db, Activity entity, string patientId)
    {
        if (await db.Appointments.AnyAsync(a => a.ActivityId == entity.Id))
            throw new InvalidOperationException("Foglalási esemény módosítását vagy törlését a webes időpontkezelőben végezd el.");
        if (entity.PatientActivities.Count != 1 || entity.PatientActivities.Single().PatientId != patientId)
            throw new InvalidOperationException("Az esemény pácienskapcsolata megváltozott vagy több pácienshez tartozik. Frissítsd a listát; a hozzárendelést a webes felületen rendezd.");
    }

    private static async Task CheckAssignmentsAsync(AppDbContext db, ActivityRecord activity)
    {
        if (!await db.Patients.AnyAsync(p => p.Id == activity.PatientId))
            throw new InvalidOperationException("A kiválasztott páciens már nem létezik.");
        var ids = activity.PractitionerIds.Distinct().ToList();
        if (await db.Practitioners.CountAsync(p => ids.Contains(p.Id)) != ids.Count)
            throw new InvalidOperationException("A kiválasztott kezelőorvos már nem létezik. Frissítsd a listát.");
    }

    private async Task WriteAsync(Func<AppDbContext, Task> action)
    {
        try
        {
            await using var db = Context();
            await using var tx = await db.Database.BeginTransactionAsync();
            await action(db);
            await db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteErrorCode: 19 })
        {
            throw new InvalidOperationException("A mentés nem sikerült: foglalt TAJ-szám vagy sérült adatkapcsolat. Frissítsd a listát.", ex);
        }
        catch (Exception ex) when (ex is SqliteException { SqliteErrorCode: 5 or 6 } ||
                                   ex.InnerException is SqliteException { SqliteErrorCode: 5 or 6 })
        {
            throw new InvalidOperationException("Az adatbázist egy másik művelet használja. Próbáld újra.", ex);
        }
    }

    private static void Apply(Domain.Patient entity, PatientRecord input)
    {
        entity.Name = input.Name; entity.TajNumber = input.TajNumber;
        entity.BirthDate = DateOnly.FromDateTime(input.BirthDate);
        entity.Email = Optional(input.Email); entity.Phone = Optional(input.Phone);
        entity.Address = Optional(input.Address); entity.Notes = Optional(input.Notes);
    }

    private static void Apply(Activity entity, ActivityRecord input)
    {
        entity.Title = input.Title; entity.Date = input.Date; entity.Description = input.Description;
        entity.Category = input.Category; entity.Status = input.Status; entity.IsCancelled = input.IsCancelled;
        entity.City = input.City; entity.Venue = input.Venue;
        entity.Latitude = input.Latitude; entity.Longitude = input.Longitude;
    }

    private static string? Optional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
