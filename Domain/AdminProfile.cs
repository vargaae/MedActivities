namespace Domain;
public class AdminProfile { public string Id { get; set; } = Guid.NewGuid().ToString(); public required string UserId { get; set; } public required string Name { get; set; } }