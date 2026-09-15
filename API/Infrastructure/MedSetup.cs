using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;
using Persistence;
using Persistence.Identity;
namespace API.Med;
public static class MedSetup
{
    public static IServiceCollection AddMedActivities(this IServiceCollection services) {
        services.Configure<IdentityOptions>(o=>o.User.RequireUniqueEmail=true);
        services.AddHttpContextAccessor();services.AddScoped<AccessService>();services.AddScoped<BookingService>();
        services.AddIdentityApiEndpoints<AppUser>().AddRoles<IdentityRole>().AddEntityFrameworkStores<AppDbContext>();
        services.AddAuthorization();return services;
    }
    public static IApplicationBuilder UseMedActivitiesGuard(this IApplicationBuilder app)=>app.Use(async(context,next)=>{

        try {await next();}
        catch(BookingException e){context.Response.StatusCode=409;await context.Response.WriteAsJsonAsync(new{message=e.Message});}
        catch(DbUpdateException e) when(e.InnerException is SqlException {Number:2601 or 2627 or 547}) {
            context.Response.StatusCode=409;await context.Response.WriteAsJsonAsync(new{message="Egyediség vagy kapcsolat sérül: TAJ, felhasználó, dokumentum, napi foglalás vagy foglalt időpont."});
        }
        catch(DbUpdateException e) when(e.GetBaseException() is SqlException) {
            context.Response.StatusCode=409;await context.Response.WriteAsJsonAsync(new{message="Az adatbázis-kapcsolat miatt a törlés vagy mentés nem hajtható végre. Ellenőrizd a kapcsolódó adatokat, majd frissítsd a listát."});
        }
        catch(SqlException) {
            context.Response.StatusCode=409;await context.Response.WriteAsJsonAsync(new{message="Az adatbázis-kapcsolat miatt a művelet nem hajtható végre. Próbáld újra frissítés után."});
        }
        catch(Exception e) when(e is SqlException {Number:1205 or 1222} || e.InnerException is SqlException {Number:1205 or 1222}) {
            context.Response.StatusCode=409;await context.Response.WriteAsJsonAsync(new{message="Párhuzamos adatbázis-művelet. Frissíts és próbáld újra."});
        }
        catch(DbUpdateException e) when(e.InnerException is SqliteException {SqliteErrorCode:19}) {
            context.Response.StatusCode=409;await context.Response.WriteAsJsonAsync(new{message="Egyediség vagy kapcsolat sérül: TAJ, felhasználó, napi foglalás vagy foglalt időpont."});
        }
        catch(Exception e) when(e is SqliteException {SqliteErrorCode:5 or 6} || e.InnerException is SqliteException {SqliteErrorCode:5 or 6}) {
            context.Response.StatusCode=409;await context.Response.WriteAsJsonAsync(new{message="Párhuzamos adatbázis-művelet. Frissítsd a szabad időpontokat és próbáld újra."});
        }
    });
    public static async Task SeedMedRolesAsync(this IServiceProvider services,IConfiguration configuration) {
        using var scope=services.CreateScope();
        var roles=scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach(var name in AppRoles.All) if(!await roles.RoleExistsAsync(name))Check(await roles.CreateAsync(new(name)));
        var email=configuration["BootstrapAdmin:Email"];var password=configuration["BootstrapAdmin:Password"];
        if(string.IsNullOrWhiteSpace(email)||string.IsNullOrWhiteSpace(password))return;
        var users=scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user=await users.FindByEmailAsync(email);
        if(user is not null)return;
        var db=scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var tx=await db.Database.BeginTransactionAsync();
        user=new(){UserName=email,Email=email,EmailConfirmed=true};Check(await users.CreateAsync(user,password));
        Check(await users.AddToRoleAsync(user,AppRoles.Admin));
        db.AdminProfiles.Add(new(){UserId=user.Id,Name="MVP admin"});await db.SaveChangesAsync();await tx.CommitAsync();
    }
    public static void Check(IdentityResult r){if(!r.Succeeded)throw new InvalidOperationException(string.Join("; ",r.Errors.Select(e=>e.Description)));}
}

