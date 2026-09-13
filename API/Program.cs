using Application.Activities.Queries;
using API.Med;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddMedActivities();
var provider = builder.Configuration["Database:Provider"] ?? "SqlServer";
if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddDbContext<SqlServerDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")));
    builder.Services.AddScoped<AppDbContext>(services => services.GetRequiredService<SqlServerDbContext>());
}
else if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=activities.db"));
else throw new InvalidOperationException("Ismeretlen Database:Provider; SqlServer vagy Sqlite választható.");

    builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
                ?? ["http://localhost:3000", "https://localhost:3000"]);
    });
});

builder.Services.AddMediatR(x => x.RegisterServicesFromAssemblyContaining<GetActivityList.Handler>());


var app = builder.Build();

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("CorsPolicy");

app.UseMedActivitiesGuard();
app.UseAuthentication();
app.UseMedSessionValidation();
app.UseDemoSessionBoundary();
app.UseAuthorization();
app.MapGroup("/api/auth").MapIdentityApi<AppUser>();
app.MapControllers();
app.MapGet("/api/health", async (AppDbContext db) =>
    await db.Database.CanConnectAsync()
        ? Results.Ok(new { status = "ok", database = db.Database.IsSqlServer() ? "SqlServer" : "Sqlite" })
        : Results.Problem("Az adatbázis nem érhető el.", statusCode: 503));
app.MapFallback(async context =>
{
    var index = Path.Combine(app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot"), "index.html");
    if (context.Request.Path.StartsWithSegments("/api") || !File.Exists(index))
    { context.Response.StatusCode = 404; return; }
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(index);
});

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    var context = services.GetRequiredService<AppDbContext>();
    if (builder.Configuration.GetValue<bool>("Database:ApplyMigrations"))
        await context.Database.MigrateAsync();
    if (builder.Configuration.GetValue<bool>("Database:SeedDemoData"))
        await DbInitalizer.SeedData(context);
    await app.Services.SeedMedRolesAsync(app.Configuration);
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during migration");
    throw;
}

app.Run();

