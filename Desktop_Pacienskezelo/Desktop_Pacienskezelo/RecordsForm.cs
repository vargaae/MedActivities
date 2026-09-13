namespace Desktop_Pacienskezelo;
public partial class RecordsForm : Form
{
    private ApiClient? api;
    private string path = "";
    public RecordsForm() { InitializeComponent(); }
    public void Configure(ApiClient client, Patient patient)
    { api = client; path = $"patients/{patient.Id}/records"; Text = $"Dokumentumok és megjegyzések – {patient.Name}"; }
    private async void RecordsForm_Shown(object? sender, EventArgs e) { if (api is not null) await Run(RefreshData); }
    private async Task RefreshData()
    {
        notesGrid.DataSource = (await api!.Get<List<NoteItem>>(path + "/notes"))
            .Select(n => new { Azonosító = n.Id, Megjegyzés = n.Text, Szerző = n.Author, Létrehozva = n.CreatedAt.ToLocalTime(), Szerkeszthető = n.CanEdit }).ToList();
        docsGrid.DataSource = (await api.Get<List<DocumentItem>>(path + "/documents"))
            .Select(d => new { Azonosító = d.Id, Cím = d.Title, Fájlnév = d.FileName, Méret = d.Size, Létrehozva = d.CreatedAt.ToLocalTime(), Szerkeszthető = d.CanEdit }).ToList();
        foreach (var grid in new[] { notesGrid, docsGrid })
        { grid.Columns["Azonosító"].Visible = false; grid.Columns["Szerkeszthető"].Visible = false; }
    }
    private async Task Run(Func<Task> work)
    {
        tabs.Enabled = false; UseWaitCursor = true;
        try { await work(); }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException or IOException or UnauthorizedAccessException)
        { MessageBox.Show(this, ex.Message, "A művelet nem sikerült", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        finally { tabs.Enabled = true; UseWaitCursor = false; }
    }
    private string? Selected(DataGridView grid, bool edit = false)
    {
        var row = grid.CurrentRow;
        if (row is null) { MessageBox.Show(this, "Válassz egy rekordot."); return null; }
        if (edit && !(bool)row.Cells["Szerkeszthető"].Value) { MessageBox.Show(this, "Csak a saját vagy adminisztrálható rekord módosítható."); return null; }
        return (string)row.Cells["Azonosító"].Value;
    }
    private async void AddNote_Click(object? sender, EventArgs e)
    { if (string.IsNullOrWhiteSpace(noteBox.Text)) return; await Run(async () => { await api!.Post(path + "/notes", new { text = noteBox.Text.Trim() }); noteBox.Clear(); await RefreshData(); }); }
    private void NotesGrid_SelectionChanged(object? sender, EventArgs e)
    { if (notesGrid.CurrentRow is not null) noteBox.Text = notesGrid.CurrentRow.Cells["Megjegyzés"].Value?.ToString(); }
    private async void SaveNote_Click(object? sender, EventArgs e)
    { var id = Selected(notesGrid, true); if (id is null || string.IsNullOrWhiteSpace(noteBox.Text)) return; await Run(async () => { await api!.Put(path + "/notes/" + id, new { text = noteBox.Text.Trim() }); await RefreshData(); }); }
    private async void DeleteNote_Click(object? sender, EventArgs e)
    { var id = Selected(notesGrid, true); if (id is null || !Confirm()) return; await Run(async () => { await api!.Delete(path + "/notes/" + id); await RefreshData(); }); }
    private bool Confirm() => MessageBox.Show(this, "Biztosan végleg törlöd a kijelölt rekordot?", "Törlés", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
    private async void Upload_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(titleBox.Text)) { MessageBox.Show(this, "Add meg a dokumentum címét."); return; }
        using var dialog = new OpenFileDialog { Filter = "Dokumentumok|*.pdf;*.png;*.jpg;*.jpeg;*.txt", CheckFileExists = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        await Run(async () => { await api!.Upload(path + "/documents", titleBox.Text.Trim(), dialog.FileName); await RefreshData(); });
    }
    private async void Rename_Click(object? sender, EventArgs e)
    { var id = Selected(docsGrid, true); if (id is null || string.IsNullOrWhiteSpace(titleBox.Text)) return; await Run(async () => { await api!.Put(path + "/documents/" + id, new { title = titleBox.Text.Trim() }); await RefreshData(); }); }
    private async void DeleteDoc_Click(object? sender, EventArgs e)
    { var id = Selected(docsGrid, true); if (id is null || !Confirm()) return; await Run(async () => { await api!.Delete(path + "/documents/" + id); await RefreshData(); }); }
    private async void Download_Click(object? sender, EventArgs e)
    {
        var id = Selected(docsGrid); if (id is null) return;
        using var dialog = new SaveFileDialog { FileName = Path.GetFileName(docsGrid.CurrentRow!.Cells["Fájlnév"].Value.ToString()), OverwritePrompt = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        await Run(async () => { var bytes = await api!.Download(path + "/documents/" + id + "/content"); await File.WriteAllBytesAsync(dialog.FileName, bytes); });
    }
    private void DocsGrid_SelectionChanged(object? sender, EventArgs e)
    { if (docsGrid.CurrentRow is not null) titleBox.Text = docsGrid.CurrentRow.Cells["Cím"].Value?.ToString(); }
}
