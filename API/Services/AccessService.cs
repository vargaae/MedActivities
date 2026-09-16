using System.Security.Claims;
using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;
namespace API.Med;
public class AccessService(AppDbContext db, IHttpContextAccessor http)
{
    public ClaimsPrincipal User => http.HttpContext!.User;
    public string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
    public bool Admin => User.IsInRole("Admin");
    public bool Staff => Admin || User.IsInRole("AdmissionsOffice");
    public IQueryable<Patient> Patients() => Staff ? db.Patients : db.Patients.Where(p =>
        (User.IsInRole("Patient") && p.UserId == UserId) ||
        (User.IsInRole("Practitioner") && p.PractitionerAccesses.Any(a => a.Practitioner.UserId == UserId)));
    public Task<bool> OwnPatient(string id) => db.Patients.AnyAsync(p => p.Id == id && p.UserId == UserId && User.IsInRole("Patient"));
    public Task<bool> OwnPractitioner(string id) => db.Practitioners.AnyAsync(p => p.Id == id && p.UserId == UserId && User.IsInRole("Practitioner"));
    public Task<bool> AssignedPatient(string id) => db.Patients.AnyAsync(p => p.Id == id &&
        User.IsInRole("Practitioner") && p.PractitionerAccesses.Any(a => a.Practitioner.UserId == UserId));
    public IQueryable<Appointment> Appointments() => Staff ? db.Appointments : db.Appointments.Where(a =>
        (User.IsInRole("Patient") && a.Patient.UserId == UserId) ||
        (User.IsInRole("Practitioner") && a.Practitioner.UserId == UserId));
    public IQueryable<Activity> Activities() => Application.Activities.Queries.ActivityVisibility.For(db, UserId,
        User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray());
    public IQueryable<Activity> EditableActivities() => Staff ? db.Activities : db.Activities.Where(a =>
        (User.IsInRole("Patient") && a.PatientActivities.Any(p => p.Patient.UserId == UserId)) ||
        (User.IsInRole("Practitioner") && a.ActivityPractitioners.Any(p => p.Practitioner.UserId == UserId)));
}
