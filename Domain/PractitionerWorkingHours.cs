namespace Domain;
public class PractitionerWorkingHours
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string PractitionerId { get; set; }
    public PractitionerProfile Practitioner { get; set; } = null!;
    public DayOfWeek DayOfWeek { get; set; }
    public bool IsWorkingDay { get; set; }
    public TimeOnly StartTime { get; set; } = new(8, 0);
    public TimeOnly EndTime { get; set; } = new(20, 0);
}