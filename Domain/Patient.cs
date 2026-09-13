namespace Domain;
public class Patient
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string TajNumber { get; set; }
    public string? UserId { get; set; }
    public required string Name { get; set; }
    public DateOnly BirthDate { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<PatientActivity> PatientActivities { get; set; } = [];
    public ICollection<PatientPractitionerAccess> PractitionerAccesses { get; set; } = [];
}