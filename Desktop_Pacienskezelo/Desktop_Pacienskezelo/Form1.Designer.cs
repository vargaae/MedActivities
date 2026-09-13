namespace Desktop_Pacienskezelo;

partial class Form1
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
        headerPanel = new Panel();
        brandLabel = new Label();
        urlLabel = new Label();
        urlBox = new TextBox();
        userLabel = new Label();
        userBox = new TextBox();
        passwordLabel = new Label();
        passwordBox = new TextBox();
        loginButton = new Button();
        logoutButton = new Button();
        statusLabel = new Label();
        contentPanel = new Panel();
        searchLabel = new Label();
        searchBox = new TextBox();
        refreshButton = new Button();
        webButton = new Button();
        patientGrid = new DataGridView();
        addPatientButton = new Button();
        editPatientButton = new Button();
        deletePatientButton = new Button();
        showPatientButton = new Button();
        recordsButton = new Button();
        selectedLabel = new Label();
        tabs = new TabControl();
        activitiesTab = new TabPage();
        bookingsTab = new TabPage();
        activityGrid = new DataGridView();
        addActivityButton = new Button();
        editActivityButton = new Button();
        deleteActivityButton = new Button();
        bookingGrid = new DataGridView();
        addBookingButton = new Button();
        moveBookingButton = new Button();
        cancelBookingButton = new Button();
        deleteBookingButton = new Button();
        statusBox = new ComboBox();
        statusBookingButton = new Button();
        SuspendLayout();
        // headerPanel
        headerPanel.Name = "headerPanel";
        headerPanel.Location = new Point(0, 0);
        headerPanel.Size = new Size(1220, 70);
        headerPanel.TabIndex = 0;
        headerPanel.BackColor = Color.FromArgb(15, 118, 110);
        headerPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        // brandLabel
        brandLabel.Name = "brandLabel";
        brandLabel.Location = new Point(24, 20);
        brandLabel.Size = new Size(800, 24);
        brandLabel.TabIndex = 1;
        brandLabel.Text = "EgészségÚt  /  Pácienskezelő";
        brandLabel.ForeColor = Color.White;
        brandLabel.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
        brandLabel.AutoSize = true;
        // urlLabel
        urlLabel.Name = "urlLabel";
        urlLabel.Location = new Point(20, 88);
        urlLabel.Size = new Size(90, 24);
        urlLabel.TabIndex = 2;
        urlLabel.Text = "API címe";
        // urlBox
        urlBox.Name = "urlBox";
        urlBox.Location = new Point(110, 84);
        urlBox.Size = new Size(430, 28);
        urlBox.TabIndex = 3;
        urlBox.MaxLength = 500;
        urlBox.Text = "https://localhost:5001/api";
        // userLabel
        userLabel.Name = "userLabel";
        userLabel.Location = new Point(560, 88);
        userLabel.Size = new Size(100, 24);
        userLabel.TabIndex = 4;
        userLabel.Text = "Felhasználó";
        // userBox
        userBox.Name = "userBox";
        userBox.Location = new Point(660, 84);
        userBox.Size = new Size(210, 28);
        userBox.TabIndex = 5;
        userBox.MaxLength = 100;
        // passwordLabel
        passwordLabel.Name = "passwordLabel";
        passwordLabel.Location = new Point(890, 88);
        passwordLabel.Size = new Size(70, 24);
        passwordLabel.TabIndex = 6;
        passwordLabel.Text = "Jelszó";
        // passwordBox
        passwordBox.Name = "passwordBox";
        passwordBox.Location = new Point(960, 84);
        passwordBox.Size = new Size(230, 28);
        passwordBox.TabIndex = 7;
        passwordBox.MaxLength = 200;
        passwordBox.UseSystemPasswordChar = true;
        // loginButton
        loginButton.Name = "loginButton";
        loginButton.Location = new Point(20, 128);
        loginButton.Size = new Size(150, 34);
        loginButton.TabIndex = 8;
        loginButton.Text = "Bejelentkezés";
        loginButton.BackColor = Color.FromArgb(15, 118, 110);
        loginButton.ForeColor = Color.White;
        loginButton.FlatStyle = FlatStyle.Flat;
        loginButton.UseVisualStyleBackColor = false;
        loginButton.Click += LoginButton_Click;
        // logoutButton
        logoutButton.Name = "logoutButton";
        logoutButton.Location = new Point(185, 128);
        logoutButton.Size = new Size(150, 34);
        logoutButton.TabIndex = 9;
        logoutButton.Text = "Kijelentkezés";
        logoutButton.BackColor = Color.FromArgb(15, 118, 110);
        logoutButton.ForeColor = Color.White;
        logoutButton.FlatStyle = FlatStyle.Flat;
        logoutButton.UseVisualStyleBackColor = false;
        logoutButton.Enabled = false;
        logoutButton.Click += LogoutButton_Click;
        // statusLabel
        statusLabel.Name = "statusLabel";
        statusLabel.Location = new Point(360, 136);
        statusLabel.Size = new Size(800, 24);
        statusLabel.TabIndex = 10;
        statusLabel.Text = "Jelentkezz be az API-hoz.";
        // contentPanel
        contentPanel.Name = "contentPanel";
        contentPanel.Location = new Point(20, 180);
        contentPanel.Size = new Size(1170, 610);
        contentPanel.TabIndex = 11;
        contentPanel.Enabled = false;
        contentPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        // searchLabel
        searchLabel.Name = "searchLabel";
        searchLabel.Location = new Point(0, 5);
        searchLabel.Size = new Size(175, 24);
        searchLabel.TabIndex = 12;
        searchLabel.Text = "Név vagy TAJ keresése";
        // searchBox
        searchBox.Name = "searchBox";
        searchBox.Location = new Point(180, 0);
        searchBox.Size = new Size(375, 28);
        searchBox.TabIndex = 13;
        searchBox.MaxLength = 100;
        searchBox.TextChanged += SearchBox_TextChanged;
        // refreshButton
        refreshButton.Name = "refreshButton";
        refreshButton.Location = new Point(575, 0);
        refreshButton.Size = new Size(110, 34);
        refreshButton.TabIndex = 14;
        refreshButton.Text = "Frissítés";
        refreshButton.BackColor = Color.FromArgb(15, 118, 110);
        refreshButton.ForeColor = Color.White;
        refreshButton.FlatStyle = FlatStyle.Flat;
        refreshButton.UseVisualStyleBackColor = false;
        refreshButton.Click += RefreshButton_Click;
        // webButton
        webButton.Name = "webButton";
        webButton.Location = new Point(700, 0);
        webButton.Size = new Size(205, 34);
        webButton.TabIndex = 15;
        webButton.Text = "Webes adminisztráció";
        webButton.BackColor = Color.FromArgb(15, 118, 110);
        webButton.ForeColor = Color.White;
        webButton.FlatStyle = FlatStyle.Flat;
        webButton.UseVisualStyleBackColor = false;
        webButton.Click += WebButton_Click;
        // patientGrid
        patientGrid.Name = "patientGrid";
        patientGrid.Location = new Point(0, 48);
        patientGrid.Size = new Size(1170, 180);
        patientGrid.TabIndex = 16;
        patientGrid.ReadOnly = true;
        patientGrid.AllowUserToAddRows = false;
        patientGrid.AllowUserToDeleteRows = false;
        patientGrid.MultiSelect = false;
        patientGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        patientGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        patientGrid.BackgroundColor = Color.White;
        patientGrid.BorderStyle = BorderStyle.None;
        patientGrid.RowHeadersVisible = false;
        patientGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        patientGrid.CellDoubleClick += PatientGrid_CellDoubleClick;
        patientGrid.SelectionChanged += PatientGrid_SelectionChanged;
        // addPatientButton
        addPatientButton.Name = "addPatientButton";
        addPatientButton.Location = new Point(0, 244);
        addPatientButton.Size = new Size(120, 34);
        addPatientButton.TabIndex = 17;
        addPatientButton.Text = "Új páciens";
        addPatientButton.BackColor = Color.FromArgb(15, 118, 110);
        addPatientButton.ForeColor = Color.White;
        addPatientButton.FlatStyle = FlatStyle.Flat;
        addPatientButton.UseVisualStyleBackColor = false;
        addPatientButton.Click += AddPatientButton_Click;
        // editPatientButton
        editPatientButton.Name = "editPatientButton";
        editPatientButton.Location = new Point(135, 244);
        editPatientButton.Size = new Size(120, 34);
        editPatientButton.TabIndex = 18;
        editPatientButton.Text = "Szerkesztés";
        editPatientButton.BackColor = Color.FromArgb(15, 118, 110);
        editPatientButton.ForeColor = Color.White;
        editPatientButton.FlatStyle = FlatStyle.Flat;
        editPatientButton.UseVisualStyleBackColor = false;
        editPatientButton.Click += EditPatientButton_Click;
        // deletePatientButton
        deletePatientButton.Name = "deletePatientButton";
        deletePatientButton.Location = new Point(270, 244);
        deletePatientButton.Size = new Size(120, 34);
        deletePatientButton.TabIndex = 19;
        deletePatientButton.Text = "Törlés";
        deletePatientButton.BackColor = Color.FromArgb(15, 118, 110);
        deletePatientButton.ForeColor = Color.White;
        deletePatientButton.FlatStyle = FlatStyle.Flat;
        deletePatientButton.UseVisualStyleBackColor = false;
        deletePatientButton.Click += DeletePatientButton_Click;
        // showPatientButton
        showPatientButton.Name = "showPatientButton";
        showPatientButton.Location = new Point(405, 244);
        showPatientButton.Size = new Size(180, 34);
        showPatientButton.TabIndex = 20;
        showPatientButton.Text = "Adatlap betöltése";
        showPatientButton.BackColor = Color.FromArgb(15, 118, 110);
        showPatientButton.ForeColor = Color.White;
        showPatientButton.FlatStyle = FlatStyle.Flat;
        showPatientButton.UseVisualStyleBackColor = false;
        showPatientButton.Click += ShowPatientButton_Click;
        // recordsButton
        recordsButton.Name = "recordsButton";
        recordsButton.Location = new Point(600, 244);
        recordsButton.Size = new Size(290, 34);
        recordsButton.TabIndex = 21;
        recordsButton.Text = "Dokumentumok / megjegyzések";
        recordsButton.BackColor = Color.FromArgb(15, 118, 110);
        recordsButton.ForeColor = Color.White;
        recordsButton.FlatStyle = FlatStyle.Flat;
        recordsButton.UseVisualStyleBackColor = false;
        recordsButton.Click += RecordsButton_Click;
        // selectedLabel
        selectedLabel.Name = "selectedLabel";
        selectedLabel.Location = new Point(0, 295);
        selectedLabel.Size = new Size(1140, 24);
        selectedLabel.TabIndex = 22;
        selectedLabel.Text = "Válassz pácienst";
        selectedLabel.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        // tabs
        tabs.Name = "tabs";
        tabs.Location = new Point(0, 330);
        tabs.Size = new Size(1170, 280);
        tabs.TabIndex = 23;
        tabs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        // activitiesTab
        activitiesTab.Name = "activitiesTab";
        activitiesTab.Location = new Point(0, 0);
        activitiesTab.Size = new Size(1160, 250);
        activitiesTab.TabIndex = 24;
        activitiesTab.Text = "Egészségügyi események";
        // bookingsTab
        bookingsTab.Name = "bookingsTab";
        bookingsTab.Location = new Point(0, 0);
        bookingsTab.Size = new Size(1160, 250);
        bookingsTab.TabIndex = 25;
        bookingsTab.Text = "Időpontok";
        // activityGrid
        activityGrid.Name = "activityGrid";
        activityGrid.Location = new Point(10, 10);
        activityGrid.Size = new Size(1130, 180);
        activityGrid.TabIndex = 26;
        activityGrid.ReadOnly = true;
        activityGrid.AllowUserToAddRows = false;
        activityGrid.AllowUserToDeleteRows = false;
        activityGrid.MultiSelect = false;
        activityGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        activityGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        activityGrid.BackgroundColor = Color.White;
        activityGrid.BorderStyle = BorderStyle.None;
        activityGrid.RowHeadersVisible = false;
        activityGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        // addActivityButton
        addActivityButton.Name = "addActivityButton";
        addActivityButton.Location = new Point(10, 200);
        addActivityButton.Size = new Size(130, 34);
        addActivityButton.TabIndex = 27;
        addActivityButton.Text = "Új esemény";
        addActivityButton.BackColor = Color.FromArgb(15, 118, 110);
        addActivityButton.ForeColor = Color.White;
        addActivityButton.FlatStyle = FlatStyle.Flat;
        addActivityButton.UseVisualStyleBackColor = false;
        addActivityButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        addActivityButton.Click += AddActivityButton_Click;
        // editActivityButton
        editActivityButton.Name = "editActivityButton";
        editActivityButton.Location = new Point(155, 200);
        editActivityButton.Size = new Size(130, 34);
        editActivityButton.TabIndex = 28;
        editActivityButton.Text = "Szerkesztés";
        editActivityButton.BackColor = Color.FromArgb(15, 118, 110);
        editActivityButton.ForeColor = Color.White;
        editActivityButton.FlatStyle = FlatStyle.Flat;
        editActivityButton.UseVisualStyleBackColor = false;
        editActivityButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        editActivityButton.Click += EditActivityButton_Click;
        // deleteActivityButton
        deleteActivityButton.Name = "deleteActivityButton";
        deleteActivityButton.Location = new Point(300, 200);
        deleteActivityButton.Size = new Size(130, 34);
        deleteActivityButton.TabIndex = 29;
        deleteActivityButton.Text = "Törlés";
        deleteActivityButton.BackColor = Color.FromArgb(15, 118, 110);
        deleteActivityButton.ForeColor = Color.White;
        deleteActivityButton.FlatStyle = FlatStyle.Flat;
        deleteActivityButton.UseVisualStyleBackColor = false;
        deleteActivityButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        deleteActivityButton.Click += DeleteActivityButton_Click;
        // bookingGrid
        bookingGrid.Name = "bookingGrid";
        bookingGrid.Location = new Point(10, 10);
        bookingGrid.Size = new Size(1130, 180);
        bookingGrid.TabIndex = 30;
        bookingGrid.ReadOnly = true;
        bookingGrid.AllowUserToAddRows = false;
        bookingGrid.AllowUserToDeleteRows = false;
        bookingGrid.MultiSelect = false;
        bookingGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        bookingGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        bookingGrid.BackgroundColor = Color.White;
        bookingGrid.BorderStyle = BorderStyle.None;
        bookingGrid.RowHeadersVisible = false;
        bookingGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        // addBookingButton
        addBookingButton.Name = "addBookingButton";
        addBookingButton.Location = new Point(10, 200);
        addBookingButton.Size = new Size(115, 34);
        addBookingButton.TabIndex = 31;
        addBookingButton.Text = "Új foglalás";
        addBookingButton.BackColor = Color.FromArgb(15, 118, 110);
        addBookingButton.ForeColor = Color.White;
        addBookingButton.FlatStyle = FlatStyle.Flat;
        addBookingButton.UseVisualStyleBackColor = false;
        addBookingButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        addBookingButton.Click += AddBookingButton_Click;
        // moveBookingButton
        moveBookingButton.Name = "moveBookingButton";
        moveBookingButton.Location = new Point(135, 200);
        moveBookingButton.Size = new Size(115, 34);
        moveBookingButton.TabIndex = 32;
        moveBookingButton.Text = "Átfoglalás";
        moveBookingButton.BackColor = Color.FromArgb(15, 118, 110);
        moveBookingButton.ForeColor = Color.White;
        moveBookingButton.FlatStyle = FlatStyle.Flat;
        moveBookingButton.UseVisualStyleBackColor = false;
        moveBookingButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        moveBookingButton.Click += MoveBookingButton_Click;
        // cancelBookingButton
        cancelBookingButton.Name = "cancelBookingButton";
        cancelBookingButton.Location = new Point(260, 200);
        cancelBookingButton.Size = new Size(115, 34);
        cancelBookingButton.TabIndex = 33;
        cancelBookingButton.Text = "Lemondás";
        cancelBookingButton.BackColor = Color.FromArgb(15, 118, 110);
        cancelBookingButton.ForeColor = Color.White;
        cancelBookingButton.FlatStyle = FlatStyle.Flat;
        cancelBookingButton.UseVisualStyleBackColor = false;
        cancelBookingButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        cancelBookingButton.Click += CancelBookingButton_Click;
        // deleteBookingButton
        deleteBookingButton.Name = "deleteBookingButton";
        deleteBookingButton.Location = new Point(385, 200);
        deleteBookingButton.Size = new Size(115, 34);
        deleteBookingButton.TabIndex = 34;
        deleteBookingButton.Text = "Törlés";
        deleteBookingButton.BackColor = Color.FromArgb(15, 118, 110);
        deleteBookingButton.ForeColor = Color.White;
        deleteBookingButton.FlatStyle = FlatStyle.Flat;
        deleteBookingButton.UseVisualStyleBackColor = false;
        deleteBookingButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        deleteBookingButton.Click += DeleteBookingButton_Click;
        // statusBox
        statusBox.Name = "statusBox";
        statusBox.Location = new Point(525, 203);
        statusBox.Size = new Size(200, 28);
        statusBox.TabIndex = 35;
        statusBox.DropDownStyle = ComboBoxStyle.DropDownList;
        statusBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        statusBox.Items.AddRange(new object[] { "Scheduled", "Cancelled", "Completed", "NoShow" });
        // statusBookingButton
        statusBookingButton.Name = "statusBookingButton";
        statusBookingButton.Location = new Point(745, 200);
        statusBookingButton.Size = new Size(160, 34);
        statusBookingButton.TabIndex = 36;
        statusBookingButton.Text = "Státusz mentése";
        statusBookingButton.BackColor = Color.FromArgb(15, 118, 110);
        statusBookingButton.ForeColor = Color.White;
        statusBookingButton.FlatStyle = FlatStyle.Flat;
        statusBookingButton.UseVisualStyleBackColor = false;
        statusBookingButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        statusBookingButton.Click += StatusBookingButton_Click;
        this.Controls.Add(headerPanel);
        headerPanel.Controls.Add(brandLabel);
        this.Controls.Add(urlLabel);
        this.Controls.Add(urlBox);
        this.Controls.Add(userLabel);
        this.Controls.Add(userBox);
        this.Controls.Add(passwordLabel);
        this.Controls.Add(passwordBox);
        this.Controls.Add(loginButton);
        this.Controls.Add(logoutButton);
        this.Controls.Add(statusLabel);
        this.Controls.Add(contentPanel);
        contentPanel.Controls.Add(searchLabel);
        contentPanel.Controls.Add(searchBox);
        contentPanel.Controls.Add(refreshButton);
        contentPanel.Controls.Add(webButton);
        contentPanel.Controls.Add(patientGrid);
        contentPanel.Controls.Add(addPatientButton);
        contentPanel.Controls.Add(editPatientButton);
        contentPanel.Controls.Add(deletePatientButton);
        contentPanel.Controls.Add(showPatientButton);
        contentPanel.Controls.Add(recordsButton);
        contentPanel.Controls.Add(selectedLabel);
        contentPanel.Controls.Add(tabs);
        tabs.Controls.Add(activitiesTab);
        tabs.Controls.Add(bookingsTab);
        activitiesTab.Controls.Add(activityGrid);
        activitiesTab.Controls.Add(addActivityButton);
        activitiesTab.Controls.Add(editActivityButton);
        activitiesTab.Controls.Add(deleteActivityButton);
        bookingsTab.Controls.Add(bookingGrid);
        bookingsTab.Controls.Add(addBookingButton);
        bookingsTab.Controls.Add(moveBookingButton);
        bookingsTab.Controls.Add(cancelBookingButton);
        bookingsTab.Controls.Add(deleteBookingButton);
        bookingsTab.Controls.Add(statusBox);
        bookingsTab.Controls.Add(statusBookingButton);
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1220, 810);
        MinimumSize = new Size(1236, 849);
        Font = new Font("Segoe UI", 9F);
        BackColor = Color.FromArgb(241, 247, 246);
        Name = "Form1";
        Text = "EgészségÚt – Pácienskezelő";
        StartPosition = FormStartPosition.CenterScreen;
        Shown += Form1_Shown;
        AcceptButton = loginButton;
        ResumeLayout(false);
        PerformLayout();
    }
    #endregion
    private Panel headerPanel;
    private Label brandLabel;
    private Label urlLabel;
    private TextBox urlBox;
    private Label userLabel;
    private TextBox userBox;
    private Label passwordLabel;
    private TextBox passwordBox;
    private Button loginButton;
    private Button logoutButton;
    private Label statusLabel;
    private Panel contentPanel;
    private Label searchLabel;
    private TextBox searchBox;
    private Button refreshButton;
    private Button webButton;
    private DataGridView patientGrid;
    private Button addPatientButton;
    private Button editPatientButton;
    private Button deletePatientButton;
    private Button showPatientButton;
    private Button recordsButton;
    private Label selectedLabel;
    private TabControl tabs;
    private TabPage activitiesTab;
    private TabPage bookingsTab;
    private DataGridView activityGrid;
    private Button addActivityButton;
    private Button editActivityButton;
    private Button deleteActivityButton;
    private DataGridView bookingGrid;
    private Button addBookingButton;
    private Button moveBookingButton;
    private Button cancelBookingButton;
    private Button deleteBookingButton;
    private ComboBox statusBox;
    private Button statusBookingButton;
}
