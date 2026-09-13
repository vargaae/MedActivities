using MedActivities.Patient.Sqlite.WinForms.Models;
using MedActivities.Patient.Sqlite.WinForms.Services;

namespace MedActivities.Patient.Sqlite.WinForms;

public sealed class MainForm : Form
{
    private readonly SqliteDatabaseService database = new();
    private readonly TextBox txtSearch = new();
    private readonly DataGridView patientsGrid = new();
    private readonly DataGridView activitiesGrid = new();
    private readonly Label lblDatabase = new();
    private readonly Label lblSelectedPatient = new();
    private readonly Label lblStatus = new();
    private readonly Button btnOpenDatabase = new();
    private List<PatientRecord> patients = new();
    private bool rebinding;
    private int patientLoadVersion;
    private int activityLoadVersion;

    public MainForm()
    {
        Text = "EgészségÚt / MedActivities – Pácienskezelő";
        Width = 1280;
        Height = 800;
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1000, 650);
        Theme.Apply(this);
        BuildUi();
        Shown += async (_, _) => await ConnectToDefaultDatabaseAsync();
    }

    private void BuildUi()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, BackColor = Theme.Page };
        root.Padding = new Padding(18, 14, 18, 0);
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(BuildTopBar(), 0, 0);
        root.Controls.Add(BuildMainSplit(), 0, 1);
        lblStatus.Text = "Kapcsolódás az alapértelmezett adatbázishoz...";
        lblStatus.AutoSize = true;
        lblStatus.ForeColor = Theme.Muted;
        lblStatus.Padding = new Padding(10, 6, 10, 10);
        root.Controls.Add(lblStatus, 0, 2);
        Controls.Add(root);
    }

    private Control BuildTopBar()
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, RowCount = 2, ColumnCount = 3, Padding = new Padding(18, 12, 18, 12), BackColor = Theme.White };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var brand = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        var title = new Label { Text = "EgészségÚt", AutoSize = true, Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Theme.Ink, Margin = new Padding(0) };
        var subtitle = new Label { Text = "Pácienskezelő és egészségügyi események", AutoSize = true, Font = Theme.Regular(9f), ForeColor = Theme.Muted, Margin = new Padding(2, 2, 0, 0) };
        brand.Controls.Add(title);
        brand.Controls.Add(subtitle);

        btnOpenDatabase.Text = "Adatbázis kiválasztása";
        btnOpenDatabase.AutoSize = true;
        btnOpenDatabase.Visible = true;
        Theme.StyleButton(btnOpenDatabase, true);
        btnOpenDatabase.Click += OpenDatabase_Click;
        var btnRefresh = new Button { Text = "Frissítés", AutoSize = true };
        Theme.StyleButton(btnRefresh);
        btnRefresh.Click += async (_, _) => await LoadPatientsAsync(true);
        lblDatabase.Text = "Nincs kiválasztva adatbázis";
        lblDatabase.AutoSize = true;
        lblDatabase.MaximumSize = new Size(360, 0);
        lblDatabase.Padding = new Padding(18, 12, 18, 0);
        lblDatabase.ForeColor = Theme.Muted;
        panel.Controls.Add(brand, 0, 0);
        panel.Controls.Add(lblDatabase, 1, 0);
        panel.Controls.Add(btnOpenDatabase, 2, 0);
        panel.Controls.Add(btnRefresh, 2, 1);
        panel.SetRowSpan(brand, 2);
        panel.SetRowSpan(lblDatabase, 2);
        return panel;
    }

    private Control BuildMainSplit()
    {
        var split = new SplitContainer
        {
            Size = new Size(1200, 560), Dock = DockStyle.Fill, Orientation = Orientation.Vertical,
            SplitterDistance = 600, Panel1MinSize = 420, Panel2MinSize = 360,
            BackColor = Theme.Page, Padding = new Padding(0, 12, 0, 0)
        };
        split.Panel1.Controls.Add(BuildPatientPanel());
        split.Panel2.Controls.Add(BuildActivityPanel());
        return split;
    }

    private Control BuildPatientPanel()
    {
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, Padding = new Padding(10, 10, 8, 8), BackColor = Theme.White };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, BackColor = Theme.White };
        toolbar.Controls.Add(new Label { Text = "Páciensek", AutoSize = true, Font = Theme.Medium(12f), ForeColor = Theme.Ink, Padding = new Padding(0, 6, 10, 0) });
        txtSearch.Width = 240;
        txtSearch.PlaceholderText = "név, TAJ, e-mail, telefon, cím";
        Theme.StyleTextBox(txtSearch);
        txtSearch.KeyDown += async (_, e) => { if (e.KeyCode == Keys.Enter) { await LoadPatientsAsync(false); e.SuppressKeyPress = true; } };
        toolbar.Controls.Add(txtSearch);
        var btnSearch = new Button { Text = "Keresés", AutoSize = true };
        Theme.StyleButton(btnSearch, true);
        btnSearch.Click += async (_, _) => await LoadPatientsAsync(false);
        var btnAll = new Button { Text = "Összes", AutoSize = true };
        Theme.StyleButton(btnAll);
        btnAll.Click += async (_, _) => { txtSearch.Clear(); await LoadPatientsAsync(false); };
        var btnAdd = new Button { Text = "+  Új páciens", AutoSize = true };
        Theme.StyleButton(btnAdd);
        btnAdd.Click += AddPatient_Click;
        var btnEdit = new Button { Text = "Módosítás", AutoSize = true };
        Theme.StyleButton(btnEdit);
        btnEdit.Click += EditPatient_Click;
        var btnDelete = new Button { Text = "Törlés", AutoSize = true };
        Theme.StyleButton(btnDelete);
        btnDelete.Click += DeletePatient_Click;
        toolbar.Controls.AddRange([btnSearch, btnAll, btnAdd, btnEdit, btnDelete]);

        patientsGrid.Dock = DockStyle.Fill;
        patientsGrid.ReadOnly = true;
        patientsGrid.AllowUserToAddRows = false;
        patientsGrid.AllowUserToDeleteRows = false;
        patientsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        patientsGrid.MultiSelect = false;
        patientsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        Theme.StyleGrid(patientsGrid);
        patientsGrid.SelectionChanged += async (_, _) => await LoadSelectedPatientActivitiesAsync();
        patientsGrid.DoubleClick += EditPatient_Click;

        var navigation = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, BackColor = Theme.White };
        var btnPrev = new Button { Text = "◀ Előző páciens", AutoSize = true };
        Theme.StyleButton(btnPrev);
        btnPrev.Click += (_, _) => MovePatient(-1);
        var btnNext = new Button { Text = "Következő páciens ▶", AutoSize = true };
        Theme.StyleButton(btnNext);
        btnNext.Click += (_, _) => MovePatient(1);
        navigation.Controls.Add(btnPrev);
        navigation.Controls.Add(btnNext);
        navigation.Controls.Add(new Label { Text = "A nyilakkal a teljes pácienslistát végig tudod nézni.", AutoSize = true, Padding = new Padding(10, 8, 0, 0) });

        layout.Controls.Add(toolbar, 0, 0);
        layout.Controls.Add(patientsGrid, 0, 1);
        layout.Controls.Add(navigation, 0, 2);
        return layout;
    }

    private Control BuildActivityPanel()
    {
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, Padding = new Padding(8, 10, 10, 8), BackColor = Theme.White };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        lblSelectedPatient.Text = "Nincs kiválasztott páciens";
        lblSelectedPatient.Font = Theme.Medium(12f);
        lblSelectedPatient.ForeColor = Theme.Ink;
        lblSelectedPatient.AutoSize = true;
        lblSelectedPatient.Padding = new Padding(0, 5, 0, 5);

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, BackColor = Theme.White };
        var btnAdd = new Button { Text = "+  Új esemény", AutoSize = true };
        Theme.StyleButton(btnAdd, true);
        btnAdd.Click += AddActivity_Click;
        var btnEdit = new Button { Text = "Módosítás", AutoSize = true };
        Theme.StyleButton(btnEdit);
        btnEdit.Click += EditActivity_Click;
        var btnDelete = new Button { Text = "Törlés", AutoSize = true };
        Theme.StyleButton(btnDelete);
        btnDelete.Click += DeleteActivity_Click;
        toolbar.Controls.AddRange([btnAdd, btnEdit, btnDelete]);

        activitiesGrid.Dock = DockStyle.Fill;
        activitiesGrid.ReadOnly = true;
        activitiesGrid.AllowUserToAddRows = false;
        activitiesGrid.AllowUserToDeleteRows = false;
        activitiesGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        activitiesGrid.MultiSelect = false;
        activitiesGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        Theme.StyleGrid(activitiesGrid);
        activitiesGrid.DoubleClick += EditActivity_Click;

        layout.Controls.Add(lblSelectedPatient, 0, 0);
        layout.Controls.Add(toolbar, 0, 1);
        layout.Controls.Add(activitiesGrid, 0, 2);
        return layout;
    }

    private async void OpenDatabase_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "SQLite adatbázis (*.db;*.sqlite;*.sqlite3)|*.db;*.sqlite;*.sqlite3|Minden fájl (*.*)|*.*",
            Title = "Válaszd ki az SQLite adatbázist",
            FileName = "activities.db"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            var backup = await database.ConnectAsync(dialog.FileName);
            activityLoadVersion++;
            lblDatabase.Text = dialog.FileName;
            await LoadPatientsAsync(false);
            SetStatus(backup is null ? "Kapcsolódva. A páciens- és eseményadatok betölthetők."
                : $"Régi kapcsolatok átemelve. Biztonsági másolat: {backup}");
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private async Task LoadPatientsAsync(bool keepSelection)
    {
        if (!EnsureDatabaseSelected()) return;
        var version = ++patientLoadVersion;
        var path = database.DatabasePath;
        try
        {
            var selectedId = keepSelection ? SelectedPatient()?.Id : null;
            var loaded = await database.GetPatientsAsync(txtSearch.Text);
            if (IsDisposed || version != patientLoadVersion || path != database.DatabasePath) return;
            patients = loaded;
            rebinding = true;
            activityLoadVersion++;
            try
            {
                patientsGrid.DataSource = null;
                patientsGrid.DataSource = patients;
                HideTechnicalColumns();

                if (patients.Count == 0)
                {
                    activitiesGrid.DataSource = null;
                    lblSelectedPatient.Text = "Nincs kiválasztott páciens";
                    SetStatus("Nincs megjeleníthető páciens.");
                    return;
                }

                var index = selectedId == null ? 0 : patients.FindIndex(x => x.Id == selectedId);
                if (index < 0) index = 0;
                patientsGrid.ClearSelection();
                patientsGrid.CurrentCell = patientsGrid.Rows[index].Cells[nameof(PatientRecord.Name)];
                patientsGrid.Rows[index].Selected = true;
            }
            finally { rebinding = false; }
            await LoadSelectedPatientActivitiesAsync();
            SetStatus($"Páciensek: {patients.Count} rekord.");
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void HideTechnicalColumns()
    {
        if (patientsGrid.Columns[nameof(PatientRecord.Id)] != null)
            patientsGrid.Columns[nameof(PatientRecord.Id)]!.Visible = false;
        if (patientsGrid.Columns[nameof(PatientRecord.Notes)] != null)
            patientsGrid.Columns[nameof(PatientRecord.Notes)]!.Width = 180;
    }

    private async Task LoadSelectedPatientActivitiesAsync()
    {
        if (rebinding || !EnsureDatabaseSelected(false)) return;
        var version = ++activityLoadVersion;
        var path = database.DatabasePath;
        var patient = SelectedPatient();
        activitiesGrid.DataSource = null;
        if (patient == null)
        {
            activitiesGrid.DataSource = null;
            lblSelectedPatient.Text = "Nincs kiválasztott páciens";
            return;
        }

        lblSelectedPatient.Text = $"{patient.Name} – események";
        try
        {
            var activities = await database.GetActivitiesForPatientAsync(patient.Id);
            if (IsDisposed || version != activityLoadVersion || path != database.DatabasePath || SelectedPatient()?.Id != patient.Id) return;
            activitiesGrid.DataSource = activities;
        }
        catch (Exception ex) { if (!IsDisposed && version == activityLoadVersion) ShowError(ex); }
    }

    private void MovePatient(int direction)
    {
        if (patientsGrid.Rows.Count == 0) return;
        var current = patientsGrid.CurrentRow?.Index ?? 0;
        var next = Math.Clamp(current + direction, 0, patientsGrid.Rows.Count - 1);
        patientsGrid.ClearSelection();
        patientsGrid.CurrentCell = patientsGrid.Rows[next].Cells[nameof(PatientRecord.Name)];
        patientsGrid.Rows[next].Selected = true;
    }

    private async void AddPatient_Click(object? sender, EventArgs e)
    {
        if (!EnsureDatabaseSelected()) return;
        using var form = new PatientEditForm(savePatient: database.InsertPatientAsync);
        if (form.ShowDialog(this) != DialogResult.OK) return;
        try { await LoadPatientsAsync(false); SetStatus("Új páciens elmentve."); }
        catch (Exception ex) { ShowError(ex); }
    }

    private async void EditPatient_Click(object? sender, EventArgs e)
    {
        if (!EnsureDatabaseSelected()) return;
        var patient = SelectedPatient();
        if (patient == null) { MessageBox.Show("Válassz ki egy pácienst."); return; }
        using var form = new PatientEditForm(patient, database.UpdatePatientAsync);
        if (form.ShowDialog(this) != DialogResult.OK) return;
        try { await LoadPatientsAsync(true); SetStatus("Páciens módosítva."); }
        catch (Exception ex) { ShowError(ex); }
    }

    private async void DeletePatient_Click(object? sender, EventArgs e)
    {
        if (!EnsureDatabaseSelected()) return;
        var patient = SelectedPatient();
        if (patient == null) { MessageBox.Show("Válassz ki egy pácienst."); return; }
        if (MessageBox.Show($"Biztosan törlöd: {patient.Name}?\nEseményhez vagy foglaláshoz kapcsolt páciens nem törölhető.", "Páciens törlése", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try { await database.DeletePatientAsync(patient.Id); await LoadPatientsAsync(false); SetStatus("Páciens törölve."); }
        catch (Exception ex) { ShowError(ex); }
    }

    private async void AddActivity_Click(object? sender, EventArgs e)
    {
        if (!EnsureDatabaseSelected()) return;
        var patient = SelectedPatient();
        if (patient == null) { MessageBox.Show("Először válassz ki egy pácienst."); return; }
        try
        {
            using var form = new ActivityEditForm(patient.Id, await database.GetPractitionersAsync(),
                saveActivity: database.InsertActivityAsync);
            if (form.ShowDialog(this) != DialogResult.OK) return;
            await LoadSelectedPatientActivitiesAsync();
            SetStatus($"Esemény elmentve: {patient.Name}.");
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private async void EditActivity_Click(object? sender, EventArgs e)
    {
        if (!EnsureDatabaseSelected()) return;
        var patient = SelectedPatient();
        var activity = SelectedActivity();
        if (patient == null || activity == null) { MessageBox.Show("Válassz ki egy eseményt."); return; }
        if (activity.IsAppointment) { MessageBox.Show("Foglalási eseményt a webes időpontkezelőben módosíthatsz."); return; }
        try
        {
            using var form = new ActivityEditForm(patient.Id, await database.GetPractitionersAsync(), activity,
                database.UpdateActivityAsync);
            if (form.ShowDialog(this) != DialogResult.OK) return;
            await LoadSelectedPatientActivitiesAsync();
            SetStatus("Esemény módosítva.");
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private async void DeleteActivity_Click(object? sender, EventArgs e)
    {
        if (!EnsureDatabaseSelected()) return;
        var activity = SelectedActivity();
        if (activity == null) { MessageBox.Show("Válassz ki egy eseményt."); return; }
        if (activity.IsAppointment) { MessageBox.Show("Foglalási eseményt a webes időpontkezelőben törölhetsz."); return; }
        if (MessageBox.Show($"Biztosan törlöd ezt az eseményt?\n{activity.Title}", "Esemény törlése", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try { await database.DeleteActivityAsync(activity.Id, activity.PatientId!); await LoadSelectedPatientActivitiesAsync(); SetStatus("Esemény törölve."); }
        catch (Exception ex) { ShowError(ex); }
    }

    private PatientRecord? SelectedPatient() => patientsGrid.CurrentRow?.DataBoundItem as PatientRecord;
    private ActivityRecord? SelectedActivity() => activitiesGrid.CurrentRow?.DataBoundItem as ActivityRecord;

    private async Task ConnectToDefaultDatabaseAsync()
    {
        var defaultDatabasePath = FindDefaultDatabasePath();
        if (defaultDatabasePath == null)
        {
            btnOpenDatabase.Visible = true;
            lblDatabase.Text = "Az API/activities.db nem található";
            SetStatus("Az alapértelmezett adatbázis nem található. Válaszd ki kézzel az SQLite adatbázist.");
            return;
        }

        try
        {
            var backup = await database.ConnectAsync(defaultDatabasePath);
            lblDatabase.Text = "Automatikusan csatlakoztatva: API/activities.db";
            await LoadPatientsAsync(false);
            SetStatus(backup is null ? "Kapcsolódva az alapértelmezett activities.db adatbázishoz."
                : $"Régi kapcsolatok átemelve. Biztonsági másolat: {backup}");
        }
        catch (Exception ex)
        {
            btnOpenDatabase.Visible = true;
            lblDatabase.Text = "Nem sikerült kapcsolódni az alapértelmezett adatbázishoz";
            SetStatus("Az automatikus adatbázis-kapcsolat nem sikerült. Válaszd ki kézzel az adatbázist.");
            ShowError(ex);
        }
    }

    private static string? FindDefaultDatabasePath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            var candidate = Path.Combine(directory.FullName, "API", "activities.db");
            if (File.Exists(candidate)) return candidate;
            directory = directory.Parent;
        }

        return null;
    }

    private bool EnsureDatabaseSelected(bool showMessage = true)
    {
        if (!string.IsNullOrWhiteSpace(database.DatabasePath)) return true;
        if (showMessage) MessageBox.Show("Az alapértelmezett adatbázis nem érhető el. Válassz ki egy SQLite adatbázisfájlt.", "Nincs adatbázis", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return false;
    }

    private void SetStatus(string text) => lblStatus.Text = text;
    private void ShowError(Exception ex) { SetStatus("Hiba történt."); MessageBox.Show(ex.Message, "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error); }
}
