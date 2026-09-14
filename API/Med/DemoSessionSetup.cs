using System.Security.Claims;
using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Identity;
namespace API.Med;

public static class DemoSessionSetup
{
    public const string Claim = "egeszsegut:demo";
    public const string Prefix = "egeszsegut-demo-v1-";
    public static readonly string[] Roles = ["Admin", "AdmissionsOffice", "Practitioner", "Patient"];
    public static readonly string[] Names = ["Demó Admin", "Demó Felvételi Iroda", "Dr. Minta Péter", "Minta Anna"];
    // Egy folyamaton belül ne induljon párhuzamos seed két gombnyomásból.
    public static readonly SemaphoreSlim Gate = new(1, 1);

    public static IApplicationBuilder UseDemoSessionBoundary(this IApplicationBuilder app)
        => app.Use(async (context, next) => {
            var env=context.RequestServices.GetRequiredService<IWebHostEnvironment>();
            var id=context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(!env.IsDevelopment() && (context.User.HasClaim(Claim,"true") || id?.StartsWith(Prefix,StringComparison.Ordinal)==true))
            {
                context.Response.StatusCode=401;
                await context.Response.WriteAsJsonAsync(new { message="A demómunkamenet csak fejlesztői módban használható." });
                return;
            }
            await next();
        });

    public static async Task SeedAsync(AppDbContext db,UserManager<AppUser> users,RoleManager<IdentityRole> roles)
    {
        // A teljes adatfeltöltés saját tranzakciójába is beilleszthető.
        await using var tx=db.Database.CurrentTransaction is null ? await db.Database.BeginTransactionAsync() : null;
        for(var i=0;i<Roles.Length;i++)
        {
            var role=Roles[i];var id=Prefix+role;
            if(!await roles.RoleExistsAsync(role))MedSetup.Check(await roles.CreateAsync(new(role)));
            var user=await users.FindByIdAsync(id);
            var username="demo.egeszsegut."+role.ToLowerInvariant();
            if(user is null)
            {
                user=new AppUser{Id=id,UserName=username,Email=username+"@demo.example.invalid",EmailConfirmed=true};
                MedSetup.Check(await users.CreateAsync(user)); // nincs közös vagy beégetett jelszó
            }
            if(user.UserName!=username)throw new InvalidOperationException("A demófiók azonosítója már foglalt.");
            if(string.IsNullOrWhiteSpace(user.Email))
                MedSetup.Check(await users.SetEmailAsync(user,username+"@demo.example.invalid"));
            var assigned=await users.GetRolesAsync(user);
            if(assigned.Any(r=>r!=role))throw new InvalidOperationException("A demófiók szerepköreit megváltoztatták; a demóbelépés leállt.");
            if(!assigned.Contains(role))MedSetup.Check(await users.AddToRoleAsync(user,role));
        }
        var patientId=Prefix+"patient-profile";
        var doctorId=Prefix+"doctor-profile";
        if(!await db.Patients.AnyAsync(p=>p.Id==patientId))db.Patients.Add(new Patient {
            Id=patientId,UserId=Prefix+"Patient",Name=Names[3],TajNumber="000008011",
            BirthDate=new DateOnly(1990,1,15),Notes="Kizárólag fiktív demóadat."
        });
        if(!await db.Practitioners.AnyAsync(p=>p.Id==doctorId))db.Practitioners.Add(new PractitionerProfile {
            Id=doctorId,UserId=Prefix+"Practitioner",Name=Names[2],TajNumber="000008012",
            Specialty="Rehabilitáció",City="Budapest",Venue="EgészségÚt demórendelő"
        });
        if(!await db.AdminProfiles.AnyAsync(p=>p.UserId==Prefix+"Admin"))db.AdminProfiles.Add(new(){Id=Prefix+"admin-profile",UserId=Prefix+"Admin",Name=Names[0]});
        if(!await db.AdmissionsOfficeProfiles.AnyAsync(p=>p.UserId==Prefix+"AdmissionsOffice"))db.AdmissionsOfficeProfiles.Add(new(){Id=Prefix+"office-profile",UserId=Prefix+"AdmissionsOffice",Name=Names[1]});
        if(!await db.PatientPractitionerAccesses.AnyAsync(p=>p.PatientId==patientId&&p.PractitionerId==doctorId))
            db.PatientPractitionerAccesses.Add(new(){PatientId=patientId,PractitionerId=doctorId});
        if(!await db.Activities.AnyAsync(a=>a.Id==Prefix+"activity"))
        {
            var a=new Activity{Id=Prefix+"activity",Title="Első konzultáció – demó",Date=BookingService.LocalNow().Date.AddDays(1).AddHours(10),
                Description="Fiktív esemény a szerepkörök és a közös eseménykezelés kipróbálásához.",Category="Vizsgálat",City="Budapest",Venue="EgészségÚt demórendelő",Latitude=47.4979,Longitude=19.0402};
            a.PatientActivities.Add(new(){ActivityId=a.Id,PatientId=patientId});
            a.ActivityPractitioners.Add(new(){ActivityId=a.Id,PractitionerId=doctorId});
            db.Activities.Add(a);
        }
        await db.SaveChangesAsync();
        if(tx is not null)await tx.CommitAsync();
    }
}
