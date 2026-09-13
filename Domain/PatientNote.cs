namespace Domain;

public class PatientNote
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public required string Text { get; set; }
    public required string AuthorUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
