namespace Desktop_Pacienskezelo;

partial class RecordsForm
{
    private System.ComponentModel.IContainer components;
    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        tabs = new TabControl();
        notesTab = new TabPage();
        docsTab = new TabPage();
        notesGrid = new DataGridView();
        noteBox = new TextBox();
        addNote = new Button();
        saveNote = new Button();
        deleteNote = new Button();
        docsGrid = new DataGridView();
        titleLabel = new Label();
        titleBox = new TextBox();
        upload = new Button();
        download = new Button();
        rename = new Button();
        deleteDoc = new Button();
        SuspendLayout();
        // tabs
        tabs.Name = "tabs";
        tabs.Location = new Point(16, 16);
        tabs.Size = new Size(930, 590);
        tabs.TabIndex = 0;
        tabs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        // notesTab
        notesTab.Name = "notesTab";
        notesTab.Location = new Point(0, 0);
        notesTab.Size = new Size(920, 560);
        notesTab.TabIndex = 1;
        notesTab.Text = "Megjegyzések";
        // docsTab
        docsTab.Name = "docsTab";
        docsTab.Location = new Point(0, 0);
        docsTab.Size = new Size(920, 560);
        docsTab.TabIndex = 2;
        docsTab.Text = "Dokumentumok";
        // notesGrid
        notesGrid.Name = "notesGrid";
        notesGrid.Location = new Point(12, 12);
        notesGrid.Size = new Size(890, 320);
        notesGrid.TabIndex = 3;
        notesGrid.ReadOnly = true;
        notesGrid.AllowUserToAddRows = false;
        notesGrid.AllowUserToDeleteRows = false;
        notesGrid.MultiSelect = false;
        notesGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        notesGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        notesGrid.BackgroundColor = Color.White;
        notesGrid.BorderStyle = BorderStyle.None;
        notesGrid.RowHeadersVisible = false;
        notesGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        notesGrid.SelectionChanged += NotesGrid_SelectionChanged;
        // noteBox
        noteBox.Name = "noteBox";
        noteBox.Location = new Point(12, 346);
        noteBox.Size = new Size(890, 120);
        noteBox.TabIndex = 4;
        noteBox.MaxLength = 2000;
        noteBox.Multiline = true;
        noteBox.ScrollBars = ScrollBars.Vertical;
        noteBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        // addNote
        addNote.Name = "addNote";
        addNote.Location = new Point(12, 486);
        addNote.Size = new Size(170, 34);
        addNote.TabIndex = 5;
        addNote.Text = "Új megjegyzés";
        addNote.BackColor = Color.FromArgb(15, 118, 110);
        addNote.ForeColor = Color.White;
        addNote.FlatStyle = FlatStyle.Flat;
        addNote.UseVisualStyleBackColor = false;
        addNote.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        addNote.Click += AddNote_Click;
        // saveNote
        saveNote.Name = "saveNote";
        saveNote.Location = new Point(198, 486);
        saveNote.Size = new Size(190, 34);
        saveNote.TabIndex = 6;
        saveNote.Text = "Kijelölt módosítása";
        saveNote.BackColor = Color.FromArgb(15, 118, 110);
        saveNote.ForeColor = Color.White;
        saveNote.FlatStyle = FlatStyle.Flat;
        saveNote.UseVisualStyleBackColor = false;
        saveNote.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        saveNote.Click += SaveNote_Click;
        // deleteNote
        deleteNote.Name = "deleteNote";
        deleteNote.Location = new Point(404, 486);
        deleteNote.Size = new Size(170, 34);
        deleteNote.TabIndex = 7;
        deleteNote.Text = "Kijelölt törlése";
        deleteNote.BackColor = Color.FromArgb(15, 118, 110);
        deleteNote.ForeColor = Color.White;
        deleteNote.FlatStyle = FlatStyle.Flat;
        deleteNote.UseVisualStyleBackColor = false;
        deleteNote.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        deleteNote.Click += DeleteNote_Click;
        // docsGrid
        docsGrid.Name = "docsGrid";
        docsGrid.Location = new Point(12, 12);
        docsGrid.Size = new Size(890, 350);
        docsGrid.TabIndex = 8;
        docsGrid.ReadOnly = true;
        docsGrid.AllowUserToAddRows = false;
        docsGrid.AllowUserToDeleteRows = false;
        docsGrid.MultiSelect = false;
        docsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        docsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        docsGrid.BackgroundColor = Color.White;
        docsGrid.BorderStyle = BorderStyle.None;
        docsGrid.RowHeadersVisible = false;
        docsGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        docsGrid.SelectionChanged += DocsGrid_SelectionChanged;
        // titleLabel
        titleLabel.Name = "titleLabel";
        titleLabel.Location = new Point(12, 380);
        titleLabel.Size = new Size(860, 24);
        titleLabel.TabIndex = 9;
        titleLabel.Text = "Dokumentum címe (PDF, PNG, JPEG, TXT; legfeljebb 5 MB)";
        titleLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        // titleBox
        titleBox.Name = "titleBox";
        titleBox.Location = new Point(12, 412);
        titleBox.Size = new Size(890, 28);
        titleBox.TabIndex = 10;
        titleBox.MaxLength = 200;
        titleBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        // upload
        upload.Name = "upload";
        upload.Location = new Point(12, 475);
        upload.Size = new Size(190, 34);
        upload.TabIndex = 11;
        upload.Text = "Új feltöltés";
        upload.BackColor = Color.FromArgb(15, 118, 110);
        upload.ForeColor = Color.White;
        upload.FlatStyle = FlatStyle.Flat;
        upload.UseVisualStyleBackColor = false;
        upload.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        upload.Click += Upload_Click;
        // download
        download.Name = "download";
        download.Location = new Point(217, 475);
        download.Size = new Size(190, 34);
        download.TabIndex = 12;
        download.Text = "Letöltés";
        download.BackColor = Color.FromArgb(15, 118, 110);
        download.ForeColor = Color.White;
        download.FlatStyle = FlatStyle.Flat;
        download.UseVisualStyleBackColor = false;
        download.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        download.Click += Download_Click;
        // rename
        rename.Name = "rename";
        rename.Location = new Point(422, 475);
        rename.Size = new Size(190, 34);
        rename.TabIndex = 13;
        rename.Text = "Cím módosítása";
        rename.BackColor = Color.FromArgb(15, 118, 110);
        rename.ForeColor = Color.White;
        rename.FlatStyle = FlatStyle.Flat;
        rename.UseVisualStyleBackColor = false;
        rename.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        rename.Click += Rename_Click;
        // deleteDoc
        deleteDoc.Name = "deleteDoc";
        deleteDoc.Location = new Point(627, 475);
        deleteDoc.Size = new Size(190, 34);
        deleteDoc.TabIndex = 14;
        deleteDoc.Text = "Törlés";
        deleteDoc.BackColor = Color.FromArgb(15, 118, 110);
        deleteDoc.ForeColor = Color.White;
        deleteDoc.FlatStyle = FlatStyle.Flat;
        deleteDoc.UseVisualStyleBackColor = false;
        deleteDoc.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        deleteDoc.Click += DeleteDoc_Click;
        this.Controls.Add(tabs);
        tabs.Controls.Add(notesTab);
        tabs.Controls.Add(docsTab);
        notesTab.Controls.Add(notesGrid);
        notesTab.Controls.Add(noteBox);
        notesTab.Controls.Add(addNote);
        notesTab.Controls.Add(saveNote);
        notesTab.Controls.Add(deleteNote);
        docsTab.Controls.Add(docsGrid);
        docsTab.Controls.Add(titleLabel);
        docsTab.Controls.Add(titleBox);
        docsTab.Controls.Add(upload);
        docsTab.Controls.Add(download);
        docsTab.Controls.Add(rename);
        docsTab.Controls.Add(deleteDoc);
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(965, 625);
        MinimumSize = new Size(981, 664);
        Font = new Font("Segoe UI", 9F);
        BackColor = Color.FromArgb(241, 247, 246);
        Name = "RecordsForm";
        Text = "Dokumentumok és megjegyzések";
        StartPosition = FormStartPosition.CenterScreen;
        Shown += RecordsForm_Shown;
        ResumeLayout(false);
        PerformLayout();
    }
    #endregion
    private TabControl tabs;
    private TabPage notesTab;
    private TabPage docsTab;
    private DataGridView notesGrid;
    private TextBox noteBox;
    private Button addNote;
    private Button saveNote;
    private Button deleteNote;
    private DataGridView docsGrid;
    private Label titleLabel;
    private TextBox titleBox;
    private Button upload;
    private Button download;
    private Button rename;
    private Button deleteDoc;
}
