using Application.Activities.Queries;
using API.Med;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Identity;

var seedDemo = args.Contains("--seed-demo-data");
var demoStatus = args.Contains("--demo-data-status");
var demoAdmin = args.Contains("--configure-demo-admin");
var builder = WebApplication.CreateBuilder(args.Where(a => a is not "--seed-demo-data" and not "--demo-data-status" and not "--configure-demo-admin").ToArray());

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSignalR(options => options.MaximumReceiveMessageSize = 16 * 1024);
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

// Explicit CLI workflow: no listening port needed and no implicit production seeding.
if (seedDemo || demoStatus || demoAdmin)
{
    using var demoScope = app.Services.CreateScope();
    var demoDb = demoScope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (demoAdmin) {
        if (!app.Environment.IsDevelopment()) throw new InvalidOperationException("Demóadmin csak Development környezetben konfigurálható.");
        await DemoAdminSetup.Configure(demoScope.ServiceProvider, app.Configuration);
    }
    if (seedDemo)
    {
        if (!app.Environment.IsDevelopment()) throw new InvalidOperationException("Demóadat csak Development környezetben tölthető fel.");
        if ((await demoDb.Database.GetPendingMigrationsAsync()).Any())
            throw new InvalidOperationException("Előbb alkalmazd a függő adatbázis-migrációkat.");
        await DemoDataSeeder.SeedAsync(demoDb,
            demoScope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<AppUser>>(),
            demoScope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>());
    }
    Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(await DemoDataSeeder.Report(demoDb)));
    return;
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("CorsPolicy");

app.UseMedActivitiesGuard();
// Browser WebSocket/SSE transports pass bearer tokens in the query string.
// Accept it only on the chat route; ASP.NET request logging is Warning (no token URLs).
app.Use(async (context, next) => {
    if (context.Request.Path.StartsWithSegments("/api/chat") && !context.Request.Headers.ContainsKey("Authorization") &&
        context.Request.Query.TryGetValue("access_token", out var chatToken))
        context.Request.Headers.Authorization = "Bearer " + chatToken.ToString();
    await next();
});
app.UseAuthentication();
app.UseMedSessionValidation();
app.UseDemoSessionBoundary();
app.UseAuthorization();
app.MapGroup("/api/auth").MapIdentityApi<AppUser>();
app.MapControllers();
app.MapHub<API.SignalR.ChatHub>("/api/chat", options => options.CloseOnAuthenticationExpiration = true);
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

