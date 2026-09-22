using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace API.Med;

[ApiController, Route("api/notifications"), Authorize]
public class NotificationsController(AppDbContext db, AccessService access) : ControllerBase
{
    [HttpGet("unread-count")]
    public async Task<IActionResult> Count(CancellationToken ct) => Ok(new {
        unreadCount = await db.UserNotifications.CountAsync(n => n.UserId == access.UserId && n.ReadAtUtc == null, ct)
    });

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct, int page = 1, int pageSize = 10, bool unreadOnly = false)
    {
        if (page < 1 || page > 100000 || pageSize < 1 || pageSize > 100) return BadRequest();
        var query = db.UserNotifications.AsNoTracking().Where(n => n.UserId == access.UserId);
        if (unreadOnly) query = query.Where(n => n.ReadAtUtc == null);
        var totalCount = await query.CountAsync(ct);
        return Ok(new { totalCount, page, pageSize, items = await query.OrderByDescending(n => n.CreatedAtUtc).ThenByDescending(n => n.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(n => new { n.Id, n.Message, n.Kind, n.CreatedAtUtc, n.ReadAtUtc }).ToListAsync(ct) });
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> Read(string id, CancellationToken ct)
    {
        var notification = await db.UserNotifications.SingleOrDefaultAsync(n => n.Id == id && n.UserId == access.UserId, ct);
        if (notification is null) return NotFound();
        string? url = null;
        if (notification.Kind == "appointment-confirmation")
            url = "/booking?appointmentId=" + Uri.EscapeDataString(notification.TargetId ?? "");
        else if (notification.ActivityId is { } activityId && await access.Activities().AnyAsync(a => a.Id == activityId, ct))
            url = "/activities/" + Uri.EscapeDataString(activityId) +
                (notification.Kind == "comment" ? "?comment=" + Uri.EscapeDataString(notification.TargetId ?? "") + "#chat" : "");
        else if (notification.PatientId is { } patientId && await access.Patients().AnyAsync(p => p.Id == patientId, ct))
            url = "/health-records?patientId=" + Uri.EscapeDataString(patientId) + "&target=" + Uri.EscapeDataString(notification.TargetId ?? "") +
                (notification.Kind == "document" ? "#documents" : notification.Kind == "note" ? "#notes" : "#profile");
        else if (notification.Kind == "practitioner-profile" && await db.Practitioners.AnyAsync(p => p.Id == notification.TargetId && p.UserId == access.UserId, ct))
            url = "/practitioners?practitionerId=" + Uri.EscapeDataString(notification.TargetId!);
        else if (notification.Kind == "account") url = "/notifications?account=updated";
        notification.ReadAtUtc ??= DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Ok(new { url, message = url is null ? "A kapcsolódó elem már nem érhető el, vagy megszűnt a hozzáférésed." : null });
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> ReadAll(CancellationToken ct)
    {
        await db.UserNotifications.Where(n => n.UserId == access.UserId && n.ReadAtUtc == null)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.ReadAtUtc, DateTime.UtcNow), ct);
        return NoContent();
    }
}
