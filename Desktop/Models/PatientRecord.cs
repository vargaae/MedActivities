using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MedActivities.Patient.Sqlite.WinForms.Models;

public class PatientRecord
{
    [Browsable(false)]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [DisplayName("Név"), Required(ErrorMessage = "A név kötelező."), MaxLength(100, ErrorMessage = "A név legfeljebb 100 karakter lehet.")]
    public string Name { get; set; } = string.Empty;
    [DisplayName("TAJ-szám"), Required(ErrorMessage = "A TAJ-szám kötelező."), RegularExpression("^[0-9]{9}$", ErrorMessage = "A TAJ-szám pontosan 9 számjegy legyen.")]
    public string TajNumber { get; set; } = string.Empty;
    [DisplayName("Születési dátum")]
    public DateTime BirthDate { get; set; } = DateTime.Today;
    [DisplayName("E-mail"), EmailAddress(ErrorMessage = "Érvényes e-mail címet adj meg.")]
    public string Email { get; set; } = string.Empty;
    [DisplayName("Telefon"), MaxLength(40, ErrorMessage = "A telefon legfeljebb 40 karakter lehet.")]
    public string Phone { get; set; } = string.Empty;
    [DisplayName("Lakcím"), MaxLength(300, ErrorMessage = "A lakcím legfeljebb 300 karakter lehet.")]
    public string Address { get; set; } = string.Empty;
    [DisplayName("Megjegyzés"), MaxLength(2000, ErrorMessage = "A megjegyzés legfeljebb 2000 karakter lehet.")]
    public string Notes { get; set; } = string.Empty;

    public override string ToString() => Name;
}
