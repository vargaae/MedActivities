using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Identity;

namespace API.Med;

/// <summary>Explicit, repeatable development fixture. Never replaces unrelated records.</summary>
public static class DemoDataSeeder
{
    public const int PatientCount = 100, PractitionerCount = 50, ActivityCount = 1000;
    private const string Prefix = DemoSessionSetup.Prefix;
    private static string PatientId(int i) => i == 0 ? Prefix + "patient-profile" : Prefix + $"patient-{i + 1:000}";
    private static string DoctorId(int i) => i == 0 ? Prefix + "doctor-profile" : Prefix + $"doctor-{i + 1:000}";
    private static string EventId(int i) => i == 0 ? Prefix + "activity" : Prefix + $"activity-{i + 1:0000}";

    public static async Task<object> Report(AppDbContext db) => new {
        database = db.Database.GetDbConnection().Database,
        provider = db.Database.ProviderName,
        demo = new {
            patients = await db.Patients.CountAsync(p => p.Id.StartsWith(Prefix)),
            practitioners = await db.Practitioners.CountAsync(p => p.Id.StartsWith(Prefix)),
            activities = await db.Activities.CountAsync(p => p.Id.StartsWith(Prefix))
        },
        total = new {
            patients = await db.Patients.CountAsync(),
            practitioners = await db.Practitioners.CountAsync(),
            activities = await db.Activities.CountAsync()
        }
    };

    public static async Task SeedAsync(AppDbContext db, UserManager<AppUser> users, RoleManager<IdentityRole> roles)
    {
        await DemoSessionSetup.Gate.WaitAsync();
        try {
            await using var transaction = await db.Database.BeginTransactionAsync();
            await DemoSessionSetup.SeedAsync(db, users, roles);
            var existingPatients = (await db.Patients.Select(p => p.Id).ToListAsync()).ToHashSet();
            var existingDoctors = (await db.Practitioners.Select(p => p.Id).ToListAsync()).ToHashSet();
            var existingEvents = (await db.Activities.Select(a => a.Id).ToListAsync()).ToHashSet();
            // A megadott számok teljes céldarabszámok. A más forrásból származó rekordok megmaradnak.
            var patientCount = Math.Max(1, PatientCount - existingPatients.Count(id => !id.StartsWith(Prefix, StringComparison.Ordinal)));
            var practitionerCount = Math.Max(1, PractitionerCount - existingDoctors.Count(id => !id.StartsWith(Prefix, StringComparison.Ordinal)));
            var activityCount = Math.Max(1, ActivityCount - existingEvents.Count(id => !id.StartsWith(Prefix, StringComparison.Ordinal)));
            var familyNames = new[] { "Minta", "Teszt", "Példa", "Próba", "Demó", "Mintás", "Próbás", "Példás", "Tesztelő", "Bemutató" };
            var givenNames = new[] { "Anna", "Péter", "Júlia", "Gábor", "Eszter", "Tamás", "Dóra", "Márton", "Katalin", "Ádám" };
            var locations = new[] {
                ("Budapest", 47.4979, 19.0402), ("Debrecen", 47.5316, 21.6273),
                ("Szeged", 46.2530, 20.1414), ("Pécs", 46.0727, 18.2323),
                ("Győr", 47.6875, 17.6504)
            };
            var specialties = new[] { "Ortopédia", "Gyógytorna", "Reumatológia", "Belgyógyászat", "Kardiológia",
                "Neurológia", "Rehabilitáció", "Diagnosztika", "Dietetika", "Sportorvoslás" };

            for (var i = 1; i < patientCount; i++) {
                var id = PatientId(i);
                if (existingPatients.Contains(id)) continue;
                var userId = Prefix + $"patient-user-{i + 1:000}";
                await EnsureUser(users, userId, $"demo.egeszsegut.patient{i + 1:000}", "Patient");
                var location = locations[i % locations.Length];
                db.Patients.Add(new Patient {
                    Id = id, UserId = userId,
                    Name = $"{familyNames[i / 10]} {givenNames[i % 10]} (demó {i + 1:000})",
                    TajNumber = TestTaj(91000000 + i),
                    BirthDate = new DateOnly(1945 + i % 60, 1 + i % 12, 1 + i % 27),
                    Email = $"patient{i + 1:000}@demo.example.invalid",
                    Address = $"{location.Item1}, Demó utca {i + 1}.",
                    Notes = "Generált, fiktív vizsgaadat. Nem valódi személy és nem használható valódi TAJ-ként."
                });
            }
            for (var i = 1; i < practitionerCount; i++) {
                var id = DoctorId(i);
                if (existingDoctors.Contains(id)) continue;
                var userId = Prefix + $"doctor-user-{i + 1:000}";
                await EnsureUser(users, userId, $"demo.egeszsegut.practitioner{i + 1:000}", "Practitioner");
                var location = locations[i % locations.Length];
                db.Practitioners.Add(new PractitionerProfile {
                    Id = id, UserId = userId,
                    Name = $"Dr. {familyNames[i / 10]} {givenNames[i % 10]} (demó {i + 1:000})",
                    TajNumber = TestTaj(92000000 + i), Specialty = specialties[i % specialties.Length],
                    City = location.Item1, Venue = $"EgészségÚt demórendelő {i + 1:000}"
                });
            }
            await db.SaveChangesAsync();

            var settings = (await db.PractitionerBookingSettings.Select(s => s.PractitionerId).ToListAsync()).ToHashSet();
            var hours = (await db.PractitionerWorkingHours.Select(h => new { h.PractitionerId, h.DayOfWeek }).ToListAsync())
                .Select(h => (h.PractitionerId, h.DayOfWeek)).ToHashSet();
            for (var i = 0; i < practitionerCount; i++) {
                var doctor = DoctorId(i);
                if (!settings.Contains(doctor))
                    db.PractitionerBookingSettings.Add(new() { Id = Prefix + $"booking-{i + 1:000}", PractitionerId = doctor, BookingEnabled = true });
                for (var day = 0; day < 7; day++) {
                    if (hours.Contains((doctor, (DayOfWeek)day))) continue;
                    db.PractitionerWorkingHours.Add(new() {
                        Id = Prefix + $"hours-{i + 1:000}-{day}", PractitionerId = doctor, DayOfWeek = (DayOfWeek)day,
                        IsWorkingDay = day is >= 1 and <= 5, StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(16, 0)
                    });
                }
            }

            var grants = (await db.PatientPractitionerAccesses.Select(g => new { g.PatientId, g.PractitionerId }).ToListAsync())
                .Select(g => (g.PatientId, g.PractitionerId)).ToHashSet();
            for (var i = 0; i < patientCount; i++) {
                var pair = (PatientId(i), DoctorId(i % practitionerCount));
                if (!grants.Contains(pair))
                    db.PatientPractitionerAccesses.Add(new() { PatientId = pair.Item1, PractitionerId = pair.Item2 });
            }
            var titles = new[] { "Első konzultáció", "Állapotfelmérés", "Laborvizsgálat", "Diagnosztikai vizsgálat",
                "Szakorvosi kontroll", "Kezelés", "Gyógytorna", "Rehabilitációs kontroll", "Utánkövetés", "Éves kontroll" };
            var categories = new[] { "Konzultáció", "Vizsgálat", "Labor", "Képalkotó vizsgálat",
                "Kontroll", "Kezelés", "Gyógytorna", "Kontroll", "Konzultáció", "Kontroll" };
            var today = BookingService.LocalNow().Date;
            for (var i = 0; i < activityCount; i++) {
                var id = EventId(i);
                if (existingEvents.Contains(id)) continue;
                var patientIndex = i % patientCount;
                var episode = (i / patientCount) % titles.Length;
                var doctorIndex = patientIndex % practitionerCount;
                var location = locations[doctorIndex % locations.Length];
                var date = today.AddDays(-120 + episode * 20 + patientIndex % 7).AddHours(8 + patientIndex % 8);
                var status = i % 17 == 0 ? "Cancelled" : date < today ? (i % 23 == 0 ? "NoShow" : "Completed") : "Scheduled";
                var activity = new Activity {
                    Id = id, Title = $"{titles[episode]} – demó {i + 1:0000}", Date = date,
                    Description = $"Fiktív betegút {episode + 1}. állomása. {specialties[doctorIndex % specialties.Length]} – kizárólag bemutató célú adat.",
                    Category = categories[episode], Status = status, IsCancelled = status == "Cancelled",
                    City = location.Item1, Venue = $"EgészségÚt demórendelő {doctorIndex + 1:000}",
                    Latitude = location.Item2, Longitude = location.Item3, CreatedByUserId = Prefix + "Admin"
                };
                activity.PatientActivities.Add(new() { ActivityId = id, PatientId = PatientId(patientIndex) });
                activity.ActivityPractitioners.Add(new() { ActivityId = id, PractitionerId = DoctorId(doctorIndex) });
                db.Activities.Add(activity);
            }
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        } finally { DemoSessionSetup.Gate.Release(); }
    }

    private static async Task EnsureUser(UserManager<AppUser> users, string id, string name, string role)
    {
        var user = await users.FindByIdAsync(id);
        if (user is null) {
            user = new AppUser { Id = id, UserName = name, Email = name + "@demo.example.invalid", EmailConfirmed = true };
            MedSetup.Check(await users.CreateAsync(user));
        }
        if (user.UserName != name) throw new InvalidOperationException("Foglalt demófiók-azonosító: " + id);
        var assigned = await users.GetRolesAsync(user);
        if (assigned.Any(r => r != role)) throw new InvalidOperationException("Eltérő szerepkörű demófiók: " + id);
        if (!assigned.Contains(role)) MedSetup.Check(await users.AddToRoleAsync(user, role));
    }

    private static string TestTaj(int value)
    {
        var digits = value.ToString("D8");
        var sum = digits.Select((digit, i) => (digit - '0') * (i % 2 == 0 ? 3 : 7)).Sum();
        return digits + (sum % 10);
    }
}
