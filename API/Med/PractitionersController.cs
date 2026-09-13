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
    [HttpGet] public async Task<IActionResult> List()=>Ok(await db.Practitioners.Select(p=>new {p.Id,p.Name,p.Specialty,p.City,p.Venue,BookingEnabled=p.BookingSettings!=null && p.BookingSettings.BookingEnabled}).ToListAsync());
    [HttpGet("{id}")] public async Task<IActionResult> Details(string id) {
        if(access.Staff) { var full=await db.Practitioners.Where(p=>p.Id==id).Select(p=>new{p.Id,p.Name,p.TajNumber,p.UserId,p.Specialty,p.City,p.Venue,BookingEnabled=p.BookingSettings!=null && p.BookingSettings.BookingEnabled}).FirstOrDefaultAsync(); return full is null?NotFound():Ok(full); }
        var p=await db.Practitioners.Where(p=>p.Id==id).Select(p=>new {p.Id,p.Name,p.Specialty,p.City,p.Venue,BookingEnabled=p.BookingSettings!=null && p.BookingSettings.BookingEnabled}).FirstOrDefaultAsync();
        return p is null?NotFound():Ok(p);
    }
    [HttpPost,Authorize(Roles="Admin")]
    public async Task<IActionResult> Create(PractitionerInput i) {
        var u=await users.FindByIdAsync(i.UserId); if(u is null || !await users.IsInRoleAsync(u,"Practitioner")) return BadRequest("Practitioner role-lal rendelkező felhasználó szükséges.");
        var p=new PractitionerProfile{Name=i.Name.Trim(),TajNumber=i.TajNumber,UserId=i.UserId,Specialty=i.Specialty,City=i.City,Venue=i.Venue};
        p.BookingSettings=new(){PractitionerId=p.Id}; db.Practitioners.Add(p); await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Details),new{id=p.Id},new{p.Id});
    }
    [HttpPut("{id}"),Authorize(Roles="Admin")]
    public async Task<IActionResult> Edit(string id,PractitionerInput i) {
        var p=await db.Practitioners.FindAsync(id); if(p is null) return NotFound();
        if(i.UserId!=p.UserId) return BadRequest("A kezelő felhasználói kapcsolata nem módosítható.");
        p.Name=i.Name.Trim();p.TajNumber=i.TajNumber;p.Specialty=i.Specialty;p.City=i.City;p.Venue=i.Venue;
        await db.SaveChangesAsync();return NoContent();
    }
    [HttpDelete("{id}"),Authorize(Roles="Admin")]
    public async Task<IActionResult> Delete(string id) {
        var p=await db.Practitioners.FindAsync(id);if(p is null)return NotFound();
        if(await db.Appointments.AnyAsync(a=>a.PractitionerId==id)||await db.ActivityPractitioners.AnyAsync(a=>a.PractitionerId==id))return Conflict("Kapcsolt kezelő nem törölhető.");
        db.Remove(p);await db.SaveChangesAsync();return NoContent();
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
        if(!access.Staff && !await access.OwnPractitioner(id))return Forbid();
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
        return Ok(await access.Activities().Where(a=>a.ActivityPractitioners.Any(p=>p.PractitionerId==id)).Select(a=>new{a.Id,a.Title,a.Date,a.Status,a.IsCancelled}).ToListAsync());
    }
}
