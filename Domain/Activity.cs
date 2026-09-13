namespace Domain;

public class Activity
{
	// Make Id public so it's usable by consumers (e.g. EF, serialization)
	public string Id { get; set; } = Guid.NewGuid().ToString();

public required string Title { get; set; }

public DateTime Date { get; set; }

public required string Description { get; set; }

public required string Category { get; set; }

public bool IsCancelled { get; set; }

// location props

public required string City { get; set; }

public required string Venue { get; set; }

public double Latitude { get; set; }

public double Longitude { get; set; }

// medical props
public string Status { get; set; } = "Scheduled";
public string? MedicalNotes { get; set; }
public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
public string? CreatedByUserId { get; set; }
public DateTime? UpdatedAt { get; set; }
public string? UpdatedByUserId { get; set; }
public ICollection<PatientActivity> PatientActivities { get; set; } = [];
public ICollection<ActivityPractitioner> ActivityPractitioners { get; set; } = [];
}
