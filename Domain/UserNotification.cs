namespace Domain;

public class UserNotification
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string UserId { get; set; }
    public required string Message { get; set; }
    public required string Kind { get; set; }
    public string? ActivityId { get; set; }
    public string? PatientId { get; set; }
    public string? TargetId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAtUtc { get; set; }
}
