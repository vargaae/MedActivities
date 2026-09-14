using Microsoft.EntityFrameworkCore;

namespace Persistence;

// Külön migrációs előzmény és snapshot, azonos domainmodell.
public class SqlServerDbContext(DbContextOptions<SqlServerDbContext> options) : AppDbContext(options)
{
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        var opened = Database.GetDbConnection().State != System.Data.ConnectionState.Open;
        if (opened) await Database.OpenConnectionAsync(cancellationToken);
        try {
            await Database.ExecuteSqlInterpolatedAsync($"EXEC sys.sp_set_session_context @key=N'MedActivitiesUserId', @value={AuditUserId}", cancellationToken);
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        finally { if (opened) await Database.CloseConnectionAsync(); }
    }
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        var opened = Database.GetDbConnection().State != System.Data.ConnectionState.Open;
        if (opened) Database.OpenConnection();
        try {
            Database.ExecuteSqlInterpolated($"EXEC sys.sp_set_session_context @key=N'MedActivitiesUserId', @value={AuditUserId}");
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }
        finally { if (opened) Database.CloseConnection(); }
    }
}
