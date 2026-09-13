namespace MedActivities.Patient.Sqlite.WinForms.Models;

public sealed record PractitionerOption(string Id, string Name)
{
    public override string ToString() => Name;
}
