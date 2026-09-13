using System.ComponentModel.DataAnnotations;
using System.Text;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace API.Med;

public record NoteInput([Required, MaxLength(2000)] string Text);
public record DocumentTitleInput([Required, MaxLength(200)] string Title);

[ApiController, Route("api/patients/{patientId}/records"), Authorize]
public class PatientRecordsController(AppDbContext db, AccessService access) : ControllerBase
{
    private Task<bool> Visible(string patientId) => access.Patients().AnyAsync(p => p.Id == patientId);
    private bool CanEdit(string ownerId) => access.Staff || ownerId == access.UserId;

    [HttpGet("notes")]
    public async Task<IActionResult> Notes(string patientId)
    {
        if (!await Visible(patientId)) return NotFound();
        return Ok(await db.PatientNotes.AsNoTracking().Where(n => n.PatientId == patientId)
            .OrderByDescending(n => n.CreatedAt).Select(n => new
            {
                n.Id, n.Text, n.CreatedAt, n.UpdatedAt,
                Author = db.Users.Where(u => u.Id == n.AuthorUserId).Select(u => u.UserName).FirstOrDefault(),
                CanEdit = access.Staff || n.AuthorUserId == access.UserId
            }).ToListAsync());
    }

    [HttpPost("notes")]
    public async Task<IActionResult> AddNote(string patientId, NoteInput input)
    {
        if (!await Visible(patientId)) return NotFound();
        if (string.IsNullOrWhiteSpace(input.Text)) return BadRequest(new { message = "A megjegyzés nem lehet üres." });
        var note = new PatientNote { PatientId = patientId, Text = input.Text.Trim(), AuthorUserId = access.UserId };
        db.PatientNotes.Add(note); await db.SaveChangesAsync();
        return StatusCode(201, new { note.Id });
    }

    [HttpPut("notes/{id}")]
    public async Task<IActionResult> EditNote(string patientId, string id, NoteInput input)
    {
        if (!await Visible(patientId)) return NotFound();
        var note = await db.PatientNotes.SingleOrDefaultAsync(n => n.PatientId == patientId && n.Id == id);
        if (note is null) return NotFound();
        if (!CanEdit(note.AuthorUserId)) return Forbid();
        if (string.IsNullOrWhiteSpace(input.Text)) return BadRequest(new { message = "A megjegyzés nem lehet üres." });
        note.Text = input.Text.Trim(); note.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(); return NoContent();
    }

    [HttpDelete("notes/{id}")]
    public async Task<IActionResult> DeleteNote(string patientId, string id)
    {
        if (!await Visible(patientId)) return NotFound();
        var note = await db.PatientNotes.SingleOrDefaultAsync(n => n.PatientId == patientId && n.Id == id);
        if (note is null) return NotFound();
        if (!CanEdit(note.AuthorUserId)) return Forbid();
        db.PatientNotes.Remove(note); await db.SaveChangesAsync(); return NoContent();
    }

    [HttpGet("documents")]
    public async Task<IActionResult> Documents(string patientId)
    {
        if (!await Visible(patientId)) return NotFound();
        return Ok(await db.PatientDocuments.AsNoTracking().Where(d => d.PatientId == patientId)
            .OrderByDescending(d => d.CreatedAt).Select(d => new
            {
                d.Id, d.Title, d.FileName, d.ContentType, Size = d.Content.Length, d.CreatedAt,
                CanEdit = access.Staff || d.UploadedByUserId == access.UserId
            }).ToListAsync());
    }

    [HttpPost("documents"), RequestSizeLimit(6 * 1024 * 1024), RequestFormLimits(MultipartBodyLengthLimit = 6 * 1024 * 1024)]
    public async Task<IActionResult> Upload(string patientId, [FromForm] string title, IFormFile file, CancellationToken ct)
    {
        if (!await Visible(patientId)) return NotFound();
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
        var type = ContentTypeFor(name, bytes);
        if (type is null) return BadRequest(new { message = "PDF, PNG, JPEG vagy UTF-8 szöveg tölthető fel, megfelelő fájltartalommal." });
        var document = new PatientDocument
        {
            PatientId = patientId, Title = title.Trim(), FileName = name, ContentType = type,
            Content = bytes, UploadedByUserId = access.UserId
        };
        db.PatientDocuments.Add(document); await db.SaveChangesAsync(ct);
        return StatusCode(201, new { document.Id });
    }

    [HttpGet("documents/{id}/content")]
    public async Task<IActionResult> Download(string patientId, string id)
    {
        if (!await Visible(patientId)) return NotFound();
        var document = await db.PatientDocuments.AsNoTracking().SingleOrDefaultAsync(d => d.PatientId == patientId && d.Id == id);
        if (document is null) return NotFound();
        Response.Headers.CacheControl = "no-store";
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(document.Content, document.ContentType, document.FileName);
    }

    [HttpPut("documents/{id}")]
    public async Task<IActionResult> EditDocument(string patientId, string id, DocumentTitleInput input)
    {
        if (!await Visible(patientId)) return NotFound();
        var document = await db.PatientDocuments.SingleOrDefaultAsync(d => d.PatientId == patientId && d.Id == id);
        if (document is null) return NotFound();
        if (!CanEdit(document.UploadedByUserId)) return Forbid();
        if (string.IsNullOrWhiteSpace(input.Title)) return BadRequest(new { message = "A cím kötelező." });
        document.Title = input.Title.Trim(); await db.SaveChangesAsync(); return NoContent();
    }

    [HttpDelete("documents/{id}")]
    public async Task<IActionResult> DeleteDocument(string patientId, string id)
    {
        if (!await Visible(patientId)) return NotFound();
        var document = await db.PatientDocuments.SingleOrDefaultAsync(d => d.PatientId == patientId && d.Id == id);
        if (document is null) return NotFound();
        if (!CanEdit(document.UploadedByUserId)) return Forbid();
        db.PatientDocuments.Remove(document); await db.SaveChangesAsync(); return NoContent();
    }

    private static string? ContentTypeFor(string name, byte[] bytes)
    {
        var ext = Path.GetExtension(name).ToLowerInvariant();
        if (ext == ".pdf" && bytes.AsSpan().StartsWith("%PDF-"u8)) return "application/pdf";
        if (ext == ".png" && bytes.AsSpan().StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })) return "image/png";
        if (ext is ".jpg" or ".jpeg" && bytes.AsSpan().StartsWith(new byte[] { 255, 216, 255 })) return "image/jpeg";
        if (ext == ".txt")
        {
            try
            {
                var text = new UTF8Encoding(false, true).GetString(bytes);
                if (text.All(c => !char.IsControl(c) || c is '\r' or '\n' or '\t')) return "text/plain";
            }
            catch (DecoderFallbackException) { }
        }
        return null;
    }
}
