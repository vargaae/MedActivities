using Domain;
using Persistence;
namespace Application.Activities.Queries;

public static class ActivityVisibility
{
    public static IQueryable<Activity> For(AppDbContext db, string userId, string[] roles)
    {
        if (string.IsNullOrWhiteSpace(userId)) return db.Activities.Where(a => false);
        if (roles.Contains("Admin") || roles.Contains("AdmissionsOffice")) return db.Activities;
        var patient = roles.Contains("Patient"); var doctor = roles.Contains("Practitioner");
        return db.Activities.Where(a =>
            (patient && a.PatientActivities.Any(p => p.Patient.UserId == userId)) ||
            (doctor && a.ActivityPractitioners.Any(p => p.Practitioner.UserId == userId)));
    }
}
