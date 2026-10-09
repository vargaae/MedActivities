using System.Linq.Expressions;
using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;
namespace Application.Activities.DTOs;

public class ActivityPersonDto
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
}

public class ActivityDto
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public DateTime Date { get; set; }
    public string Description { get; set; } = "";
    public string Category { get; set; } = "";
    public bool IsCancelled { get; set; }
    public string City { get; set; } = "";
    public string Venue { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool CanDelete { get; set; }
    public bool CanEditFields { get; set; }
    public bool CanEditAssignments { get; set; }
    public bool IsAppointment { get; set; }
    public bool IsRegistrationEvent { get; set; }
    public string Status { get; set; } = "";
    public List<ActivityPersonDto> Patients { get; set; } = [];
    public List<ActivityPersonDto> Practitioners { get; set; } = [];
    public static async Task MarkRegistrationEvents(AppDbContext db, List<ActivityDto> activities, CancellationToken ct)
    {
        var ids = activities.Select(a => a.Id).ToArray();
        var welcomeIds = await db.UserNotifications
            .Where(n => n.Kind == "welcome" && n.ActivityId != null && ids.Contains(n.ActivityId))
            .Select(n => n.ActivityId!).Distinct().ToListAsync(ct);
        foreach (var activity in activities) activity.IsRegistrationEvent = welcomeIds.Contains(activity.Id);
    }

    public static Expression<Func<Activity, ActivityDto>> Projection => a => new ActivityDto
    {
        Id=a.Id, Title=a.Title, Date=a.Date, Description=a.Description,
        Category=a.Category, IsCancelled=a.IsCancelled, City=a.City,
        Venue=a.Venue, Latitude=a.Latitude, Longitude=a.Longitude, Status=a.Status,
        Patients=a.PatientActivities.OrderBy(p=>p.Patient.Name).ThenBy(p=>p.PatientId)
            .Select(p=>new ActivityPersonDto {Id=p.PatientId,Name=p.Patient.Name}).ToList(),
        Practitioners=a.ActivityPractitioners.OrderBy(p=>p.Practitioner.Name).ThenBy(p=>p.PractitionerId)
            .Select(p=>new ActivityPersonDto {Id=p.PractitionerId,Name=p.Practitioner.Name}).ToList()
    };
}
