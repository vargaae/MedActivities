using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence.Identity;

namespace Persistence;

public partial class AppDbContext
{
    // Notifications and their triggering change are committed in the same SaveChanges transaction.
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        var notifications = await BuildNotifications(cancellationToken);
        UserNotifications.AddRange(notifications);
        try { return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken); }
        catch
        {
            foreach (var notification in notifications) Entry(notification).State = EntityState.Detached;
            throw;
        }
    }

    private async Task<List<UserNotification>> BuildNotifications(CancellationToken ct)
    {
        var result = new List<UserNotification>();
        var changes = ChangeTracker.Entries().Where(e => e.State is EntityState.Added or EntityState.Modified).ToList();
        async Task Add(IEnumerable<string?> recipients, string? actor, string kind, string message,
            string? patientId = null, string? activityId = null, string? targetId = null)
        {
            var ids = recipients.Where(id => !string.IsNullOrWhiteSpace(id) && id != actor).Distinct().ToArray();
            if (ids.Length == 0) return;
            var existing = await Users.Where(u => ids.Contains(u.Id)).Select(u => u.Id).ToListAsync(ct);
            foreach (var id in existing)
                if (!result.Any(n => n.UserId == id && n.Kind == kind && n.TargetId == targetId))
                    result.Add(new() { UserId = id, Kind = kind, Message = message,
                        PatientId = patientId, ActivityId = activityId, TargetId = targetId });
        }
        async Task<List<string?>> PatientRecipients(string patientId)
        {
            var ids = await Patients.Where(p => p.Id == patientId).Select(p => p.UserId).ToListAsync(ct);
            ids.AddRange(await PatientPractitionerAccesses.Where(a => a.PatientId == patientId)
                .Select(a => (string?)a.Practitioner.UserId).ToListAsync(ct));
            return ids;
        }
        async Task<List<string?>> ActivityRecipients(string activityId)
        {
            var ids = await PatientActivities.Where(p => p.ActivityId == activityId).Select(p => p.Patient.UserId).ToListAsync(ct);
            ids.AddRange(await ActivityPractitioners.Where(p => p.ActivityId == activityId)
                .Select(p => (string?)p.Practitioner.UserId).ToListAsync(ct));
            ids.AddRange(await PatientPractitionerAccesses.Where(a => a.Patient.PatientActivities.Any(p => p.ActivityId == activityId))
                .Select(a => (string?)a.Practitioner.UserId).ToListAsync(ct));
            ids.AddRange(await Activities.Where(a => a.Id == activityId).Select(a => a.CreatedByUserId).ToListAsync(ct));
            return ids;
        }
        foreach (var entry in changes)
        {
            if (entry.Entity is ActivityComment comment && entry.State == EntityState.Added)
                await Add(await ActivityRecipients(comment.ActivityId), comment.UserId, "comment",
                    "Új üzenet érkezett egy hozzád kapcsolódó esemény beszélgetésében.", activityId: comment.ActivityId, targetId: comment.Id);
            if (string.IsNullOrEmpty(AuditUserId)) continue; // Seeding and migrations do not create alerts.
            switch (entry.Entity)
            {
                case Appointment appointment when entry.State == EntityState.Added && !appointment.PractitionerConfirmed:
                    var practitionerUserId = await Practitioners.Where(p => p.Id == appointment.PractitionerId)
                        .Select(p => (string?)p.UserId).SingleOrDefaultAsync(ct);
                    await Add([practitionerUserId], AuditUserId, "appointment-confirmation",
                        "Új időpontfoglalás várja a kezelőorvosi megerősítésedet.",
                        appointment.PatientId, appointment.ActivityId, appointment.Id);
                    break;
                case PatientDocument document when entry.State == EntityState.Added:
                    await Add(await PatientRecipients(document.PatientId), AuditUserId, "document",
                        "Új dokumentumot töltöttek fel egy hozzád kapcsolódó adatlapra.", document.PatientId, targetId: document.Id);
                    break;
                case PatientNote note:
                    await Add(await PatientRecipients(note.PatientId), AuditUserId, "note",
                        entry.State == EntityState.Added ? "Új megjegyzés érkezett egy hozzád kapcsolódó adatlaphoz." : "Módosítottak egy megjegyzést egy hozzád kapcsolódó adatlapon.",
                        note.PatientId, targetId: note.Id);
                    break;
                case Patient patient when entry.State == EntityState.Modified:
                    await Add([patient.UserId], AuditUserId, "patient-profile", "Módosították a páciensprofilod adatait.", patient.Id, targetId: patient.Id);
                    break;
                case PractitionerProfile doctor when entry.State == EntityState.Modified:
                    await Add([doctor.UserId], AuditUserId, "practitioner-profile", "Módosították a kezelőorvosi profilod adatait.", targetId: doctor.Id);
                    break;
                case AdminProfile admin when entry.State == EntityState.Modified:
                    await Add([admin.UserId], AuditUserId, "account", "Módosították a profilod adatait.", targetId: admin.UserId);
                    break;
                case AdmissionsOfficeProfile office when entry.State == EntityState.Modified:
                    await Add([office.UserId], AuditUserId, "account", "Módosították a profilod adatait.", targetId: office.UserId);
                    break;
                case AppUser user when entry.State == EntityState.Modified && entry.Properties.Any(p => p.IsModified &&
                    p.Metadata.Name is "UserName" or "Email" or "PhoneNumber" or "PasswordHash" or "LockoutEnd"):
                    await Add([user.Id], AuditUserId, "account", "Módosították a felhasználói fiókod adatait vagy beállításait.", targetId: user.Id);
                    break;
                case Activity activity when entry.State == EntityState.Modified:
                    await Add(await ActivityRecipients(activity.Id), AuditUserId, "activity",
                        "Módosítottak egy hozzád kapcsolódó eseményt.", activityId: activity.Id, targetId: activity.Id);
                    break;
            }
        }
        return result;
    }
}
