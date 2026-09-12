namespace Domain;
public class PractitionerProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string UserId { get; set; }
    public required string Name { get; set; }
    public required string TajNumber { get; set; }
    public string Specialty { get; set; } = "";
    public string City { get; set; } = "";
    public string Venue { get; set; } = "";
    public PractitionerBookingSettings? BookingSettings { get; set; }
    public ICollection<PractitionerWorkingHours> WorkingHours { get; set; } = [];
    public ICollection<ActivityPractitioner> ActivityPractitioners { get; set; } = [];
}