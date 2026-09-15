using System.ComponentModel.DataAnnotations;
namespace API.Med;
public class ActivityWriteInput
{
    [RegularExpression("^(Scheduled|Cancelled|Completed|NoShow)$")] public string? Status { get; set; }
    public string? Id { get; set; }
    [Required,MaxLength(200)] public string Title { get; set; } = "";
    public DateTime Date { get; set; }
    [Required,MaxLength(5000)] public string Description { get; set; } = "";
    [Required,MaxLength(100)] public string Category { get; set; } = "";
    [MaxLength(200)] public string City { get; set; } = "";
    [Required,MaxLength(300)] public string Venue { get; set; } = "";
    [Range(-90,90)] public double Latitude { get; set; }
    [Range(-180,180)] public double Longitude { get; set; }
    public string? PatientId { get; set; }
    public List<string>? PractitionerIds { get; set; }
}