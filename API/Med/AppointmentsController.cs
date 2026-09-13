using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;
namespace API.Med;
[ApiController,Route("api/appointments"),Authorize]
public class AppointmentsController(AppDbContext db,AccessService access,BookingService booking):ControllerBase
{
    [HttpGet("slots")]
    public async Task<IActionResult> Slots(string practitionerId,DateOnly date)=>Ok(await booking.Slots(practitionerId,date));
    [HttpGet] public async Task<IActionResult> List()=>Ok(await access.Appointments().OrderBy(a=>a.StartTime).Select(a=>new{a.Id,a.PatientId,a.PractitionerId,a.ActivityId,a.BookingDate,a.StartTime,a.EndTime,a.Status,a.Note}).ToListAsync());
    [HttpPost] public async Task<IActionResult> Create(BookingInput input) {
        if(!access.Staff && !await access.OwnPatient(input.PatientId))return Forbid();
        var a=await booking.Book(input,access.UserId);return StatusCode(201,new{a.Id,a.ActivityId,a.StartTime,a.EndTime});
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> Details(string id) {
        var result=await access.Appointments().AsNoTracking().Where(a=>a.Id==id)
            .Select(a=>new{a.Id,a.PatientId,a.PractitionerId,a.ActivityId,a.BookingDate,a.StartTime,a.EndTime,a.Status,a.Note}).SingleOrDefaultAsync();
        return result is null?NotFound():Ok(result);
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> Reschedule(string id,RescheduleInput input) {
        await using var tx=await db.Database.BeginTransactionAsync();
        var a=await access.Appointments().Include(a=>a.Activity).SingleOrDefaultAsync(a=>a.Id==id);
        if(a is null)return NotFound();
        if(!access.Staff && !await access.OwnPatient(a.PatientId))return Forbid();
        if(a.Status!=AppointmentStatus.Booked || a.StartTime<=BookingService.LocalNow())
            return Conflict(new{message="Csak jövőbeli, aktív foglalás helyezhető át."});
        if(input.Date==default)return BadRequest(new{message="A dátum kötelező."});
        await booking.Sync(a.Activity,a,input.Date.ToDateTime(new TimeOnly(input.Hour,0)),"Scheduled");
        a.Note=input.Note ?? a.Note;a.Activity.UpdatedAt=DateTime.UtcNow;a.Activity.UpdatedByUserId=access.UserId;
        await db.SaveChangesAsync();await tx.CommitAsync();return NoContent();
    }
    [HttpPut("{id}/status"),Authorize(Roles="Admin,AdmissionsOffice,Practitioner")]
    public async Task<IActionResult> Status(string id,AppointmentStatusInput input) {
        await using var tx=await db.Database.BeginTransactionAsync();
        var a=await access.Appointments().Include(a=>a.Activity).SingleOrDefaultAsync(a=>a.Id==id);
        if(a is null)return NotFound();
        await booking.Sync(a.Activity,a,a.StartTime,input.Status);
        a.Activity.UpdatedAt=DateTime.UtcNow;a.Activity.UpdatedByUserId=access.UserId;
        await db.SaveChangesAsync();await tx.CommitAsync();return NoContent();
    }
    [HttpDelete("{id}"),Authorize(Roles="Admin,AdmissionsOffice")]
    public async Task<IActionResult> Delete(string id) {
        await using var tx=await db.Database.BeginTransactionAsync();
        var a=await db.Appointments.Include(a=>a.Activity).SingleOrDefaultAsync(a=>a.Id==id);
        if(a is null)return NotFound();
        db.Appointments.Remove(a);await db.SaveChangesAsync();
        db.Activities.Remove(a.Activity);await db.SaveChangesAsync();
        await tx.CommitAsync();return NoContent();
    }
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(string id) {
        var a=await access.Appointments().Include(a=>a.Activity).SingleOrDefaultAsync(a=>a.Id==id);
        if(a is null)return NotFound();
        if(a.Status==AppointmentStatus.Cancelled)return NoContent();
        if(a.Status!=AppointmentStatus.Booked || a.StartTime<=BookingService.LocalNow())return Conflict("Csak jövőbeli foglalás mondható le.");
        a.Status=AppointmentStatus.Cancelled;a.Activity.IsCancelled=true;a.Activity.Status="Cancelled";a.Activity.UpdatedAt=DateTime.UtcNow;a.Activity.UpdatedByUserId=access.UserId;
        await db.SaveChangesAsync();return NoContent();
    }
}
