using System.ComponentModel.DataAnnotations;
namespace Desktop_Pacienskezelo;
public partial class PatientEditForm : Form
{
    public PatientEditForm() { InitializeComponent(); }
    public void LoadPatient(Patient? patient)
    {
        Text = patient is null ? "Új páciens" : "Páciens szerkesztése";
        if (patient is null) { birthPicker.Value = DateTime.Today.AddYears(-30); return; }
        nameBox.Text = patient.Name; tajBox.Text = patient.TajNumber;
        birthPicker.Value = patient.BirthDate.ToDateTime(TimeOnly.MinValue);
        emailBox.Text = patient.Email; phoneBox.Text = patient.Phone;
        addressBox.Text = patient.Address; notesBox.Text = patient.Notes;
    }
    public object Input => new { name = nameBox.Text.Trim(), tajNumber = tajBox.Text,
        birthDate = DateOnly.FromDateTime(birthPicker.Value), email = Optional(emailBox.Text),
        phone = Optional(phoneBox.Text), address = Optional(addressBox.Text), notes = Optional(notesBox.Text) };
    private static string? Optional(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private void SaveButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(nameBox.Text) || tajBox.Text.Length != 9 ||
            tajBox.Text.Any(c => c is < '0' or > '9') || birthPicker.Value.Date > DateTime.Today)
        { MessageBox.Show(this, "Név, pontosan 9 számjegyű TAJ és érvényes születési dátum szükséges."); return; }
        if (!string.IsNullOrWhiteSpace(emailBox.Text) && !new EmailAddressAttribute().IsValid(emailBox.Text.Trim()))
        { MessageBox.Show(this, "Az e-mail-cím nem érvényes."); return; }
        DialogResult = DialogResult.OK;
    }
}
