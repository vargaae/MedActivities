namespace Domain;
public class PractitionerBookingSettings
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string PractitionerId { get; set; }
    public PractitionerProfile Practitioner { get; set; } = null!;
    public bool BookingEnabled { get; set; }
    // MVP: fix egyórás időpont, 08:00–20:00. Nem írható kliensadat.
    public int SlotDurationMinutes => 60;
    public TimeOnly EarliestBookingTime => new(8, 0);
    public TimeOnly LatestBookingTime => new(20, 0);
}