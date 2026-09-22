namespace Domain;
public class Appointment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public required string PractitionerId { get; set; }
    public PractitionerProfile Practitioner { get; set; } = null!;
    public required string ActivityId { get; set; }
    public Activity Activity { get; set; } = null!;
    // Europe/Budapest helyi falióra; nincs Z/UTC konverzió.
    public DateOnly BookingDate { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;
    public bool PractitionerConfirmed { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Note { get; set; }
}
