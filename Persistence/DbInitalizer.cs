using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence.Identity;

namespace Persistence;

public class DbInitalizer
{
    public static async Task SeedData(AppDbContext context)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        if (!await context.Activities.AnyAsync())
        {
        var activities = new List<Activity>
        {
        new()
        {
            Title = "Első ortopédiai vizsgálat",
            Date = DateTime.Now.AddMonths(-2),
            Description = "Első szakorvosi vizsgálat jobb Achilles-ín fájdalom miatt.",
            Category = "Vizsgálat",
            City = "Budapest",
    Venue = "Semmelweis Egyetem Ortopédiai Klinika",
        Latitude = 47.488820,
            Longitude = 19.086320
        },
new()
{
    Title = "MR vizsgálat",
    Date = DateTime.Now.AddMonths(-1),
    Description = "Lumbális gerinc MR vizsgálat derékfájdalom kivizsgálására.",
    Category = "Képalkotó vizsgálat",
    City = "Budapest",
    Venue = "Affidea Diagnosztika",
    Latitude = 47.505000,
    Longitude = 19.070500
},
new()
{
    Title = "Gyógytorna állapotfelmérés",
    Date = DateTime.Now.AddMonths(1),
    Description = "Első gyógytorna konzultáció és mozgásszervi állapotfelmérés.",
    Category = "Gyógytorna",
    City = "Budapest",
    Venue = "Physio Praxis",
    Latitude = 47.513600,
    Longitude = 19.050100
},
new()
{
    Title = "Kontroll ortopédiai vizsgálat",
    Date = DateTime.Now.AddMonths(2),
    Description = "A kezelés eredményének kontrollvizsgálata.",
    Category = "Kontroll",
    City = "Budapest",
    Venue = "Ortopédiai Szakrendelő",
    Latitude = 47.510500,
    Longitude = 19.040700
},
new()
{
    Title = "Lökéshullám-terápia",
    Date = DateTime.Now.AddMonths(3),
    Description = "Krónikus Achilles-ín gyulladás kezelése lökéshullám-terápiával.",
    Category = "Kezelés",
    City = "Budapest",
    Venue = "Mozgásszervi Rehabilitációs Centrum",
    Latitude = 47.507900,
    Longitude = 19.048300
},
new()
{
    Title = "Laborvizsgálat",
    Date = DateTime.Now.AddMonths(4),
    Description = "Általános laborvizsgálat és gyulladásos paraméterek ellenőrzése.",
    Category = "Labor",
    City = "Budapest",
    Venue = "Synlab Diagnosztika",
    Latitude = 47.506800,
    Longitude = 19.060500
},
new()
{
    Title = "Reumatológiai szakvizsgálat",
    Date = DateTime.Now.AddMonths(5),
    Description = "Krónikus derékfájdalom kivizsgálása.",
    Category = "Vizsgálat",
    City = "Budapest",
    Venue = "ORFI",
    Latitude = 47.498900,
    Longitude = 19.067400
},
new()
{
    Title = "Kontroll gyógytorna",
    Date = DateTime.Now.AddMonths(6),
    Description = "Funkcionális állapot felmérése és gyakorlatok módosítása.",
    Category = "Gyógytorna",
    City = "Budapest",
    Venue = "Physio Praxis",
    Latitude = 47.513600,
    Longitude = 19.050100
},
new()
{
    Title = "Műtéti konzultáció",
    Date = DateTime.Now.AddMonths(7),
    Description = "Sebészeti konzultáció a további kezelési lehetőségekről.",
    Category = "Konzultáció",
    City = "Budapest",
    Venue = "Traumatológiai Centrum",
    Latitude = 47.492600,
    Longitude = 19.071000
},
new()
{
    Title = "Éves mozgásszervi kontroll",
    Date = DateTime.Now.AddMonths(8),
    Description = "Éves komplex ortopédiai és fizioterápiás kontrollvizsgálat.",
    Category = "Kontroll",
    City = "Budapest",
    Venue = "Semmelweis Egészségközpont",
    Latitude = 47.495900,
    Longitude = 19.084300
}
        };


            context.Activities.AddRange(activities);
            await context.SaveChangesAsync();
        }

        // A régi korai return helyett a meglévő rekordokat is kiegészítjük.
        var activitiesToLink = await context.Activities
            .Include(a => a.PatientActivities)
            .Include(a => a.ActivityPractitioners)
            .Where(a => !a.PatientActivities.Any() || !a.ActivityPractitioners.Any())
            .ToListAsync();

        if (activitiesToLink.Count == 0)
        {
            await transaction.CommitAsync();
            return;
        }

        Patient? patient = null;
        if (activitiesToLink.Any(a => a.PatientActivities.Count == 0))
        {
            patient = await context.Patients.FindAsync("demo-patient-minta-anna");
            if (patient is null)
            {
                if (await context.Patients.AnyAsync(p => p.TajNumber == "000000001"))
                    throw new InvalidOperationException("A demópáciens 000000001 teszt-TAJ értéke már foglalt. Válassz másik fiktív értéket a seedben.");

                patient = new Patient
                {
                    Id = "demo-patient-minta-anna",
                    Name = "Minta Anna (demó)",
                    TajNumber = "000000001",
                    BirthDate = new DateOnly(1990, 1, 15),
                    Notes = "Fiktív vizsgapáciens; nem valós TAJ-adat."
                };
                context.Patients.Add(patient);
            }
        }

        PractitionerProfile? practitioner = null;
        if (activitiesToLink.Any(a => a.ActivityPractitioners.Count == 0))
        {
            practitioner = await context.Practitioners.FindAsync("demo-practitioner-minta-peter");
            if (practitioner is null)
            {
                if (await context.Practitioners.AnyAsync(p => p.TajNumber == "000000002"))
                    throw new InvalidOperationException("A demókezelő 000000002 teszt-TAJ értéke már foglalt. Válassz másik fiktív értéket a seedben.");

                // A PractitionerProfile.UserId kötelező idegen kulcs.
                // Jelszó nélküli, belépésre nem használható demóidentitás.
                const string userId = "demo-user-minta-peter";
                var user = await context.Users.FindAsync(userId);
                if (user is null)
                {
                    user = new AppUser
                    {
                        Id = userId,
                        UserName = "demo.seed.minta.peter",
                        NormalizedUserName = "DEMO.SEED.MINTA.PETER",
                        SecurityStamp = Guid.NewGuid().ToString(),
                        EmailConfirmed = false
                    };
                    context.Users.Add(user);
                }

                practitioner = new PractitionerProfile
                {
                    Id = "demo-practitioner-minta-peter",
                    UserId = userId,
                    Name = "Dr. Minta Péter (demó)",
                    TajNumber = "000000002",
                    Specialty = "Rehabilitáció",
                    City = "Budapest",
                    Venue = "Demó rendelő"
                };
                context.Practitioners.Add(practitioner);
            }
        }

        foreach (var activity in activitiesToLink)
        {
            if (activity.PatientActivities.Count == 0)
            {
                activity.PatientActivities.Add(new PatientActivity
                {
                    ActivityId = activity.Id,
                    PatientId = patient!.Id,
                    Patient = patient
                });
            }

            if (activity.ActivityPractitioners.Count == 0)
            {
                activity.ActivityPractitioners.Add(new ActivityPractitioner
                {
                    ActivityId = activity.Id,
                    PractitionerId = practitioner!.Id,
                    Practitioner = practitioner
                });
            }
        }

        await context.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}
