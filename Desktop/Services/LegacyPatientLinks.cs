using Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Persistence;

namespace MedActivities.Patient.Sqlite.WinForms.Services;

internal static class LegacyPatientLinks
{
    // A régi desktop az API adatbázisához külön Activities.PatientId oszlopot adott.
    // Csak az egyértelmű kapcsolatot emeljük át. A régi értéket a sikeres tranzakció
    // végén ürítjük, nehogy a következő indítás visszahozzon egy később törölt kapcsolatot.
    public static async Task<string?> ImportAsync(AppDbContext db, string path)
    {
        var connection = (SqliteConnection)db.Database.GetDbConnection();
        await using (var columns = connection.CreateCommand())
        {
            columns.CommandText = "PRAGMA table_info(Activities);";
            var found = false;
            await using var reader = await columns.ExecuteReaderAsync();
            while (await reader.ReadAsync()) found |= reader.GetString(1) == "PatientId";
            if (!found) return null;
        }
        await using (var count = connection.CreateCommand())
        {
            count.CommandText = "SELECT COUNT(*) FROM Activities WHERE PatientId IS NOT NULL;";
            if (Convert.ToInt64(await count.ExecuteScalarAsync()) == 0) return null;
        }

        var backupPath = path + ".desktop-links-" + Guid.NewGuid().ToString("N") + ".db";
        await using (var backup = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = backupPath, Pooling = false
        }.ToString()))
        {
            await backup.OpenAsync();
            connection.BackupDatabase(backup);
        }

        await using var tx = await db.Database.BeginTransactionAsync();
        var links = new List<(string ActivityId, string PatientId)>();
        await using (var command = connection.CreateCommand())
        {
            command.Transaction = (SqliteTransaction)tx.GetDbTransaction();
            command.CommandText = "SELECT Id, PatientId FROM Activities WHERE PatientId IS NOT NULL;";
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync()) links.Add((reader.GetString(0), reader.GetString(1)));
        }

        foreach (var link in links)
        {
            var existing = await db.PatientActivities.Where(p => p.ActivityId == link.ActivityId)
                .Select(p => p.PatientId).ToListAsync();
            if (!await db.Patients.AnyAsync(p => p.Id == link.PatientId) ||
                existing.Any(id => id != link.PatientId) ||
                await db.Appointments.AnyAsync(a => a.ActivityId == link.ActivityId && a.PatientId != link.PatientId))
                throw new InvalidOperationException(
                    $"A régi pácienskapcsolatok között ütközés vagy hiányzó páciens van. Az adatbázis nem módosult. Biztonsági másolat: {backupPath}");
            if (existing.Count == 0)
                db.PatientActivities.Add(new PatientActivity { ActivityId = link.ActivityId, PatientId = link.PatientId });
        }
        await db.SaveChangesAsync();
        await db.Database.ExecuteSqlRawAsync("UPDATE Activities SET PatientId = NULL WHERE PatientId IS NOT NULL;");
        await tx.CommitAsync();
        return backupPath;
    }
}
