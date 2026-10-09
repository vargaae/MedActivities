using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace API.Med;

[ApiController, Authorize, Route("api/activities/{activityId}/documents")]
public class ActivityDocumentsController(AppDbContext db, AccessService access) : ControllerBase
{
    private Task<bool> Visible(string id, CancellationToken ct) => access.Activities().AnyAsync(a => a.Id == id, ct);

    [HttpGet]
    public async Task<IActionResult> List(string activityId, CancellationToken ct)
    {
        if (!await Visible(activityId, ct)) return NotFound();
        Response.Headers.CacheControl = "no-store";
        return Ok(await db.ActivityDocuments.AsNoTracking().Where(d => d.ActivityId == activityId)
            .OrderByDescending(d => d.CreatedAt).ThenBy(d => d.Id)
            .Select(d => new { d.Id, d.Title, d.FileName, d.ContentType, d.CreatedAt,
                d.UploadedByName, Size = d.Content.Length }).ToListAsync(ct));
    }

    [HttpPost, RequestSizeLimit(6 * 1024 * 1024), RequestFormLimits(MultipartBodyLengthLimit = 6 * 1024 * 1024)]
    public async Task<IActionResult> Upload(string activityId, [FromForm] string title, IFormFile file, CancellationToken ct)
    {
        if (!await Visible(activityId, ct)) return NotFound();
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 200)
            return BadRequest(new { message = "A dokumentum címe 1–200 karakter legyen." });
        if (file.Length is <= 0 or > 5 * 1024 * 1024)
            return BadRequest(new { message = "A dokumentum mérete 1 bájt és 5 MB között lehet." });
        var name = Path.GetFileName(file.FileName.Replace('\\', '/'));
        if (string.IsNullOrWhiteSpace(name) || name.Length > 255 || name.Any(char.IsControl))
            return BadRequest(new { message = "Érvénytelen fájlnév." });
        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream, ct);
        var bytes = stream.ToArray();
        var type = PatientRecordsController.ContentTypeFor(name, bytes);
        if (type is null) return BadRequest(new { message = "PDF, PNG, JPEG vagy UTF-8 szöveg tölthető fel, megfelelő fájltartalommal." });
        var userId = access.UserId;
        var uploader = await db.Patients.Where(p => p.UserId == userId).Select(p => p.Name).FirstOrDefaultAsync(ct)
            ?? await db.Practitioners.Where(p => p.UserId == userId).Select(p => p.Name).FirstOrDefaultAsync(ct)
            ?? await db.AdminProfiles.Where(p => p.UserId == userId).Select(p => p.Name).FirstOrDefaultAsync(ct)
            ?? await db.AdmissionsOfficeProfiles.Where(p => p.UserId == userId).Select(p => p.Name).FirstOrDefaultAsync(ct)
            ?? await db.Users.Where(u => u.Id == userId).Select(u => u.UserName).SingleAsync(ct) ?? "Felhasználó";
        var document = new ActivityDocument { ActivityId = activityId, Title = title.Trim(), FileName = name,
            ContentType = type, Content = bytes, UploadedByUserId = userId, UploadedByName = uploader };
        db.ActivityDocuments.Add(document);
        await db.SaveChangesAsync(ct);
        return StatusCode(201, new { document.Id });
    }

    [HttpGet("{id}/content")]
    public async Task<IActionResult> Download(string activityId, string id, CancellationToken ct)
    {
        if (!await Visible(activityId, ct)) return NotFound();
        var document = await db.ActivityDocuments.AsNoTracking().SingleOrDefaultAsync(d => d.ActivityId == activityId && d.Id == id, ct);
        if (document is null) return NotFound();
        Response.Headers.CacheControl = "no-store";
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(document.Content, document.ContentType, document.FileName);
    }
}
