using System.ComponentModel.DataAnnotations;
using MedActivities.Patient.Sqlite.WinForms.Models;

namespace MedActivities.Patient.Sqlite.WinForms.Services;

public static class RecordValidation
{
    // A form és az adatkezelő is ellenőrzi; a TAJ szabálya az API-val azonos (9 ASCII számjegy).
    public static void Patient(PatientRecord patient)
    {
        patient.Name = patient.Name.Trim();
        patient.TajNumber = patient.TajNumber.Trim();
        // Az opcionális, üres e-mailt az API is nullként kezeli.
        var errors = new List<ValidationResult>();
        foreach (var property in typeof(PatientRecord).GetProperties())
        {
            var value = property.GetValue(patient);
            if (property.Name == nameof(PatientRecord.Email) && string.IsNullOrWhiteSpace(patient.Email)) continue;
            Validator.TryValidateProperty(value, new ValidationContext(patient) { MemberName = property.Name }, errors);
        }
        if (errors.Count > 0) throw new ValidationException(string.Join(Environment.NewLine, errors.Select(x => x.ErrorMessage)));
        if (patient.BirthDate.Date == default || patient.BirthDate.Date > DateTime.Today)
            throw new ValidationException("A születési dátum kötelező, és nem lehet jövőbeli.");
    }

    public static void Activity(ActivityRecord activity)
    {
        activity.Title = activity.Title.Trim();
        activity.Description = activity.Description.Trim();
        activity.Category = activity.Category.Trim();
        activity.Venue = activity.Venue.Trim();
        Validator.ValidateObject(activity, new ValidationContext(activity), validateAllProperties: true);
        if (activity.Date == default) throw new ValidationException("Az esemény dátuma kötelező.");
        if (string.IsNullOrWhiteSpace(activity.PatientId)) throw new ValidationException("Válassz egy pácienst.");
        if (activity.PractitionerIds.Count == 0) throw new ValidationException("Legalább egy kezelőorvost válassz.");
    }
}
