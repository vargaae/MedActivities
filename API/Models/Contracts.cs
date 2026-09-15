using System.ComponentModel.DataAnnotations;
namespace API.Med;
public record PatientInput([Required, MaxLength(100)] string Name,
    [Required, RegularExpression(@"^[0-9]{9}$")] string TajNumber, DateOnly BirthDate, [MaxLength(100)] string? BirthPlace,
    [EmailAddress] string? Email, [MaxLength(40)] string? Phone, [MaxLength(300)] string? Address, [MaxLength(2000)] string? Notes);
public record PractitionerInput([Required, MaxLength(100)] string Name,
    [Required, RegularExpression(@"^[0-9]{9}$")] string TajNumber,
    [Required] string UserId, [MaxLength(100)] string Specialty, [Required] string City, [Required] string Venue);
public record WorkingHoursInput(DayOfWeek DayOfWeek, bool IsWorkingDay, TimeOnly StartTime, TimeOnly EndTime);
public record BookingInput([Required] string PatientId, [Required] string PractitionerId, DateOnly Date, [Range(8,19)] int Hour, [MaxLength(1000)] string? Note);
public record EnabledInput(bool BookingEnabled);
public record LinkInput([Required] string UserId);
public record RoleInput([Required] string Role, [Required] string Name);
public record ContactInput([EmailAddress] string? Email, [MaxLength(40)] string? Phone, [MaxLength(300)] string? Address);
public record RescheduleInput(DateOnly Date, [Range(8,19)] int Hour, [MaxLength(1000)] string? Note);
public record AppointmentStatusInput([RegularExpression("^(Scheduled|Cancelled|Completed|NoShow)$")] string Status);
