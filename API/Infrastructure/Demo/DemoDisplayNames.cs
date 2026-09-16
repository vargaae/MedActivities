namespace API.Med;

/// <summary>Stable fictional names for generated accounts, including accounts whose profile was removed.</summary>
public static class DemoDisplayNames
{
    private static readonly string[] FamilyNames =
        ["Kovács", "Nagy", "Tóth", "Szabó", "Horváth", "Varga", "Kiss", "Molnár", "Németh", "Farkas"];
    private static readonly string[] GivenNames =
        ["Anna", "Péter", "Júlia", "Gábor", "Eszter", "Tamás", "Dóra", "Márton", "Katalin", "Ádám"];

    public static string GeneratedName(int number, bool practitioner)
    {
        var index = number - 1;
        return (practitioner ? "Dr. " : "") +
            $"{FamilyNames[index / GivenNames.Length % FamilyNames.Length]} {GivenNames[index % GivenNames.Length]}";
    }

    public static string? ForAccount(string id)
    {
        if (!id.StartsWith(DemoSessionSetup.Prefix, StringComparison.Ordinal)) return null;
        var suffix = id[DemoSessionSetup.Prefix.Length..];
        for (var i = 0; i < DemoSessionSetup.Roles.Length; i++)
            if (suffix == DemoSessionSetup.Roles[i]) return DemoSessionSetup.Names[i];

        var practitioner = suffix.StartsWith("doctor-user-", StringComparison.Ordinal);
        var prefix = practitioner ? "doctor-user-" : "patient-user-";
        if (!suffix.StartsWith(prefix, StringComparison.Ordinal) ||
            !int.TryParse(suffix[prefix.Length..], out var number) ||
            number < 1 || number > (practitioner ? DemoDataSeeder.PractitionerCount : DemoDataSeeder.PatientCount))
            return null;

        return GeneratedName(number, practitioner);
    }
}
