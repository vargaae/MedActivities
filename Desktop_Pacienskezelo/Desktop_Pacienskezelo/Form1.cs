using System.ComponentModel;
namespace Desktop_Pacienskezelo;

public partial class Form1 : Form
{
    private ApiClient? api;
    private List<Patient> patients = [];
    private string? loadedPatientId;
    public Form1() { InitializeComponent(); }
    private void Form1_Shown(object? sender, EventArgs e)
    {
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;
        var configured = Environment.GetEnvironmentVariable("MEDACTIVITIES_API_URL");
        if (!string.IsNullOrWhiteSpace(configured)) urlBox.Text = configured;
    }
    protected override void OnFormClosed(FormClosedEventArgs e) { api?.Dispose(); base.OnFormClosed(e); }
    private async Task Run(Func<Task> action)
    {
        contentPanel.Enabled = false; loginButton.Enabled = false; logoutButton.Enabled = false; UseWaitCursor = true;
        try { await action(); }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
        { MessageBox.Show(this, ex.Message, "MedActivities", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        finally {
            var loggedIn = api?.Session is not null;
            contentPanel.Enabled = loggedIn; loginButton.Enabled = !loggedIn; logoutButton.Enabled = loggedIn;
            urlBox.Enabled = userBox.Enabled = passwordBox.Enabled = !loggedIn; UseWaitCursor = false;
            if (!loggedIn) ClearData();
        }
    }
    private async void LoginButton_Click(object? sender, EventArgs e) => await Run(async () => {
        api?.Dispose(); api = new ApiClient();
        try { await api.Login(urlBox.Text, userBox.Text, passwordBox.Text); } finally { passwordBox.Clear(); }
        statusLabel.Text = $"{api.Session!.UserName} · {string.Join(", ", api.Session.Roles)} · közös SQL Server API";
        addPatientButton.Enabled = editPatientButton.Enabled = deletePatientButton.Enabled = api.Staff;
        addBookingButton.Enabled = moveBookingButton.Enabled = deleteBookingButton.Enabled = api.Staff;
        deleteActivityButton.Enabled = api.Staff;
        await RefreshData();
    });
    private void LogoutButton_Click(object? sender, EventArgs e)
    {
        api?.Dispose(); api = null; ClearData(); contentPanel.Enabled = false;
        loginButton.Enabled = true; logoutButton.Enabled = false; urlBox.Enabled = userBox.Enabled = passwordBox.Enabled = true;
    }
    private void ClearData()
    { patients.Clear(); loadedPatientId = null; patientGrid.DataSource = activityGrid.DataSource = bookingGrid.DataSource = null; selectedLabel.Text = "Válassz pácienst"; statusLabel.Text = "Jelentkezz be az API-hoz."; }
    private async Task RefreshData()
    {
        var selected = SelectedPatient()?.Id;
        patients = await api!.Get<List<Patient>>("patients");
        FilterPatients();
        if (selected is not null)
            foreach (DataGridViewRow row in patientGrid.Rows)
                if ((string)row.Cells["Azonosító"].Value == selected) { patientGrid.CurrentCell = row.Cells["Név"]; break; }
        await LoadPatientData();
    }
    private void FilterPatients()
    {
        var term = searchBox.Text.Trim();
        patientGrid.DataSource = patients.Where(p => p.Name.Contains(term, StringComparison.CurrentCultureIgnoreCase) || p.TajNumber.Contains(term))
            .Select(p => new { Azonosító = p.Id, Név = p.Name, TAJ = p.TajNumber, Születés = p.BirthDate, Email = p.Email, Telefon = p.Phone }).ToList();
        patientGrid.Columns["Azonosító"].Visible = false;
        activityGrid.DataSource = bookingGrid.DataSource = null; loadedPatientId = null; selectedLabel.Text = "Páciens kiválasztása után: Adatlap betöltése";
    }
    private Patient? SelectedPatient() => patientGrid.CurrentRow is null ? null : patients.FirstOrDefault(p => p.Id == (string)patientGrid.CurrentRow.Cells["Azonosító"].Value);
    private async Task LoadPatientData()
    {
        var patient = SelectedPatient();
        activityGrid.DataSource = bookingGrid.DataSource = null; loadedPatientId = null;
        if (patient is null) return;
        selectedLabel.Text = patient.Name + " · TAJ: " + patient.TajNumber;
        var activities = await api!.Get<List<ActivityItem>>("activities");
        activityGrid.DataSource = activities.Where(a => a.Patients.Any(p => p.Id == patient.Id))
            .Select(a => new { Azonosító = a.Id, Cím = a.Title, Dátum = a.Date, Állapot = a.Status, Helyszín = a.Venue }).ToList();
        activityGrid.Columns["Azonosító"].Visible = false;
        var bookings = await api.Get<List<AppointmentItem>>("appointments");
        var labels = new[] { "Rögzítve", "Lemondva", "Befejezett", "Nem jelent meg" };
        bookingGrid.DataSource = bookings.Where(a => a.PatientId == patient.Id)
            .Select(a => new { Azonosító = a.Id, Kezdés = a.StartTime, Állapot = labels[a.Status], Megjegyzés = a.Note }).ToList();
        bookingGrid.Columns["Azonosító"].Visible = false; loadedPatientId = patient.Id;
    }
    private async void RefreshButton_Click(object? sender, EventArgs e) => await Run(RefreshData);
    private void SearchBox_TextChanged(object? sender, EventArgs e) { if (api?.Session is not null) FilterPatients(); }
    private void PatientGrid_SelectionChanged(object? sender, EventArgs e)
    {
        if (SelectedPatient()?.Id == loadedPatientId) return;
        activityGrid.DataSource = bookingGrid.DataSource = null; loadedPatientId = null; selectedLabel.Text = "Adatlap betöltése gombbal töltsd be a pácienst.";
    }
    private async void PatientGrid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e) { if (e.RowIndex >= 0) await Run(LoadPatientData); }
    private async void ShowPatientButton_Click(object? sender, EventArgs e) => await Run(LoadPatientData);
    private async void AddPatientButton_Click(object? sender, EventArgs e) {
        using var form = new PatientEditForm(); form.LoadPatient(null);
        if (form.ShowDialog(this) == DialogResult.OK) await Run(async () => { await api!.Post("patients", form.Input); await RefreshData(); });
    }
    private async void EditPatientButton_Click(object? sender, EventArgs e) {
        var patient = SelectedPatient(); if (patient is null) return;
        using var form = new PatientEditForm(); form.LoadPatient(patient);
        if (form.ShowDialog(this) == DialogResult.OK) await Run(async () => { await api!.Put("patients/" + patient.Id, form.Input); await RefreshData(); });
    }
    private async void DeletePatientButton_Click(object? sender, EventArgs e) {
        var patient = SelectedPatient(); if (patient is null || !Confirm("páciens", patient.Name)) return;
        await Run(async () => { await api!.Delete("patients/" + patient.Id); await RefreshData(); });
    }
    private bool Confirm(string kind, string name) => MessageBox.Show(this, $"Megerősíted: {kind} – {name}?", "Megerősítés", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
    private void RecordsButton_Click(object? sender, EventArgs e) {
        var patient = SelectedPatient(); if (patient is null) return;
        using var form = new RecordsForm(); form.Configure(api!, patient); form.ShowDialog(this);
        if (api!.Session is null) LogoutButton_Click(sender, e);
    }
    private async void AddActivityButton_Click(object? sender, EventArgs e) => await EditActivity(false);
    private async void EditActivityButton_Click(object? sender, EventArgs e) => await EditActivity(true);
    private async Task EditActivity(bool edit) => await Run(async () => {
        var patient = SelectedPatient(); if (patient is null) return;
        var id = activityGrid.CurrentRow?.Cells["Azonosító"].Value?.ToString();
        if (edit && id is null) return;
        var options = await api!.Get<AssignmentOptions>("activities/assignment-options");
        var item = edit ? await api.Get<ActivityItem>("activities/" + id) : null;
        if ((item is not null && !item.CanEditFields) || (item is null && !options.CanCreate)) throw new InvalidOperationException("Nincs szerkesztési jogosultság.");
        if (item?.IsAppointment == true) throw new InvalidOperationException("A foglalást az Időpontok fülön módosítsd.");
        using var form = new ActivityEditForm(); form.Configure(options, patient.Id, item);
        if (form.ShowDialog(this) != DialogResult.OK) return;
        if (edit) await api.Put("activities", form.Input); else await api.Post("activities", form.Input);
        await LoadPatientData();
    });
    private async void DeleteActivityButton_Click(object? sender, EventArgs e) {
        var row = activityGrid.CurrentRow; if (row is null || !Confirm("esemény és esetleges foglalása végleges törlése", row.Cells["Cím"].Value.ToString()!)) return;
        await Run(async () => { await api!.Delete("activities/" + row.Cells["Azonosító"].Value); await LoadPatientData(); });
    }
    private async void AddBookingButton_Click(object? sender, EventArgs e) => await EditBooking(false);
    private async void MoveBookingButton_Click(object? sender, EventArgs e) => await EditBooking(true);
    private async Task EditBooking(bool edit) => await Run(async () => {
        var patient = SelectedPatient(); if (patient is null) return;
        var id = bookingGrid.CurrentRow?.Cells["Azonosító"].Value?.ToString(); if (edit && id is null) return;
        var item = edit ? await api!.Get<AppointmentItem>("appointments/" + id) : null;
        var doctors = await api!.Get<List<PractitionerItem>>("practitioners");
        using var form = new AppointmentForm(); form.Configure(api, patient.Id, doctors, item);
        if (form.ShowDialog(this) == DialogResult.OK) await LoadPatientData();
    });
    private async void CancelBookingButton_Click(object? sender, EventArgs e) => await ChangeBooking("cancel");
    private async void StatusBookingButton_Click(object? sender, EventArgs e) => await ChangeBooking("status");
    private async void DeleteBookingButton_Click(object? sender, EventArgs e) => await ChangeBooking("delete");
    private async Task ChangeBooking(string action) {
        var row = bookingGrid.CurrentRow; if (row is null) return;
        if (!Confirm(action == "delete" ? "foglalás végleges törlése" : action == "cancel" ? "foglalás lemondása" : "státusz módosítása", row.Cells["Kezdés"].Value.ToString()!)) return;
        await Run(async () => {
            var path = "appointments/" + row.Cells["Azonosító"].Value;
            if (action == "delete") await api!.Delete(path);
            else if (action == "cancel") await api!.Post(path + "/cancel", new { });
            else await api!.Put(path + "/status", new { status = statusBox.SelectedItem?.ToString() ?? "Completed" });
            await LoadPatientData();
        });
    }
    private void WebButton_Click(object? sender, EventArgs e)
    {
        if (api?.BaseAddress is not null)
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(new Uri(api.BaseAddress, "../").ToString()) { UseShellExecute = true });
    }
}
