using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence.Identity;

namespace Persistence;

public partial class AppDbContext
{
    // Notifications and their triggering change are committed in the same SaveChanges transaction.
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        if (SuppressNotifications) return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
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
            string? patientId = null, string? activityId = null, string? targetId = null,
            string? actorMessage = null, string? adminMessage = null)
        {
            var ids = recipients.Append(actor).Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
            var adminIds = await (from ur in UserRoles
                                  join role in Roles on ur.RoleId equals role.Id
                                  join user in Users on ur.UserId equals user.Id
                                  where role.NormalizedName == "ADMIN" && (user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow)
                                  select ur.UserId).ToListAsync(ct);
            if (adminMessage is null)
            {
                var contextParts = new List<string>();
                if (patientId is not null)
                {
                    var name = await Patients.Where(p => p.Id == patientId).Select(p => p.Name).SingleOrDefaultAsync(ct);
                    if (!string.IsNullOrWhiteSpace(name)) contextParts.Add($"Páciens: {name}");
                }
                if (activityId is not null)
                {
                    var title = await Activities.Where(a => a.Id == activityId).Select(a => a.Title).SingleOrDefaultAsync(ct);
                    if (!string.IsNullOrWhiteSpace(title)) contextParts.Add($"Esemény: {title}");
                }
                if (kind == "practitioner-profile" && targetId is not null)
                {
                    var name = await Practitioners.Where(p => p.Id == targetId).Select(p => p.Name).SingleOrDefaultAsync(ct);
                    if (!string.IsNullOrWhiteSpace(name)) contextParts.Add($"Kezelőorvos: {name}");
                }
                if (kind == "account" && targetId is not null)
                {
                    var name = await Users.Where(u => u.Id == targetId).Select(u => u.UserName).SingleOrDefaultAsync(ct);
                    if (!string.IsNullOrWhiteSpace(name)) contextParts.Add($"Felhasználó: {name}");
                }
                if (contextParts.Count > 0) adminMessage = message + " " + string.Join("; ", contextParts) + ".";
            }
            ids.AddRange(adminIds);
            ids = ids.Distinct().ToList();
            if (ids.Count == 0) return;
            var existing = await Users.Where(u => ids.Contains(u.Id)).Select(u => u.Id).ToListAsync(ct);
            foreach (var id in existing)
                // Egy SaveChanges hívás egy címzettnek csak egy értesítést eredményezzen.
                if (!result.Any(n => n.UserId == id))
                    result.Add(new()
                    {
                        UserId = id,
                        Kind = kind,
                        Message = adminIds.Contains(id) && adminMessage is not null ? adminMessage
                            : id == actor && actorMessage is not null ? actorMessage : message,
                        PatientId = patientId,
                        ActivityId = activityId,
                        TargetId = targetId
                    });
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
            // Include unsaved links, and both sides of assignment changes.
            var patientIds = ChangeTracker.Entries<PatientActivity>().Where(e => e.Entity.ActivityId == activityId)
                .Select(e => e.Entity.PatientId).Distinct().ToArray();
            foreach (var patientId in patientIds) ids.AddRange(await PatientRecipients(patientId));
            var doctorIds = ChangeTracker.Entries<ActivityPractitioner>().Where(e => e.Entity.ActivityId == activityId)
                .Select(e => e.Entity.PractitionerId).Distinct().ToArray();
            ids.AddRange(await Practitioners.Where(p => doctorIds.Contains(p.Id)).Select(p => (string?)p.UserId).ToListAsync(ct));
            if (await UserNotifications.AnyAsync(n => n.ActivityId == activityId && n.Kind == "welcome", ct))
                ids.AddRange(await (from ur in UserRoles
                                    join role in Roles on ur.RoleId equals role.Id
                                    where role.NormalizedName == "ADMIN"
                                    select (string?)ur.UserId).ToListAsync(ct));
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
                case Appointment appointment:
                    var practitionerUserId = await Practitioners.Where(p => p.Id == appointment.PractitionerId)
                        .Select(p => (string?)p.UserId).SingleOrDefaultAsync(ct);
                    var bookingRecipients = await ActivityRecipients(appointment.ActivityId);
                    bookingRecipients.Add(practitionerUserId);
                    bookingRecipients.AddRange(await PatientRecipients(appointment.PatientId));
                    await Add(bookingRecipients, AuditUserId, "appointment",
                        entry.State == EntityState.Added ? "Új időpontfoglalás készült egy hozzád kapcsolódó eseményhez."
                        : appointment.Status == AppointmentStatus.Cancelled ? "Lemondtak egy hozzád kapcsolódó időpontfoglalást."
                        : "Megváltozott egy hozzád kapcsolódó időpontfoglalás időpontja, állapota vagy adata.",
                        appointment.PatientId, appointment.ActivityId, appointment.Id);
                    if (entry.State == EntityState.Added && !appointment.PractitionerConfirmed)
                        await Add([practitionerUserId], null, "appointment-confirmation",
                        "Új időpontfoglalás várja a kezelőorvosi megerősítésedet.",
                        appointment.PatientId, appointment.ActivityId, appointment.Id);
                    break;
                case ActivityDocument document when entry.State == EntityState.Added:
                    var activityTitle = await Activities.Where(a => a.Id == document.ActivityId)
                        .Select(a => a.Title).SingleOrDefaultAsync(ct) ?? "az eseményhez";
                    await Add(await ActivityRecipients(document.ActivityId), document.UploadedByUserId, "activity-document",
                        $"{document.UploadedByName} dokumentumot töltött fel egy hozzád kapcsolódó eseményhez.",
                        activityId: document.ActivityId, targetId: document.Id,
                        actorMessage: $"Feltöltöttél egy dokumentumot a következő eseményhez: {activityTitle}");
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
                    await Add([patient.UserId], AuditUserId, "patient-profile", "Módosítva lett a páciensprofil adatai.", patient.Id, targetId: patient.Id);
                    break;
                case PractitionerProfile doctor when entry.State == EntityState.Modified:
                    await Add([doctor.UserId], AuditUserId, "practitioner-profile", "Módosítva lett a kezelőorvosi profil adatai.", targetId: doctor.Id);
                    break;
                case AdminProfile admin when entry.State == EntityState.Modified:
                    await Add([admin.UserId], AuditUserId, "account", "Módosítva lett az admin profil adatai.", targetId: admin.UserId);
                    break;
                case AdmissionsOfficeProfile office when entry.State == EntityState.Modified:
                    await Add([office.UserId], AuditUserId, "account", "Módosítva lett a profil adatai.", targetId: office.UserId);
                    break;
                case AppUser user when entry.State == EntityState.Modified && entry.Properties.Any(p => p.IsModified &&
                    p.Metadata.Name is "UserName" or "Email" or "PhoneNumber" or "PasswordHash" or "LockoutEnd"):
                    await Add([user.Id], AuditUserId, "account", "Módosítva lett egy felhasználói fiók adatai vagy beállításai.", targetId: user.Id);
                    break;
                case Activity activity when !changes.Any(e => e.Entity is Appointment a && a.ActivityId == activity.Id)
                    && !ChangeTracker.Entries<UserNotification>().Any(e => e.Entity.ActivityId == activity.Id && e.Entity.Kind == "welcome"):
                    await Add(await ActivityRecipients(activity.Id), AuditUserId, "activity",
                        entry.State == EntityState.Added ? "Új, hozzád kapcsolódó esemény jött létre." : "Módosítva lett egy hozzád kapcsolódó esemény.",
                        activityId: activity.Id, targetId: activity.Id);
                    break;
            }
        }
        return result;
    }
}
