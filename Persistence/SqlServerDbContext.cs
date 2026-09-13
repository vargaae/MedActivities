using Microsoft.EntityFrameworkCore;

namespace Persistence;

// Külön migrációs előzmény és snapshot, azonos domainmodell.
public class SqlServerDbContext(DbContextOptions<SqlServerDbContext> options) : AppDbContext(options);
