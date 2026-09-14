namespace Domain;

public class ActivityComment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string ActivityId { get; set; }
    public Activity Activity { get; set; } = null!;
    public required string UserId { get; set; }
    public required string DisplayName { get; set; }
    public required string Body { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
