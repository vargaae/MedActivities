namespace Domain;

public class ActivityDocument
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string ActivityId { get; set; }
    public Activity Activity { get; set; } = null!;
    public required string Title { get; set; }
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public byte[] Content { get; set; } = [];
    public required string UploadedByUserId { get; set; }
    public required string UploadedByName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
