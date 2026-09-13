namespace Desktop_Pacienskezelo;
public partial class AppointmentForm : Form
{
    private ApiClient? api;
    private string patientId = "";
    private AppointmentItem? appointment;
    public AppointmentForm() { InitializeComponent(); }
    public void Configure(ApiClient client, string patient, List<PractitionerItem> doctors, AppointmentItem? item)
    {
        api = client; patientId = patient; appointment = item;
        doctorBox.DataSource = doctors.Where(d => d.BookingEnabled || d.Id == item?.PractitionerId).ToList();
        doctorBox.DisplayMember = "Name"; doctorBox.ValueMember = "Id";
        if (item is not null) { doctorBox.SelectedValue = item.PractitionerId; datePicker.Value = item.StartTime; }
        else datePicker.Value = DateTime.Today.AddDays(1);
        doctorBox.Enabled = item is null; noteBox.Text = item?.Note ?? "";
    }
    private async void SlotsButton_Click(object? sender, EventArgs e)
    {
        if (api is null || doctorBox.SelectedItem is not PractitionerItem doctor) return;
        slotsButton.Enabled = false;
        try {
            hourBox.DataSource = await api.Get<List<int>>($"appointments/slots?practitionerId={Uri.EscapeDataString(doctor.Id)}&date={datePicker.Value:yyyy-MM-dd}");
            if (hourBox.Items.Count == 0) MessageBox.Show(this, "Erre a napra nincs szabad időpont.");
        } catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException or TaskCanceledException) { MessageBox.Show(this, ex.Message); }
        finally { slotsButton.Enabled = true; }
    }
    private void SelectionChanged(object? sender, EventArgs e) { hourBox.DataSource = null; }
    private async void SaveButton_Click(object? sender, EventArgs e)
    {
        if (api is null || hourBox.SelectedItem is not int hour || doctorBox.SelectedItem is not PractitionerItem doctor)
        { MessageBox.Show(this, "Kérd le és válaszd ki a szabad időpontot."); return; }
        Enabled = false;
        try {
            if (appointment is null)
                await api.Post("appointments", new { patientId, practitionerId = doctor.Id, date = DateOnly.FromDateTime(datePicker.Value), hour, note = noteBox.Text });
            else await api.Put("appointments/" + appointment.Id, new { date = DateOnly.FromDateTime(datePicker.Value), hour, note = noteBox.Text });
            DialogResult = DialogResult.OK;
        } catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException or TaskCanceledException) { MessageBox.Show(this, ex.Message); }
        finally { Enabled = true; }
    }
}
