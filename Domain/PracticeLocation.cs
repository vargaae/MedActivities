namespace Domain;

public static class PracticeLocation
{
    public const string PhysioPraxisVenue = "PhysioPraxis – Szent István körút 1.";
    public const double Latitude = 47.51299;
    public const double Longitude = 19.04809;

    public static bool IsPhysioPraxis(string? venue) =>
        venue?.Replace(" ", "").StartsWith("PhysioPraxis", StringComparison.OrdinalIgnoreCase) == true;

    public static void Apply(Activity activity)
    {
        if (!IsPhysioPraxis(activity.Venue)) return;
        activity.City = "Budapest";
        activity.Venue = PhysioPraxisVenue;
        activity.Latitude = Latitude;
        activity.Longitude = Longitude;
    }
}
