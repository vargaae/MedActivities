// Ezt a fájlt mintának tedd át az ASP.NET Core / Domain projektedbe,
// ne közvetlenül a WinForms projektbe.

namespace Domain;

public class Patient
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string Name { get; set; }
    public DateTime BirthDate { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public ICollection<Activity> Activities { get; set; } = [];
}

// A meglévő Activity osztályba ezt a két property-t add:
// public string? PatientId { get; set; }
// public Patient? Patient { get; set; }
