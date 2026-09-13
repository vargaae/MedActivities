using MedActivities.Patient.Sqlite.WinForms.Models;
using MedActivities.Patient.Sqlite.WinForms.Services;
using System.ComponentModel.DataAnnotations;

namespace MedActivities.Patient.Sqlite.WinForms;

public sealed class PatientEditForm : Form
{
    private readonly TextBox txtName = new();
    private readonly TextBox txtTaj = new();
    private readonly DateTimePicker dtpBirthDate = new();
    private readonly TextBox txtEmail = new();
    private readonly TextBox txtPhone = new();
    private readonly TextBox txtAddress = new();
    private readonly TextBox txtNotes = new();
    private readonly Func<PatientRecord, Task>? savePatient;

    public PatientRecord Patient { get; }

    public PatientEditForm(PatientRecord? patient = null, Func<PatientRecord, Task>? savePatient = null)
    {
        this.savePatient = savePatient;
        Patient = patient == null
            ? new PatientRecord()
            : new PatientRecord
            {
                Id = patient.Id,
                Name = patient.Name,
                TajNumber = patient.TajNumber,
                BirthDate = patient.BirthDate,
                Email = patient.Email,
                Phone = patient.Phone,
                Address = patient.Address,
                Notes = patient.Notes
            };

        Text = patient == null ? "Új páciens" : "Páciens módosítása";
        Width = 620;
        Height = 640;
        MinimumSize = new Size(560, 560);
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
            RowCount = 9,
            AutoScroll = true
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 155));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        dtpBirthDate.Format = DateTimePickerFormat.Custom;
        dtpBirthDate.CustomFormat = "yyyy.MM.dd";
        dtpBirthDate.MaxDate = DateTime.Today;
        dtpBirthDate.Font = Theme.Regular();
        txtNotes.Multiline = true;
        txtNotes.Height = 130;
        Theme.StyleTextBox(txtName);
        Theme.StyleTextBox(txtTaj);
        txtTaj.PlaceholderText = "9 számjegy";
        txtTaj.MaxLength = 9;
        txtTaj.AccessibleName = "TAJ-szám";
        txtName.MaxLength = 100;
        txtPhone.MaxLength = 40;
        txtAddress.MaxLength = 300;
        txtNotes.MaxLength = 2000;
        Theme.StyleTextBox(txtEmail);
        Theme.StyleTextBox(txtPhone);
        Theme.StyleTextBox(txtAddress);
        Theme.StyleTextBox(txtNotes);

        AddRow(layout, 0, "Név *", txtName);
        AddRow(layout, 1, "TAJ-szám *", txtTaj);
        AddRow(layout, 2, "Születési dátum *", dtpBirthDate);
        AddRow(layout, 3, "E-mail", txtEmail);
        AddRow(layout, 4, "Telefon", txtPhone);
        AddRow(layout, 5, "Cím", txtAddress);
        AddRow(layout, 6, "Megjegyzés", txtNotes);

        var hint = new Label { Text = "* Kötelező mező", AutoSize = true, ForeColor = SystemColors.GrayText };
        layout.Controls.Add(hint, 1, 7);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, AutoSize = true };
        var btnSave = new Button { Text = "Mentés", Width = 110, Height = 38 };
        var btnCancel = new Button { Text = "Mégse", Width = 110, Height = 38, DialogResult = DialogResult.Cancel };
        Theme.StyleButton(btnSave, true);
        Theme.StyleButton(btnCancel);
        btnSave.Click += Save_Click;
        buttons.Controls.Add(btnSave);
        buttons.Controls.Add(btnCancel);
        layout.Controls.Add(buttons, 1, 8);

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
        txtName.Text = Patient.Name;
        txtTaj.Text = Patient.TajNumber;
        dtpBirthDate.Value = Patient.BirthDate < dtpBirthDate.MinDate || Patient.BirthDate > dtpBirthDate.MaxDate
            ? DateTime.Today : Patient.BirthDate;
        txtEmail.Text = Patient.Email;
        txtPhone.Text = Patient.Phone;
        txtAddress.Text = Patient.Address;
        txtNotes.Text = Patient.Notes;
    }

    private async void Save_Click(object? sender, EventArgs e)
    {
        Patient.Name = txtName.Text.Trim();
        Patient.TajNumber = txtTaj.Text.Trim();
        Patient.BirthDate = dtpBirthDate.Value.Date;
        Patient.Email = txtEmail.Text.Trim();
        Patient.Phone = txtPhone.Text.Trim();
        Patient.Address = txtAddress.Text.Trim();
        Patient.Notes = txtNotes.Text.Trim();

        try { RecordValidation.Patient(Patient); }
        catch (ValidationException ex)
        {
            MessageBox.Show(ex.Message, "Ellenőrzés", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Enabled = false;
        try
        {
            if (savePatient is not null) await savePatient(Patient);
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
