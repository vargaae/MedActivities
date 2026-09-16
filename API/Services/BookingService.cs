using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;
namespace API.Med;
public class BookingService(AppDbContext db)
{
    public static DateTime LocalNow()=>TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,TimeZoneInfo.FindSystemTimeZoneById("Europe/Budapest"));
    public async Task<List<int>> Slots(string practitionerId,DateOnly date,string? excludeId=null) {
        var settings=await db.PractitionerBookingSettings.SingleOrDefaultAsync(s=>s.PractitionerId==practitionerId);
        var w=await db.PractitionerWorkingHours.SingleOrDefaultAsync(w=>w.PractitionerId==practitionerId&&w.DayOfWeek==date.DayOfWeek);
        if(settings?.BookingEnabled!=true || w?.IsWorkingDay!=true)return [];
        var used=await db.Appointments.Where(a=>a.Id!=excludeId&&a.PractitionerId==practitionerId&&a.BookingDate==date&&a.Status!=AppointmentStatus.Cancelled).Select(a=>a.StartTime).ToListAsync();
        var now=LocalNow();
        return Enumerable.Range(8,12).Where(h=>new TimeOnly(h,0)>=w.StartTime && new TimeOnly(h+1,0)<=w.EndTime && date.ToDateTime(new TimeOnly(h,0))>now && !used.Any(t=>t.Hour==h)).ToList();
    }
    public async Task Sync(Activity activity,Appointment appointment,DateTime date,string status) {
        var target=status switch {
            "Scheduled"=>AppointmentStatus.Booked,"Cancelled"=>AppointmentStatus.Cancelled,
            "Completed"=>AppointmentStatus.Completed,"NoShow"=>AppointmentStatus.NoShow,
            _=>throw new BookingException("Ismeretlen státusz.")};
        if(date.Kind==DateTimeKind.Utc)
            date=TimeZoneInfo.ConvertTimeFromUtc(date,TimeZoneInfo.FindSystemTimeZoneById("Europe/Budapest"));
        date=DateTime.SpecifyKind(date,DateTimeKind.Unspecified);
        if(date.TimeOfDay.Ticks%TimeSpan.TicksPerHour!=0 || date.Hour<8 || date.Hour>19)
            throw new BookingException("Foglalás csak egész órakor, 08:00–19:00 kezdettel lehetséges.");
        var day=DateOnly.FromDateTime(date);
        var entry=db.Entry(appointment);
        var changed=date!=appointment.StartTime ||
            entry.Property(a=>a.PatientId).OriginalValue!=appointment.PatientId ||
            entry.Property(a=>a.PractitionerId).OriginalValue!=appointment.PractitionerId;
        if(target!=AppointmentStatus.Cancelled) {
            if(changed || appointment.Status==AppointmentStatus.Cancelled) {
                if(!(await Slots(appointment.PractitionerId,day,appointment.Id)).Contains(date.Hour))
                    throw new BookingException("A kiválasztott időpont nem foglalható (munkaidő, engedélyezés vagy ütközés).");
            }
            if(await db.Appointments.AnyAsync(a=>a.Id!=appointment.Id && a.PatientId==appointment.PatientId && a.BookingDate==day && a.Status!=AppointmentStatus.Cancelled))
                throw new BookingException("A páciensnek erre a napra már van foglalása.");
            if(target is AppointmentStatus.Completed or AppointmentStatus.NoShow && date>LocalNow())
                throw new BookingException("Jövőbeli foglalás nem jelölhető befejezettnek vagy meg nem jelentnek.");
        }
        appointment.BookingDate=day;appointment.StartTime=date;appointment.EndTime=date.AddHours(1);appointment.Status=target;
        activity.Date=date;activity.Status=status;activity.IsCancelled=target==AppointmentStatus.Cancelled;
    }
    public async Task<Appointment> Book(BookingInput i,string userId) {
        if(i.Hour<8||i.Hour>19)throw new BookingException("Hibás óra.");
        await using var tx=await db.Database.BeginTransactionAsync();
        if(!await db.Patients.AnyAsync(p=>p.Id==i.PatientId))throw new BookingException("Nincs ilyen páciens.");
        var practitioner=await db.Practitioners.SingleOrDefaultAsync(p=>p.Id==i.PractitionerId) ?? throw new BookingException("Nincs ilyen kezelő.");
        if(!(await Slots(i.PractitionerId,i.Date)).Contains(i.Hour))throw new BookingException("Ez az időpont nem foglalható.");
        if(await db.Appointments.AnyAsync(a=>a.PatientId==i.PatientId&&a.BookingDate==i.Date&&a.Status!=AppointmentStatus.Cancelled))throw new BookingException("A páciensnek erre a napra már van időpontja.");
        var start=i.Date.ToDateTime(new TimeOnly(i.Hour,0));
        var activity=new Activity{Title=$"Időpont – {practitioner.Name}",Date=start,Description="Egyórás konzultáció",Category="Időpontfoglalás",City=practitioner.City,Venue=practitioner.Venue,CreatedByUserId=userId};
        activity.PatientActivities.Add(new(){PatientId=i.PatientId,ActivityId=activity.Id});
        activity.ActivityPractitioners.Add(new(){PractitionerId=i.PractitionerId,ActivityId=activity.Id});
        var appointment=new Appointment{PatientId=i.PatientId,PractitionerId=i.PractitionerId,ActivityId=activity.Id,Activity=activity,BookingDate=i.Date,StartTime=start,EndTime=start.AddHours(1),Note=i.Note};
        db.Appointments.Add(appointment);
        await db.SaveChangesAsync();await tx.CommitAsync();return appointment;
    }
}
public class BookingException(string message):Exception(message);
