namespace Domain;

public class DeletedRecord
{
    public long Id { get; set; }
    public required string TableName { get; set; }
    public required string RecordKey { get; set; }
    public required string SnapshotJson { get; set; }
    public DateTime DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
}
