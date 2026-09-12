namespace Domain;
public class PatientPractitionerAccess
{
    public required string PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public required string PractitionerId { get; set; }
    public PractitionerProfile Practitioner { get; set; } = null!;
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
}