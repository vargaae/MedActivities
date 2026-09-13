namespace Desktop_Pacienskezelo;
public partial class ActivityEditForm : Form
{
    private string? id;
    private double latitude, longitude;
    public ActivityEditForm() { InitializeComponent(); }
    public void Configure(AssignmentOptions options, string patientId, ActivityItem? item)
    {
        id = item?.Id; latitude = item?.Latitude ?? 0; longitude = item?.Longitude ?? 0;
        titleBox.Text = item?.Title ?? ""; descriptionBox.Text = item?.Description ?? "";
        categoryBox.Text = item?.Category ?? "Vizsgálat"; cityBox.Text = item?.City ?? "";
        venueBox.Text = item?.Venue ?? ""; datePicker.Value = item?.Date ?? DateTime.Now;
        statusBox.SelectedItem = item?.Status ?? "Scheduled";
        var people = options.Patients.Concat(item?.Patients ?? []).DistinctBy(p => p.Id).ToList();
        patientBox.DataSource = people; patientBox.DisplayMember = "Name"; patientBox.ValueMember = "Id";
        patientBox.SelectedValue = item?.Patients.FirstOrDefault()?.Id ?? patientId;
        patientBox.Enabled = item is null || options.CanAssign;
        var doctors = options.CanAssign ? options.Practitioners : item?.Practitioners ?? (options.OwnPractitioner is null ? [] : [options.OwnPractitioner]);
        foreach (var doctor in doctors)
            doctorList.Items.Add(doctor, item is null ? !options.CanAssign : item.Practitioners.Any(p => p.Id == doctor.Id));
        doctorList.Enabled = options.CanAssign;
    }
    public object Input => new { id, title = titleBox.Text.Trim(), date = datePicker.Value,
        description = descriptionBox.Text.Trim(), category = categoryBox.Text.Trim(), city = cityBox.Text.Trim(),
        venue = venueBox.Text.Trim(), status = statusBox.SelectedItem?.ToString(), latitude, longitude,
        patientId = (patientBox.SelectedItem as Person)?.Id,
        practitionerIds = doctorList.CheckedItems.Cast<Person>().Select(p => p.Id).ToArray() };
    private void SaveButton_Click(object? sender, EventArgs e)
    {
        if (new[] { titleBox.Text, descriptionBox.Text, categoryBox.Text, venueBox.Text }.Any(string.IsNullOrWhiteSpace) ||
            patientBox.SelectedItem is null || doctorList.CheckedItems.Count == 0)
        { MessageBox.Show(this, "Cím, leírás, kategória, helyszín, páciens és legalább egy kezelőorvos szükséges."); return; }
        DialogResult = DialogResult.OK;
    }
}
