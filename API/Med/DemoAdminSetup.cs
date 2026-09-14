using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Identity;
namespace API.Med;

public static class DemoAdminSetup
{
    public static async Task Configure(IServiceProvider services, IConfiguration configuration)
    {
        var email = configuration["DemoAdmin:Email"];
        var password = configuration["DemoAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("DemoAdmin:Email és DemoAdmin:Password szükséges (környezeti változóban vagy titokkezelőben).");
        var db = services.GetRequiredService<AppDbContext>();
        var users = services.GetRequiredService<UserManager<AppUser>>();
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        var id = DemoSessionSetup.Prefix + "desktop-admin";
        var byEmail = await users.FindByEmailAsync(email);
        if (byEmail is not null && byEmail.Id != id)
            throw new InvalidOperationException("Az e-mail már másik fiókhoz tartozik; nem módosítjuk és nem emeljük adminná.");
        await using var tx = await db.Database.BeginTransactionAsync();
        if (!await roles.RoleExistsAsync("Admin")) MedSetup.Check(await roles.CreateAsync(new("Admin")));
        var user = await users.FindByIdAsync(id);
        if (user is null) {
            user = new AppUser { Id = id, UserName = email, Email = email, EmailConfirmed = true };
            MedSetup.Check(await users.CreateAsync(user, password));
            MedSetup.Check(await users.AddToRoleAsync(user, "Admin"));
            db.AdminProfiles.Add(new AdminProfile { UserId = id, Name = "Asztali demóadmin" });
        } else {
            if (user.Email != email || !(await users.GetRolesAsync(user)).SequenceEqual(new[] { "Admin" }))
                throw new InvalidOperationException("A demóadmin profilját megváltoztatták; kézi ellenőrzés szükséges.");
            MedSetup.Check(await users.ResetPasswordAsync(user, await users.GeneratePasswordResetTokenAsync(user), password));
            MedSetup.Check(await users.SetLockoutEndDateAsync(user, null));
        }
        await db.SaveChangesAsync(); await tx.CommitAsync();
        Console.WriteLine("A helyi demóadmin elkészült. Jelszó nincs a forráskódban; Production módban a fiók tiltott.");
    }
}
