using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Identity;
namespace API.Med;
[ApiController, Route("api/practitioners"), Authorize]
public class PractitionersController(AppDbContext db,AccessService access,UserManager<AppUser> users):ControllerBase
{
    [HttpGet] public async Task<IActionResult> List()=>Ok(await db.Practitioners.OrderBy(p=>p.Name).ThenBy(p=>p.Id).Select(p=>new {p.Id,p.Name,p.Specialty,p.City,p.Venue,BookingEnabled=p.BookingSettings!=null && p.BookingSettings.BookingEnabled}).ToListAsync());
    [HttpGet("{id}")] public async Task<IActionResult> Details(string id) {
        if(access.Staff || await access.OwnPractitioner(id)) { var full=await db.Practitioners.Where(p=>p.Id==id).Select(p=>new{p.Id,p.Name,p.TajNumber,p.UserId,p.Specialty,p.City,p.Venue,BookingEnabled=p.BookingSettings!=null && p.BookingSettings.BookingEnabled}).FirstOrDefaultAsync(); return full is null?NotFound():Ok(full); }
        var p=await db.Practitioners.Where(p=>p.Id==id).Select(p=>new {p.Id,p.Name,p.Specialty,p.City,p.Venue,BookingEnabled=p.BookingSettings!=null && p.BookingSettings.BookingEnabled}).FirstOrDefaultAsync();
        return p is null?NotFound():Ok(p);
    }
    [HttpPost,Authorize(Roles="Admin")]
    public async Task<IActionResult> Create(PractitionerInput i) {
        var u=await users.FindByIdAsync(i.UserId); if(u is null || !await users.IsInRoleAsync(u,"Practitioner")) return BadRequest("Practitioner role-lal rendelkező felhasználó szükséges.");
        if (await db.Practitioners.AnyAsync(p => p.UserId == i.UserId)) return Conflict(new { message = "Ehhez a fiókhoz már tartozik kezelőorvos profil. A meglévő kezelőorvosot szerkeszd, vagy válassz szabad Practitioner-fiókot." });
        if (await db.Practitioners.AnyAsync(p => p.TajNumber == i.TajNumber)) return Conflict(new { message = "Ez a TAJ-szám már szerepel a kezelőorvosok között." });
        var p=new PractitionerProfile{Name=i.Name.Trim(),TajNumber=i.TajNumber,UserId=i.UserId,Specialty=i.Specialty,City=i.City,Venue=i.Venue};
        p.BookingSettings=new(){PractitionerId=p.Id}; db.Practitioners.Add(p); await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Details),new{id=p.Id},new{p.Id});
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> Edit(string id,PractitionerInput i) {
        var p=await db.Practitioners.FindAsync(id); if(p is null) return NotFound();
        if(!access.Staff && !await access.OwnPractitioner(id)) return Forbid();
        if(i.UserId!=p.UserId) return BadRequest("A kezelőorvos felhasználói kapcsolata nem módosítható.");
        if (await db.Practitioners.AnyAsync(p => p.Id != id && p.TajNumber == i.TajNumber)) return Conflict(new { message = "Ez a TAJ-szám már másik kezelőorvoshoz tartozik." });
        p.Name=i.Name.Trim();p.TajNumber=i.TajNumber;p.Specialty=i.Specialty;p.City=i.City;p.Venue=i.Venue;
        await db.SaveChangesAsync();return NoContent();
    }
    [HttpDelete("{id}"),Authorize(Roles="Admin")]
    public async Task<IActionResult> Delete(string id) {
        var p=await db.Practitioners.FindAsync(id);if(p is null)return NotFound();
        await using var tx=await db.Database.BeginTransactionAsync();
        // A hozzáférések, foglalhatósági beállítások és munkaidők explicit
        // törlése kiszámíthatóvá teszi a törlést az SQL Server triggerei mellett.
        db.PatientPractitionerAccesses.RemoveRange(await db.PatientPractitionerAccesses.Where(a=>a.PractitionerId==id).ToListAsync());
        db.PractitionerWorkingHours.RemoveRange(await db.PractitionerWorkingHours.Where(a=>a.PractitionerId==id).ToListAsync());
        db.PractitionerBookingSettings.RemoveRange(await db.PractitionerBookingSettings.Where(s=>s.PractitionerId==id).ToListAsync());
        db.Remove(p);
        try { await db.SaveChangesAsync(); await tx.CommitAsync(); return NoContent(); }
        catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.SqlClient.SqlException)
        { return Conflict("A kezelőorvos törlése nem hajtható végre. Frissítsd a listát és próbáld újra."); }
        catch (Microsoft.Data.SqlClient.SqlException)
        { return Conflict("A kezelőorvos törlése nem hajtható végre. Frissítsd a listát és próbáld újra."); }
    }
    [HttpGet("available-accounts"), Authorize(Roles="Admin")]
    public async Task<IActionResult> AvailableAccounts() {
        var assigned = await db.Practitioners.Select(p => p.UserId).ToListAsync();
        return Ok((await users.GetUsersInRoleAsync("Practitioner"))
            .Where(u => !assigned.Contains(u.Id) && (!u.LockoutEnd.HasValue || u.LockoutEnd <= DateTimeOffset.UtcNow))
            .Select(u => new { u.Id, u.UserName }));
    }
    [HttpPut("{id}/booking-enabled"),Authorize(Roles="Admin")]
    public async Task<IActionResult> Enable(string id,EnabledInput i) {
        if(!await db.Practitioners.AnyAsync(p=>p.Id==id))return NotFound();
        var s=await db.PractitionerBookingSettings.SingleOrDefaultAsync(s=>s.PractitionerId==id);
        if(s is null){s=new(){PractitionerId=id};db.Add(s);}s.BookingEnabled=i.BookingEnabled;await db.SaveChangesAsync();return NoContent();
    }
    [HttpGet("{id}/working-hours")]
    public async Task<IActionResult> Hours(string id)=>Ok(await db.PractitionerWorkingHours.Where(w=>w.PractitionerId==id).Select(w=>new{w.DayOfWeek,w.IsWorkingDay,w.StartTime,w.EndTime}).ToListAsync());
    [HttpPut("{id}/working-hours")]
    public async Task<IActionResult> Hours(string id,WorkingHoursInput i) {
        if(!access.Admin)return Forbid();
        if(!await db.Practitioners.AnyAsync(p=>p.Id==id))return NotFound();
        if(!Enum.IsDefined(i.DayOfWeek) || i.StartTime.Minute!=0 || i.EndTime.Minute!=0 || i.StartTime.Ticks%TimeSpan.TicksPerHour!=0 || i.EndTime.Ticks%TimeSpan.TicksPerHour!=0 || i.StartTime<new TimeOnly(8,0)||i.EndTime>new TimeOnly(20,0)||i.StartTime>=i.EndTime)return BadRequest("Egész órás munkaidő kell 08–20 között.");
        var now=BookingService.LocalNow();
        var future=await db.Appointments.Where(a=>a.PractitionerId==id && a.Status==AppointmentStatus.Booked && a.StartTime>now).ToListAsync();
        if(future.Any(a=>a.StartTime.DayOfWeek==i.DayOfWeek && (!i.IsWorkingDay || TimeOnly.FromDateTime(a.StartTime)<i.StartTime || TimeOnly.FromDateTime(a.EndTime)>i.EndTime)))return Conflict("A módosítás meglévő foglalást érint; előbb mondd le azt.");
        var w=await db.PractitionerWorkingHours.SingleOrDefaultAsync(w=>w.PractitionerId==id&&w.DayOfWeek==i.DayOfWeek);
        if(w is null){w=new(){PractitionerId=id,DayOfWeek=i.DayOfWeek};db.Add(w);}w.IsWorkingDay=i.IsWorkingDay;w.StartTime=i.StartTime;w.EndTime=i.EndTime;await db.SaveChangesAsync();return NoContent();
    }
    [HttpGet("{id}/activities")]
    public async Task<IActionResult> Activities(string id) {
        if(!access.Staff && !await access.OwnPractitioner(id))return Forbid();
        return Ok(await access.Activities().Where(a=>a.ActivityPractitioners.Any(p=>p.PractitionerId==id)).OrderByDescending(a=>a.Date).ThenByDescending(a=>a.Id).Select(a=>new{a.Id,a.Title,a.Date,a.Status,a.IsCancelled}).ToListAsync());
    }
}
