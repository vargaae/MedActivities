using MedActivities.Patient.Sqlite.WinForms.Models;
using MedActivities.Patient.Sqlite.WinForms.Services;
using System.ComponentModel.DataAnnotations;

namespace MedActivities.Patient.Sqlite.WinForms;

public sealed class ActivityEditForm : Form
{
    private readonly TextBox txtTitle = new();
    private readonly DateTimePicker dtpDate = new();
    private readonly TextBox txtDescription = new();
    private readonly TextBox txtCategory = new();
    private readonly ComboBox cboStatus = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly CheckedListBox lstPractitioners = new() { CheckOnClick = true, Height = 100, IntegralHeight = false };
    private readonly IReadOnlyList<PractitionerOption> practitioners;
    private readonly Func<ActivityRecord, Task>? saveActivity;
    private readonly TextBox txtCity = new();
    private readonly TextBox txtVenue = new();

    public ActivityRecord Activity { get; }
    private sealed record StatusOption(string Code, string Label)
    {
        public override string ToString() => Label;
    }

    public ActivityEditForm(string patientId, IReadOnlyList<PractitionerOption> practitioners, ActivityRecord? activity = null,
        Func<ActivityRecord, Task>? saveActivity = null)
    {
        this.practitioners = practitioners;
        this.saveActivity = saveActivity;
        Activity = activity == null
            ? new ActivityRecord { PatientId = patientId }
            : new ActivityRecord
            {
                Id = activity.Id,
                PatientId = activity.PatientId,
                Title = activity.Title,
                Date = activity.Date,
                Description = activity.Description,
                Category = activity.Category,
                Status = activity.Status,
                PractitionerIds = [.. activity.PractitionerIds],
                IsAppointment = activity.IsAppointment,
                City = activity.City,
                Venue = activity.Venue,
                Latitude = activity.Latitude,
                Longitude = activity.Longitude
            };

        Text = activity == null ? "Új egészségügyi esemény" : "Esemény módosítása";
        Width = 620;
        Height = 710;
        MinimumSize = new Size(560, 600);
        StartPosition = FormStartPosition.CenterParent;
        Theme.Apply(this);
        BuildUi();
        LoadData();
    }

    private void BuildUi()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
            ColumnCount = 2,
            RowCount = 10,
            AutoScroll = true
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        txtDescription.Multiline = true;
        txtDescription.Height = 100;
        dtpDate.Format = DateTimePickerFormat.Custom;
        dtpDate.CustomFormat = "yyyy.MM.dd HH:mm";
        dtpDate.Font = Theme.Regular();
        Theme.StyleTextBox(txtTitle);
        Theme.StyleTextBox(txtDescription);
        Theme.StyleTextBox(txtCategory);
        Theme.StyleTextBox(txtCity);
        Theme.StyleTextBox(txtVenue);
        txtTitle.MaxLength = 200;
        txtDescription.MaxLength = 5000;
        txtCategory.MaxLength = 100;
        txtCity.MaxLength = 200;
        txtVenue.MaxLength = 300;
        cboStatus.Items.AddRange([
            new StatusOption("Scheduled", "Tervezett"),
            new StatusOption("Cancelled", "Lemondva"),
            new StatusOption("Completed", "Befejezve"),
            new StatusOption("NoShow", "Nem jelent meg")
        ]);
        foreach (var practitioner in practitioners) lstPractitioners.Items.Add(practitioner);

        AddRow(layout, 0, "Cím *", txtTitle);
        AddRow(layout, 1, "Dátum", dtpDate);
        AddRow(layout, 2, "Leírás *", txtDescription);
        AddRow(layout, 3, "Kategória *", txtCategory);
        AddRow(layout, 4, "Állapot", cboStatus);
        AddRow(layout, 5, "Város", txtCity);
        AddRow(layout, 6, "Helyszín *", txtVenue);
        AddRow(layout, 7, "Kezelőorvosok *", lstPractitioners);
        AddRow(layout, 8, "", new Label
        {
            Text = practitioners.Count == 0 ? "Először hozz létre kezelőorvost a webes felületen." : "* Kötelező mező",
            AutoSize = true, ForeColor = Theme.Muted
        });

        var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, AutoSize = true };
        var btnSave = new Button { Text = "Mentés", Width = 110, Height = 38 };
        var btnCancel = new Button { Text = "Mégse", Width = 110, Height = 38, DialogResult = DialogResult.Cancel };
        Theme.StyleButton(btnSave, true);
        Theme.StyleButton(btnCancel);
        btnSave.Click += Save_Click;
        buttons.Controls.Add(btnSave);
        buttons.Controls.Add(btnCancel);
        layout.Controls.Add(buttons, 1, 9);

        Controls.Add(layout);
        AcceptButton = btnSave;
        CancelButton = btnCancel;
    }

    private static void AddRow(TableLayoutPanel layout, int row, string label, Control control)
    {
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(new Label { Text = label, AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, row);
        control.Dock = DockStyle.Top;
        layout.Controls.Add(control, 1, row);
    }

    private void LoadData()
    {
        txtTitle.Text = Activity.Title;
        dtpDate.Value = Activity.Date < dtpDate.MinDate || Activity.Date > dtpDate.MaxDate ? DateTime.Now : Activity.Date;
        txtDescription.Text = Activity.Description;
        txtCategory.Text = Activity.Category;
        cboStatus.SelectedItem = cboStatus.Items.Cast<StatusOption>().Single(x => x.Code == Activity.Status);
        for (var i = 0; i < lstPractitioners.Items.Count; i++)
            lstPractitioners.SetItemChecked(i, Activity.PractitionerIds.Contains(((PractitionerOption)lstPractitioners.Items[i]).Id));
        txtCity.Text = Activity.City;
        txtVenue.Text = Activity.Venue;
    }

    private async void Save_Click(object? sender, EventArgs e)
    {
        Activity.Title = txtTitle.Text.Trim();
        Activity.Date = dtpDate.Value;
        Activity.Description = txtDescription.Text.Trim();
        Activity.Category = txtCategory.Text.Trim();
        Activity.Status = ((StatusOption)cboStatus.SelectedItem!).Code;
        Activity.PractitionerIds = lstPractitioners.CheckedItems.Cast<PractitionerOption>().Select(p => p.Id).ToList();
        Activity.City = txtCity.Text.Trim();
        Activity.Venue = txtVenue.Text.Trim();

        try { RecordValidation.Activity(Activity); }
        catch (ValidationException ex)
        {
            MessageBox.Show(ex.Message, "Ellenőrzés", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Enabled = false;
        try
        {
            if (saveActivity is not null) await saveActivity(Activity);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "A mentés nem sikerült", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally { if (!IsDisposed) Enabled = true; }
    }
}
