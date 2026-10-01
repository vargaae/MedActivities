using System.Globalization;
using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace API.Med;

public static class RegistrationWelcome
{
    // Called only within the registration transaction. No booking, clinician assignment or medical advice is created.
    public static async Task<Activity> Add(AppDbContext db, string userId, string name,
        RegistrationDeclaration policy, string signedName, DateTimeOffset acceptedAtUtc,
        string? patientId, string? practitionerId)
    {
        var admins = await (from ur in db.UserRoles
                            join role in db.Roles on ur.RoleId equals role.Id
                            join user in db.Users on ur.UserId equals user.Id
                            where role.NormalizedName == "ADMIN"
                            orderby user.Id
                            select new { user.Id, user.LockoutEnabled, user.LockoutEnd }).ToListAsync();
        var adminId = admins.Where(u => !u.LockoutEnabled || u.LockoutEnd is null || u.LockoutEnd <= acceptedAtUtc)
            .OrderBy(u => u.Id.StartsWith(DemoSessionSetup.Prefix)).Select(u => u.Id).FirstOrDefault();
        var localTime = TimeZoneInfo.ConvertTime(acceptedAtUtc, TimeZoneInfo.FindSystemTimeZoneById("Europe/Budapest"));
        var contact = adminId is not null
            ? "Probléma esetén írj ennek az eseménynek az Üzenetek beszélgetésében. Az eseményhez rendelt admin értesítést kap az új üzenetedről. Ez nem sürgősségi csatorna; válaszadási időt nem garantálunk."
            : "Jelenleg nincs aktív adminisztrátor a rendszerben. Probléma esetén fordulj a hozzáférésedet biztosító intézményhez vagy az alkalmazás üzemeltetőjéhez. Az esemény beszélgetése jelenleg nem felügyelt támogatási csatorna.";
        var activity = new Activity
        {
            Title = "Üdvözlünk az EgészségÚt világában!",
            Date = localTime.DateTime,
            CreatedAt = acceptedAtUtc.UtcDateTime,
            CreatedByUserId = adminId,
            Category = "Egyéb",
            Status = "Completed",
            City = "Budapest",
            Venue = "Deák Ferenc tér – bemutató térképpont (nem rendelő)",
            // Public square; coordinates checked against Budapest Közút's published Deák tér map points.
            Latitude = 47.497428,
            Longitude = 19.055135,
            Description = $"""
                Kedves {name.Trim()}!

                Gratulálunk, hogy csatlakoztál az EgészségÚt világához! Örülünk, hogy velünk indulsz el az átláthatóbb egészségügyi ügyintézés útján.
                Itt követheted a hozzád kapcsolódó eseményeket, dokumentumokat és értesítéseket.

                ELFOGADOTT NYILATKOZAT
                {policy.Title}
                Verzió: {policy.Version}
                Nyilatkozattevő: {signedName.Trim()}
                Elfogadás időpontja (budapesti idő): {localTime.ToString("yyyy. MM. dd. HH:mm:ss zzz", CultureInfo.InvariantCulture)}

                {policy.Highlight}

                {policy.Text}

                A fenti szöveg a regisztrációkor elfogadott nyilatkozat másolata. A naplózott elfogadási adat külön, a felhasználói fiókhoz kapcsolva is megmarad. Vizsgaremekhez készült minta; jogi felülvizsgálatig kizárólag fiktív adatot használj.

                TÉRKÉP
                Budapest, Deák Ferenc tér. Ez egy bemutató térképpont, nem egészségügyi intézmény vagy ügyfélszolgálati cím. Ez az üdvözlő esemény nem időpontfoglalás, személyes megjelenést nem igényel.

                KAPCSOLAT PROBLÉMA ESETÉN
                {contact}
                A beszélgetésre az esemény meglévő hozzáférési szabályai vonatkoznak; ne küldj ide jelszót vagy szükségtelen egészségügyi adatot.

                Üdvözlettel:
                EgészségÚt ADMIN
                Automatikus rendszerüzenet
                """
        };
        if (patientId is not null) activity.PatientActivities.Add(new() { ActivityId = activity.Id, PatientId = patientId });
        if (practitionerId is not null) activity.ActivityPractitioners.Add(new() { ActivityId = activity.Id, PractitionerId = practitionerId });
        db.Activities.Add(activity);
        db.UserNotifications.Add(new UserNotification
        {
            UserId = userId, Kind = "welcome", ActivityId = activity.Id, TargetId = activity.Id,
            Message = "Gratulálunk, hogy csatlakoztál az EgészségÚt világához! Megérkezett az üdvözlő eseményed az elfogadott nyilatkozattal.",
            CreatedAtUtc = acceptedAtUtc.UtcDateTime
        });
        return activity;
    }
}
