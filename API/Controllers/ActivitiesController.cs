using Application.Activities.DTOs;
using API.Med;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;
using MediatR;
using Application.Activities.Queries;

namespace API.Controllers;

[ApiController]
[Route("api/activities")]
public class ActivitiesController(AppDbContext db, AccessService access, IWebHostEnvironment environment, BookingService booking, IMediator mediator) : ControllerBase
{
    bool Authenticated => User.Identity?.IsAuthenticated == true;
    IQueryable<Activity> Visible() => !Authenticated && environment.IsDevelopment()
        ? db.Activities : access.Activities();

    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> GetActivities(CancellationToken ct, string? patientId = null, string? practitionerId = null)
    {
        if (!Authenticated) return Unauthorized();
        return Ok(await mediator.Send(new GetActivityList.Query(access.UserId,
            User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToArray(), patientId, practitionerId), ct));
    }

    [HttpGet("page"), Authorize]
    public async Task<IActionResult> GetPage(CancellationToken ct, int page = 1, string? patientId = null,
        string? practitionerId = null, string? category = null, DateTime? from = null, DateTime? to = null)
    {
        if (page < 1 || page > 100000 || (from.HasValue && to.HasValue && from >= to))
            return BadRequest(new { message = "Érvénytelen lap vagy dátumtartomány." });
        return Ok(await mediator.Send(new GetActivityPage.Query(access.UserId,
            User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToArray(),
            page, patientId, practitionerId, category, from, to), ct));
    }

    [HttpGet("{id}"), AllowAnonymous]
    public async Task<IActionResult> GetActivity(string id,CancellationToken ct)
    {
        if (!Authenticated) return Unauthorized();
        var dto=await Visible().AsNoTracking().Where(a=>a.Id==id).Select(ActivityDto.Projection).SingleOrDefaultAsync(ct);
        if(dto is null)return NotFound();
        dto.IsAppointment=await db.Appointments.AnyAsync(a=>a.ActivityId==id,ct);
        dto.CanEditFields=Authenticated && (access.Staff || (!dto.IsAppointment &&
            await access.EditableActivities().AnyAsync(a => a.Id == id, ct)));
        dto.CanDelete=Authenticated && access.Staff;
        dto.CanEditAssignments=dto.CanEditFields && access.Staff;
        return Ok(dto);
    }

    [HttpGet("assignment-options"),Authorize]
    public async Task<IActionResult> Options(CancellationToken ct)
    {
        var own=User.IsInRole("Practitioner")
            ? await db.Practitioners.Where(p=>p.UserId==access.UserId)
                .Select(p=>new {p.Id,p.Name}).SingleOrDefaultAsync(ct) : null;
        var canCreate=access.Staff || own is not null;
        var patients=access.Staff ? db.Patients : User.IsInRole("Practitioner")
            ? access.Patients() : db.Patients.Where(p=>false);
        return Ok(new {
            userName=User.Identity?.Name ?? "", roles=User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c=>c.Value).ToArray(), userId=access.UserId, canCreate, canAssign=access.Staff,
            isPractitioner=User.IsInRole("Practitioner"), ownPractitioner=own,
            patients=await patients.OrderBy(p=>p.Name).Select(p=>new {p.Id,p.Name}).ToListAsync(ct),
            practitioners=await (access.Staff ? db.Practitioners : db.Practitioners.Where(p=>false))
                .OrderBy(p=>p.Name).Select(p=>new {p.Id,p.Name}).ToListAsync(ct)
        });
    }

    [HttpPost,Authorize(Roles="Admin,AdmissionsOffice,Practitioner")]
    public async Task<IActionResult> CreateActivity(ActivityWriteInput input,CancellationToken ct)
    {
        if(input.Date==default)return BadRequest(new {message="A dátum kötelező."});
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        if(string.IsNullOrWhiteSpace(input.PatientId))return BadRequest(new {message="Pontosan egy páciens kiválasztása kötelező."});
        if(!await db.Patients.AnyAsync(p=>p.Id==input.PatientId,ct))return BadRequest(new {message="A páciens nem létezik."});
        List<string> ids;
        if(access.Staff)
        {
            ids=input.PractitionerIds?.Distinct().ToList() ?? [];
            if(ids.Count==0 || await db.Practitioners.CountAsync(p=>ids.Contains(p.Id),ct)!=ids.Count)
                return BadRequest(new {message="Legalább egy létező kezelőorvost válassz."});
        }
        else
        {
            var own=await db.Practitioners.SingleOrDefaultAsync(p=>p.UserId==access.UserId,ct);
            if(own is null)return Forbid();
            if(!await access.Patients().AnyAsync(p=>p.Id==input.PatientId,ct))return Forbid();
            if(input.PractitionerIds is not null && !input.PractitionerIds.ToHashSet().SetEquals([own.Id]))return Forbid();
            ids=[own.Id];
        }
        var activity=new Activity {Title=input.Title,Description=input.Description,Category=input.Category,City=input.City,Venue=input.Venue};
        Apply(activity,input);activity.CreatedByUserId=access.UserId;
        activity.PatientActivities.Add(new(){ActivityId=activity.Id,PatientId=input.PatientId});
        foreach(var id in ids)activity.ActivityPractitioners.Add(new(){ActivityId=activity.Id,PractitionerId=id});
        db.Activities.Add(activity);await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
        return CreatedAtAction(nameof(GetActivity),new{id=activity.Id},activity.Id);
    }

    [HttpPut,Authorize(Roles="Admin,AdmissionsOffice,Practitioner,Patient")]
    public async Task<IActionResult> UpdateActivity(ActivityWriteInput input,CancellationToken ct)
    {
        if(input.Date==default)return BadRequest(new {message="A dátum kötelező."});
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        var activity=await access.Activities().Include(a=>a.PatientActivities).Include(a=>a.ActivityPractitioners)
            .SingleOrDefaultAsync(a=>a.Id==input.Id,ct);
        if(activity is null)return NotFound();
        if(!await access.EditableActivities().AnyAsync(a=>a.Id==activity.Id,ct))return Forbid();
        var appointment=await db.Appointments.SingleOrDefaultAsync(a=>a.ActivityId==activity.Id,ct);
        if(appointment is not null && !access.Staff)
            return Conflict(new {message="Foglalási eseménynél itt csak admin/felvételi iroda módosíthat; jelentkezz be megfelelő szerepkörrel."});
        if(access.Staff)
        {
            if(string.IsNullOrWhiteSpace(input.PatientId) || !await db.Patients.AnyAsync(p=>p.Id==input.PatientId,ct))
                return BadRequest(new {message="Pontosan egy létező páciens kiválasztása kötelező."});
            var ids=input.PractitionerIds?.ToHashSet() ?? [];
            if(ids.Count==0 || await db.Practitioners.CountAsync(p=>ids.Contains(p.Id),ct)!=ids.Count)
                return BadRequest(new {message="Legalább egy létező kezelőorvost válassz."});
            if(appointment is not null)
            {
                // Az eddigi főkezelő megmarad, ha továbbra is szerepel a kiválasztottak között.
                // Ha eltávolítják, a kérés első kezelője veszi át a foglalást.
                var primary=ids.Contains(appointment.PractitionerId)
                    ? appointment.PractitionerId : input.PractitionerIds![0];
                appointment.PatientId=input.PatientId;
                appointment.PractitionerId=primary;
            }
            foreach(var link in activity.PatientActivities.Where(p=>p.PatientId!=input.PatientId).ToList())db.PatientActivities.Remove(link);
            if(!activity.PatientActivities.Any(p=>p.PatientId==input.PatientId))
                activity.PatientActivities.Add(new(){ActivityId=activity.Id,PatientId=input.PatientId});
            foreach(var link in activity.ActivityPractitioners.Where(p=>!ids.Contains(p.PractitionerId)).ToList())db.ActivityPractitioners.Remove(link);
            foreach(var id in ids.Where(id=>!activity.ActivityPractitioners.Any(p=>p.PractitionerId==id)))
                activity.ActivityPractitioners.Add(new(){ActivityId=activity.Id,PractitionerId=id});
        }
        else
        {
            // A tiltást nem lehet módosított HTTP-kéréssel megkerülni.
            if(input.PatientId is not null && !activity.PatientActivities.Select(p=>p.PatientId).ToHashSet().SetEquals([input.PatientId]))return Forbid();
            if(input.PractitionerIds is not null && !activity.ActivityPractitioners.Select(p=>p.PractitionerId).ToHashSet().SetEquals(input.PractitionerIds))return Forbid();
        }
        var status=input.Status ?? activity.Status;
        Apply(activity,input);
        if(appointment is not null)await booking.Sync(activity,appointment,input.Date,status);
        activity.UpdatedAt=DateTime.UtcNow;activity.UpdatedByUserId=access.UserId;
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return NoContent();
    }

    [HttpDelete("{id}"),Authorize(Roles="Admin,AdmissionsOffice")]
    public async Task<IActionResult> DeleteActivity(string id,CancellationToken ct)
    {
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        var activity=await db.Activities.FindAsync([id],ct);if(activity is null)return NotFound();
        var appointment=await db.Appointments.SingleOrDefaultAsync(a=>a.ActivityId==id,ct);
        if(appointment is not null){db.Appointments.Remove(appointment);await db.SaveChangesAsync(ct);}
        db.Activities.Remove(activity);await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return NoContent();
    }
    static void Apply(Activity a,ActivityWriteInput i)
    {
        if(i.Status is not null){a.Status=i.Status;a.IsCancelled=i.Status=="Cancelled";}
        a.Title=i.Title.Trim();a.Date=i.Date;a.Description=i.Description;a.Category=i.Category;
        a.City=i.City;a.Venue=i.Venue;a.Latitude=i.Latitude;a.Longitude=i.Longitude;
    }
}
