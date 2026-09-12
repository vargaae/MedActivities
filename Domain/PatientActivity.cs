namespace Domain;
public class PatientActivity
{
    public required string PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public required string ActivityId { get; set; }
    public Activity Activity { get; set; } = null!;
}