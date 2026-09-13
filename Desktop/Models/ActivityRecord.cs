using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MedActivities.Patient.Sqlite.WinForms.Models;

public class ActivityRecord
{
    [Browsable(false)]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [Browsable(false)]
    public string? PatientId { get; set; }
    [DisplayName("Esemény"), Required(ErrorMessage = "A cím kötelező."), MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    [DisplayName("Dátum")]
    public DateTime Date { get; set; } = DateTime.Now;
    [DisplayName("Leírás"), Required(ErrorMessage = "A leírás kötelező."), MaxLength(5000)]
    public string Description { get; set; } = string.Empty;
    [DisplayName("Kategória"), Required(ErrorMessage = "A kategória kötelező."), MaxLength(100)]
    public string Category { get; set; } = string.Empty;
    [Browsable(false)]
    public bool IsCancelled => Status == "Cancelled";
    [Browsable(false), RegularExpression("^(Scheduled|Cancelled|Completed|NoShow)$")]
    public string Status { get; set; } = "Scheduled";
    [DisplayName("Állapot")]
    public string StatusLabel => Status switch { "Cancelled" => "Lemondva", "Completed" => "Befejezve", "NoShow" => "Nem jelent meg", _ => "Tervezett" };
    [DisplayName("Város"), MaxLength(200)]
    public string City { get; set; } = string.Empty;
    [DisplayName("Helyszín"), Required(ErrorMessage = "A helyszín kötelező."), MaxLength(300)]
    public string Venue { get; set; } = string.Empty;
    [Browsable(false), Range(-90, 90)]
    public double Latitude { get; set; }
    [Browsable(false), Range(-180, 180)]
    public double Longitude { get; set; }
    [Browsable(false)]
    public List<string> PractitionerIds { get; set; } = [];
    [DisplayName("Kezelők")]
    public string PractitionerNames { get; set; } = string.Empty;
    [DisplayName("Foglalási esemény")]
    public bool IsAppointment { get; set; }
}
