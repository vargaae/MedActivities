using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Identity;
using Microsoft.AspNetCore.Identity;
namespace API.Med;
[ApiController, Route("api/patients"), Authorize]
public class PatientsController(AppDbContext db, AccessService access, UserManager<AppUser> users) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> List() => Ok(await access.Patients().AsNoTracking().OrderBy(p => p.Name).ThenBy(p=>p.Id).Select(p => new {p.Id,p.Name,p.TajNumber,p.BirthDate,p.BirthPlace,p.Email,p.Phone,p.Address,p.Notes,p.UserId}).ToListAsync());
    [HttpGet("directory", Order = -1), Authorize(Roles="Admin,AdmissionsOffice,Practitioner")]
    public async Task<IActionResult> Directory() {
        var assigned = await access.Patients().AsNoTracking().OrderBy(p=>p.Name).ThenBy(p=>p.Id)
            .Select(p=>new {p.Id,p.Name,p.TajNumber,p.BirthDate,p.BirthPlace,p.Email,p.Phone,p.Address,p.Notes,p.UserId}).ToListAsync();
        var all = await db.Patients.AsNoTracking().OrderBy(p=>p.Name).ThenBy(p=>p.Id)
            .Select(p=>new {p.Id,p.Name,p.TajNumber,p.BirthDate,p.BirthPlace,p.Email,p.Phone,p.Address,p.Notes,p.UserId}).ToListAsync();
        return Ok(new { assigned, all });
    }
    [HttpGet("{id}")] public async Task<IActionResult> Details(string id) {
        var p = await access.Patients().AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        return p is null ? NotFound() : Ok(new {p.Id,p.Name,p.TajNumber,p.BirthDate,p.BirthPlace,p.Email,p.Phone,p.Address,p.Notes,p.UserId});
    }
    [HttpPost, Authorize(Roles="Admin,AdmissionsOffice")]
    public async Task<IActionResult> Create(PatientInput input) {
        if (input.BirthDate > DateOnly.FromDateTime(DateTime.Today) || input.BirthDate == default) return BadRequest("Hibás születési dátum.");
        if (await db.Patients.AnyAsync(p => p.TajNumber == input.TajNumber)) return Conflict(new { message = "Ez a TAJ-szám már szerepel a páciensek között." });
        var p = new Patient { Name=input.Name.Trim(), TajNumber=input.TajNumber };
        Apply(p,input); db.Patients.Add(p); await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Details),new {id=p.Id},new {p.Id});
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> Edit(string id, PatientInput input) {
        if (input.BirthDate > DateOnly.FromDateTime(DateTime.Today) || input.BirthDate == default) return BadRequest("Hibás születési dátum.");
        var p=await db.Patients.FindAsync(id); if(p is null) return NotFound();
        if(!access.Staff && !await access.OwnPatient(id)) return Forbid();
        if(!access.Staff) { input = input with { TajNumber = p.TajNumber, Notes = p.Notes }; }
        if (await db.Patients.AnyAsync(p => p.Id != id && p.TajNumber == input.TajNumber)) return Conflict(new { message = "Ez a TAJ-szám már másik pácienshez tartozik." });
        Apply(p,input); await db.SaveChangesAsync(); return NoContent();
    }
    [HttpDelete("{id}"), Authorize(Roles="Admin,AdmissionsOffice")]
    public async Task<IActionResult> Delete(string id) {
        var p=await db.Patients.FindAsync(id); if(p is null) return NotFound();
        if(await db.Appointments.AnyAsync(a=>a.PatientId==id) || await db.PatientActivities.AnyAsync(a=>a.PatientId==id) ||
            await db.PatientNotes.AnyAsync(n=>n.PatientId==id) || await db.PatientDocuments.AnyAsync(d=>d.PatientId==id))
            return Conflict(new { message = "Eseményhez, foglaláshoz, dokumentumhoz vagy megjegyzéshez kapcsolt páciens nem törölhető." });
        db.Patients.Remove(p); await db.SaveChangesAsync(); return NoContent();
    }
    [HttpPut("{id}/user"), Authorize(Roles="Admin")]
    public async Task<IActionResult> Link(string id, LinkInput input) {
        var p=await db.Patients.FindAsync(id); var u=await users.FindByIdAsync(input.UserId);
        if(p is null || u is null) return NotFound();
        if(!await users.IsInRoleAsync(u,"Patient")) return BadRequest("Először rendelj Patient role-t a felhasználóhoz.");
        if (await db.Patients.AnyAsync(p => p.Id != id && p.UserId == u.Id)) return Conflict(new { message = "Ehhez a fiókhoz már másik páciensprofil tartozik." });
        p.UserId=u.Id; await db.SaveChangesAsync(); return NoContent();
    }
    [HttpGet("{id}/access"),Authorize(Roles="Admin,AdmissionsOffice")]
    public async Task<IActionResult> AccessList(string id)=>Ok(await db.PatientPractitionerAccesses.Where(a=>a.PatientId==id).Select(a=>new{Id=a.PractitionerId,a.Practitioner.Name}).ToListAsync());
    [HttpPut("{id}/access/{practitionerId}"), Authorize(Roles="Admin,AdmissionsOffice,Practitioner")]
    public async Task<IActionResult> Grant(string id,string practitionerId) {
        if(!await db.Patients.AnyAsync(p=>p.Id==id) || !await db.Practitioners.AnyAsync(p=>p.Id==practitionerId)) return NotFound();
        if(User.IsInRole("Practitioner") && !await access.OwnPractitioner(practitionerId)) return Forbid();
        if(!await db.PatientPractitionerAccesses.AnyAsync(a=>a.PatientId==id && a.PractitionerId==practitionerId)) {
            db.PatientPractitionerAccesses.Add(new(){PatientId=id,PractitionerId=practitionerId}); await db.SaveChangesAsync();
        } return NoContent();
    }
    [HttpDelete("{id}/access/{practitionerId}"), Authorize(Roles="Admin,AdmissionsOffice")]
    public async Task<IActionResult> Revoke(string id,string practitionerId) {
        var a=await db.PatientPractitionerAccesses.FindAsync(id,practitionerId);
        if(a is not null) {db.Remove(a); await db.SaveChangesAsync();} return NoContent();
    }
    [HttpPut("{id}/contact")]
    public async Task<IActionResult> Contact(string id, ContactInput input) {
        if(!access.Staff && !await access.OwnPatient(id)) return Forbid();
        var p=await db.Patients.FindAsync(id); if(p is null) return NotFound();
        p.Email=input.Email;p.Phone=input.Phone;p.Address=input.Address;
        await db.SaveChangesAsync();return NoContent();
    }
    [HttpGet("{id}/activities")]
    public async Task<IActionResult> Activities(string id) {
        if(!await access.Patients().AnyAsync(p=>p.Id==id)) return NotFound();
        return Ok(await access.Activities().Where(a=>a.PatientActivities.Any(p=>p.PatientId==id))
            .OrderByDescending(a=>a.Date).ThenByDescending(a=>a.Id)
            .Select(a=>new {a.Id,a.Title,a.Date,a.Status,a.IsCancelled,a.City,a.Venue}).ToListAsync());
    }
    static void Apply(Patient p,PatientInput i) { p.Name=i.Name.Trim(); p.TajNumber=i.TajNumber; p.BirthDate=i.BirthDate; p.BirthPlace=i.BirthPlace?.Trim(); p.Email=i.Email; p.Phone=i.Phone; p.Address=i.Address; p.Notes=i.Notes; }
}
