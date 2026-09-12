namespace Domain;
public class ActivityPractitioner
{
    public required string PractitionerId { get; set; }
    public PractitionerProfile Practitioner { get; set; } = null!;
    public required string ActivityId { get; set; }
    public Activity Activity { get; set; } = null!;
}