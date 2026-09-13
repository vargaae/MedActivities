using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Persistence;

namespace API;

public sealed class SqlServerDesignFactory : IDesignTimeDbContextFactory<SqlServerDbContext>
{
    public SqlServerDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<SqlServerDbContext>()
        .UseSqlServer(Environment.GetEnvironmentVariable("ConnectionStrings__SqlServerConnection")
            ?? @"Server=(localdb)\MSSQLLocalDB;Database=MedActivities;Integrated Security=True;Encrypt=True;TrustServerCertificate=True").Options);
}

public sealed class SqliteDesignFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlite(Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Data Source=activities.db").Options);
}
