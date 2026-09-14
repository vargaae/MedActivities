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
        activityGrid = new DataGridView();
        addActivityButton = new Button();
        editActivityButton = new Button();
        deleteActivityButton = new Button();
        bookingsTab = new TabPage();
        bookingGrid = new DataGridView();
        addBookingButton = new Button();
        moveBookingButton = new Button();
        cancelBookingButton = new Button();
        deleteBookingButton = new Button();
        statusBox = new ComboBox();
        statusBookingButton = new Button();
        headerPanel.SuspendLayout();
        contentPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)patientGrid).BeginInit();
        tabs.SuspendLayout();
        activitiesTab.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)activityGrid).BeginInit();
        bookingsTab.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)bookingGrid).BeginInit();
        SuspendLayout();
        // 
        // headerPanel
        // 
        headerPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        headerPanel.BackColor = Color.FromArgb(15, 118, 110);
        headerPanel.Controls.Add(brandLabel);
        headerPanel.Location = new Point(0, 0);
        headerPanel.Margin = new Padding(3, 4, 3, 4);
        headerPanel.Name = "headerPanel";
        headerPanel.Size = new Size(1394, 93);
        headerPanel.TabIndex = 0;
        // 
        // brandLabel
        // 
        brandLabel.AutoSize = true;
        brandLabel.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
        brandLabel.ForeColor = Color.White;
        brandLabel.Location = new Point(27, 27);
        brandLabel.Name = "brandLabel";
        brandLabel.Size = new Size(416, 41);
        brandLabel.TabIndex = 1;
        brandLabel.Text = "EgészségÚt  /  Pácienskezelő";
        // 
        // urlLabel
        // 
        urlLabel.Location = new Point(23, 117);
        urlLabel.Name = "urlLabel";
        urlLabel.Size = new Size(103, 32);
        urlLabel.TabIndex = 2;
        urlLabel.Text = "API címe";
        // 
        // urlBox
        // 
        urlBox.Location = new Point(126, 112);
        urlBox.Margin = new Padding(3, 4, 3, 4);
        urlBox.MaxLength = 500;
        urlBox.Name = "urlBox";
        urlBox.Size = new Size(491, 27);
        urlBox.TabIndex = 3;
        urlBox.Text = "https://localhost:5001/api";
        // 
        // userLabel
        // 
        userLabel.Location = new Point(640, 117);
        userLabel.Name = "userLabel";
        userLabel.Size = new Size(114, 32);
        userLabel.TabIndex = 4;
        userLabel.Text = "Felhasználó";
        // 
        // userBox
        // 
        userBox.Location = new Point(754, 112);
        userBox.Margin = new Padding(3, 4, 3, 4);
        userBox.MaxLength = 100;
        userBox.Name = "userBox";
        userBox.Size = new Size(239, 27);
        userBox.TabIndex = 5;
        // 
        // passwordLabel
        // 
        passwordLabel.Location = new Point(1017, 117);
        passwordLabel.Name = "passwordLabel";
        passwordLabel.Size = new Size(80, 32);
        passwordLabel.TabIndex = 6;
        passwordLabel.Text = "Jelszó";
        // 
        // passwordBox
        // 
        passwordBox.Location = new Point(1097, 112);
        passwordBox.Margin = new Padding(3, 4, 3, 4);
        passwordBox.MaxLength = 200;
        passwordBox.Name = "passwordBox";
        passwordBox.Size = new Size(262, 27);
        passwordBox.TabIndex = 7;
        passwordBox.UseSystemPasswordChar = true;
        // 
        // loginButton
        // 
        loginButton.BackColor = Color.FromArgb(15, 118, 110);
        loginButton.FlatStyle = FlatStyle.Flat;
        loginButton.ForeColor = Color.White;
        loginButton.Location = new Point(23, 171);
        loginButton.Margin = new Padding(3, 4, 3, 4);
        loginButton.Name = "loginButton";
        loginButton.Size = new Size(171, 45);
        loginButton.TabIndex = 8;
        loginButton.Text = "Bejelentkezés";
        loginButton.UseVisualStyleBackColor = false;
        loginButton.Click += LoginButton_Click;
        // 
        // logoutButton
        // 
        logoutButton.BackColor = Color.FromArgb(15, 118, 110);
        logoutButton.Enabled = false;
        logoutButton.FlatStyle = FlatStyle.Flat;
        logoutButton.ForeColor = Color.White;
        logoutButton.Location = new Point(211, 171);
        logoutButton.Margin = new Padding(3, 4, 3, 4);
        logoutButton.Name = "logoutButton";
        logoutButton.Size = new Size(171, 45);
        logoutButton.TabIndex = 9;
        logoutButton.Text = "Kijelentkezés";
        logoutButton.UseVisualStyleBackColor = false;
        logoutButton.Click += LogoutButton_Click;
        // 
        // statusLabel
        // 
        statusLabel.Location = new Point(411, 181);
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(914, 32);
        statusLabel.TabIndex = 10;
        statusLabel.Text = "Jelentkezz be az API-hoz.";
        // 
        // contentPanel
        // 
        contentPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
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
        contentPanel.Enabled = false;
        contentPanel.Location = new Point(23, 240);
        contentPanel.Margin = new Padding(3, 4, 3, 4);
        contentPanel.Name = "contentPanel";
        contentPanel.Size = new Size(1337, 813);
        contentPanel.TabIndex = 11;
        // 
        // searchLabel
        // 
        searchLabel.Anchor = AnchorStyles.Left;
        searchLabel.Location = new Point(0, 10);
        searchLabel.Name = "searchLabel";
        searchLabel.Size = new Size(200, 30);
        searchLabel.TabIndex = 12;
        searchLabel.Text = "Név vagy TAJ keresése";
        searchLabel.Click += searchLabel_Click;
        // 
        // searchBox
        // 
        searchBox.Location = new Point(206, 10);
        searchBox.Margin = new Padding(3, 4, 3, 4);
        searchBox.MaxLength = 100;
        searchBox.Name = "searchBox";
        searchBox.Size = new Size(428, 27);
        searchBox.TabIndex = 13;
        searchBox.TextChanged += SearchBox_TextChanged;
        // 
        // refreshButton
        // 
        refreshButton.BackColor = Color.FromArgb(15, 118, 110);
        refreshButton.FlatStyle = FlatStyle.Flat;
        refreshButton.ForeColor = Color.White;
        refreshButton.Location = new Point(657, 0);
        refreshButton.Margin = new Padding(3, 4, 3, 4);
        refreshButton.Name = "refreshButton";
        refreshButton.Size = new Size(126, 45);
        refreshButton.TabIndex = 14;
        refreshButton.Text = "Frissítés";
        refreshButton.UseVisualStyleBackColor = false;
        refreshButton.Click += RefreshButton_Click;
        // 
        // webButton
        // 
        webButton.BackColor = Color.FromArgb(15, 118, 110);
        webButton.FlatStyle = FlatStyle.Flat;
        webButton.ForeColor = Color.White;
        webButton.Location = new Point(800, 0);
        webButton.Margin = new Padding(3, 4, 3, 4);
        webButton.Name = "webButton";
        webButton.Size = new Size(234, 45);
        webButton.TabIndex = 15;
        webButton.Text = "Webes adminisztráció";
        webButton.UseVisualStyleBackColor = false;
        webButton.Click += WebButton_Click;
        // 
        // patientGrid
        // 
        patientGrid.AllowUserToAddRows = false;
        patientGrid.AllowUserToDeleteRows = false;
        patientGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        patientGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        patientGrid.BackgroundColor = Color.White;
        patientGrid.BorderStyle = BorderStyle.None;
        patientGrid.ColumnHeadersHeight = 29;
        patientGrid.Location = new Point(0, 64);
        patientGrid.Margin = new Padding(3, 4, 3, 4);
        patientGrid.MultiSelect = false;
        patientGrid.Name = "patientGrid";
        patientGrid.ReadOnly = true;
        patientGrid.RowHeadersVisible = false;
        patientGrid.RowHeadersWidth = 51;
        patientGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        patientGrid.Size = new Size(1337, 240);
        patientGrid.TabIndex = 16;
        patientGrid.CellDoubleClick += PatientGrid_CellDoubleClick;
        patientGrid.SelectionChanged += PatientGrid_SelectionChanged;
        // 
        // addPatientButton
        // 
        addPatientButton.BackColor = Color.FromArgb(15, 118, 110);
        addPatientButton.FlatStyle = FlatStyle.Flat;
        addPatientButton.ForeColor = Color.White;
        addPatientButton.Location = new Point(0, 325);
        addPatientButton.Margin = new Padding(3, 4, 3, 4);
        addPatientButton.Name = "addPatientButton";
        addPatientButton.Size = new Size(137, 45);
        addPatientButton.TabIndex = 17;
        addPatientButton.Text = "Új páciens";
        addPatientButton.UseVisualStyleBackColor = false;
        addPatientButton.Click += AddPatientButton_Click;
        // 
        // editPatientButton
        // 
        editPatientButton.BackColor = Color.FromArgb(15, 118, 110);
        editPatientButton.FlatStyle = FlatStyle.Flat;
        editPatientButton.ForeColor = Color.White;
        editPatientButton.Location = new Point(154, 325);
        editPatientButton.Margin = new Padding(3, 4, 3, 4);
        editPatientButton.Name = "editPatientButton";
        editPatientButton.Size = new Size(137, 45);
        editPatientButton.TabIndex = 18;
        editPatientButton.Text = "Szerkesztés";
        editPatientButton.UseVisualStyleBackColor = false;
        editPatientButton.Click += EditPatientButton_Click;
        // 
        // deletePatientButton
        // 
        deletePatientButton.BackColor = Color.FromArgb(15, 118, 110);
        deletePatientButton.FlatStyle = FlatStyle.Flat;
        deletePatientButton.ForeColor = Color.White;
        deletePatientButton.Location = new Point(309, 325);
        deletePatientButton.Margin = new Padding(3, 4, 3, 4);
        deletePatientButton.Name = "deletePatientButton";
        deletePatientButton.Size = new Size(137, 45);
        deletePatientButton.TabIndex = 19;
        deletePatientButton.Text = "Törlés";
        deletePatientButton.UseVisualStyleBackColor = false;
        deletePatientButton.Click += DeletePatientButton_Click;
        // 
        // showPatientButton
        // 
        showPatientButton.BackColor = Color.FromArgb(15, 118, 110);
        showPatientButton.FlatStyle = FlatStyle.Flat;
        showPatientButton.ForeColor = Color.White;
        showPatientButton.Location = new Point(463, 325);
        showPatientButton.Margin = new Padding(3, 4, 3, 4);
        showPatientButton.Name = "showPatientButton";
        showPatientButton.Size = new Size(206, 45);
        showPatientButton.TabIndex = 20;
        showPatientButton.Text = "Adatlap betöltése";
        showPatientButton.UseVisualStyleBackColor = false;
        showPatientButton.Click += ShowPatientButton_Click;
        // 
        // recordsButton
        // 
        recordsButton.BackColor = Color.FromArgb(15, 118, 110);
        recordsButton.FlatStyle = FlatStyle.Flat;
        recordsButton.ForeColor = Color.White;
        recordsButton.Location = new Point(686, 325);
        recordsButton.Margin = new Padding(3, 4, 3, 4);
        recordsButton.Name = "recordsButton";
        recordsButton.Size = new Size(331, 45);
        recordsButton.TabIndex = 21;
        recordsButton.Text = "Dokumentumok / megjegyzések";
        recordsButton.UseVisualStyleBackColor = false;
        recordsButton.Click += RecordsButton_Click;
        // 
        // selectedLabel
        // 
        selectedLabel.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        selectedLabel.Location = new Point(0, 393);
        selectedLabel.Name = "selectedLabel";
        selectedLabel.Size = new Size(1303, 32);
        selectedLabel.TabIndex = 22;
        selectedLabel.Text = "Válassz pácienst";
        // 
        // tabs
        // 
        tabs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        tabs.Controls.Add(activitiesTab);
        tabs.Controls.Add(bookingsTab);
        tabs.Location = new Point(0, 440);
        tabs.Margin = new Padding(3, 4, 3, 4);
        tabs.Name = "tabs";
        tabs.SelectedIndex = 0;
        tabs.Size = new Size(1337, 373);
        tabs.TabIndex = 23;
        // 
        // activitiesTab
        // 
        activitiesTab.Controls.Add(activityGrid);
        activitiesTab.Controls.Add(addActivityButton);
        activitiesTab.Controls.Add(editActivityButton);
        activitiesTab.Controls.Add(deleteActivityButton);
        activitiesTab.Location = new Point(4, 29);
        activitiesTab.Margin = new Padding(3, 4, 3, 4);
        activitiesTab.Name = "activitiesTab";
        activitiesTab.Size = new Size(1329, 340);
        activitiesTab.TabIndex = 24;
        activitiesTab.Text = "Egészségügyi események";
        // 
        // activityGrid
        // 
        activityGrid.AllowUserToAddRows = false;
        activityGrid.AllowUserToDeleteRows = false;
        activityGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        activityGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        activityGrid.BackgroundColor = Color.White;
        activityGrid.BorderStyle = BorderStyle.None;
        activityGrid.ColumnHeadersHeight = 29;
        activityGrid.Location = new Point(11, 13);
        activityGrid.Margin = new Padding(3, 4, 3, 4);
        activityGrid.MultiSelect = false;
        activityGrid.Name = "activityGrid";
        activityGrid.ReadOnly = true;
        activityGrid.RowHeadersVisible = false;
        activityGrid.RowHeadersWidth = 51;
        activityGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        activityGrid.Size = new Size(1291, 240);
        activityGrid.TabIndex = 26;
        // 
        // addActivityButton
        // 
        addActivityButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        addActivityButton.BackColor = Color.FromArgb(15, 118, 110);
        addActivityButton.FlatStyle = FlatStyle.Flat;
        addActivityButton.ForeColor = Color.White;
        addActivityButton.Location = new Point(11, 267);
        addActivityButton.Margin = new Padding(3, 4, 3, 4);
        addActivityButton.Name = "addActivityButton";
        addActivityButton.Size = new Size(149, 45);
        addActivityButton.TabIndex = 27;
        addActivityButton.Text = "Új esemény";
        addActivityButton.UseVisualStyleBackColor = false;
        addActivityButton.Click += AddActivityButton_Click;
        // 
        // editActivityButton
        // 
        editActivityButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        editActivityButton.BackColor = Color.FromArgb(15, 118, 110);
        editActivityButton.FlatStyle = FlatStyle.Flat;
        editActivityButton.ForeColor = Color.White;
        editActivityButton.Location = new Point(177, 267);
        editActivityButton.Margin = new Padding(3, 4, 3, 4);
        editActivityButton.Name = "editActivityButton";
        editActivityButton.Size = new Size(149, 45);
        editActivityButton.TabIndex = 28;
        editActivityButton.Text = "Szerkesztés";
        editActivityButton.UseVisualStyleBackColor = false;
        editActivityButton.Click += EditActivityButton_Click;
        // 
        // deleteActivityButton
        // 
        deleteActivityButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        deleteActivityButton.BackColor = Color.FromArgb(15, 118, 110);
        deleteActivityButton.FlatStyle = FlatStyle.Flat;
        deleteActivityButton.ForeColor = Color.White;
        deleteActivityButton.Location = new Point(343, 267);
        deleteActivityButton.Margin = new Padding(3, 4, 3, 4);
        deleteActivityButton.Name = "deleteActivityButton";
        deleteActivityButton.Size = new Size(149, 45);
        deleteActivityButton.TabIndex = 29;
        deleteActivityButton.Text = "Törlés";
        deleteActivityButton.UseVisualStyleBackColor = false;
        deleteActivityButton.Click += DeleteActivityButton_Click;
        // 
        // bookingsTab
        // 
        bookingsTab.Controls.Add(bookingGrid);
        bookingsTab.Controls.Add(addBookingButton);
        bookingsTab.Controls.Add(moveBookingButton);
        bookingsTab.Controls.Add(cancelBookingButton);
        bookingsTab.Controls.Add(deleteBookingButton);
        bookingsTab.Controls.Add(statusBox);
        bookingsTab.Controls.Add(statusBookingButton);
        bookingsTab.Location = new Point(4, 29);
        bookingsTab.Margin = new Padding(3, 4, 3, 4);
        bookingsTab.Name = "bookingsTab";
        bookingsTab.Size = new Size(1329, 340);
        bookingsTab.TabIndex = 25;
        bookingsTab.Text = "Időpontok";
        // 
        // bookingGrid
        // 
        bookingGrid.AllowUserToAddRows = false;
        bookingGrid.AllowUserToDeleteRows = false;
        bookingGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        bookingGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        bookingGrid.BackgroundColor = Color.White;
        bookingGrid.BorderStyle = BorderStyle.None;
        bookingGrid.ColumnHeadersHeight = 29;
        bookingGrid.Location = new Point(11, 13);
        bookingGrid.Margin = new Padding(3, 4, 3, 4);
        bookingGrid.MultiSelect = false;
        bookingGrid.Name = "bookingGrid";
        bookingGrid.ReadOnly = true;
        bookingGrid.RowHeadersVisible = false;
        bookingGrid.RowHeadersWidth = 51;
        bookingGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        bookingGrid.Size = new Size(1291, 240);
        bookingGrid.TabIndex = 30;
        // 
        // addBookingButton
        // 
        addBookingButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        addBookingButton.BackColor = Color.FromArgb(15, 118, 110);
        addBookingButton.FlatStyle = FlatStyle.Flat;
        addBookingButton.ForeColor = Color.White;
        addBookingButton.Location = new Point(11, 267);
        addBookingButton.Margin = new Padding(3, 4, 3, 4);
        addBookingButton.Name = "addBookingButton";
        addBookingButton.Size = new Size(131, 45);
        addBookingButton.TabIndex = 31;
        addBookingButton.Text = "Új foglalás";
        addBookingButton.UseVisualStyleBackColor = false;
        addBookingButton.Click += AddBookingButton_Click;
        // 
        // moveBookingButton
        // 
        moveBookingButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        moveBookingButton.BackColor = Color.FromArgb(15, 118, 110);
        moveBookingButton.FlatStyle = FlatStyle.Flat;
        moveBookingButton.ForeColor = Color.White;
        moveBookingButton.Location = new Point(154, 267);
        moveBookingButton.Margin = new Padding(3, 4, 3, 4);
        moveBookingButton.Name = "moveBookingButton";
        moveBookingButton.Size = new Size(131, 45);
        moveBookingButton.TabIndex = 32;
        moveBookingButton.Text = "Átfoglalás";
        moveBookingButton.UseVisualStyleBackColor = false;
        moveBookingButton.Click += MoveBookingButton_Click;
        // 
        // cancelBookingButton
        // 
        cancelBookingButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        cancelBookingButton.BackColor = Color.FromArgb(15, 118, 110);
        cancelBookingButton.FlatStyle = FlatStyle.Flat;
        cancelBookingButton.ForeColor = Color.White;
        cancelBookingButton.Location = new Point(297, 267);
        cancelBookingButton.Margin = new Padding(3, 4, 3, 4);
        cancelBookingButton.Name = "cancelBookingButton";
        cancelBookingButton.Size = new Size(131, 45);
        cancelBookingButton.TabIndex = 33;
        cancelBookingButton.Text = "Lemondás";
        cancelBookingButton.UseVisualStyleBackColor = false;
        cancelBookingButton.Click += CancelBookingButton_Click;
        // 
        // deleteBookingButton
        // 
        deleteBookingButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        deleteBookingButton.BackColor = Color.FromArgb(15, 118, 110);
        deleteBookingButton.FlatStyle = FlatStyle.Flat;
        deleteBookingButton.ForeColor = Color.White;
        deleteBookingButton.Location = new Point(440, 267);
        deleteBookingButton.Margin = new Padding(3, 4, 3, 4);
        deleteBookingButton.Name = "deleteBookingButton";
        deleteBookingButton.Size = new Size(131, 45);
        deleteBookingButton.TabIndex = 34;
        deleteBookingButton.Text = "Törlés";
        deleteBookingButton.UseVisualStyleBackColor = false;
        deleteBookingButton.Click += DeleteBookingButton_Click;
        // 
        // statusBox
        // 
        statusBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        statusBox.DropDownStyle = ComboBoxStyle.DropDownList;
        statusBox.Items.AddRange(new object[] { "Scheduled", "Cancelled", "Completed", "NoShow" });
        statusBox.Location = new Point(600, 271);
        statusBox.Margin = new Padding(3, 4, 3, 4);
        statusBox.Name = "statusBox";
        statusBox.Size = new Size(228, 28);
        statusBox.TabIndex = 35;
        // 
        // statusBookingButton
        // 
        statusBookingButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        statusBookingButton.BackColor = Color.FromArgb(15, 118, 110);
        statusBookingButton.FlatStyle = FlatStyle.Flat;
        statusBookingButton.ForeColor = Color.White;
        statusBookingButton.Location = new Point(851, 267);
        statusBookingButton.Margin = new Padding(3, 4, 3, 4);
        statusBookingButton.Name = "statusBookingButton";
        statusBookingButton.Size = new Size(183, 45);
        statusBookingButton.TabIndex = 36;
        statusBookingButton.Text = "Státusz mentése";
        statusBookingButton.UseVisualStyleBackColor = false;
        statusBookingButton.Click += StatusBookingButton_Click;
        // 
        // Form1
        // 
        AcceptButton = loginButton;
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(241, 247, 246);
        ClientSize = new Size(1394, 1080);
        Controls.Add(headerPanel);
        Controls.Add(urlLabel);
        Controls.Add(urlBox);
        Controls.Add(userLabel);
        Controls.Add(userBox);
        Controls.Add(passwordLabel);
        Controls.Add(passwordBox);
        Controls.Add(loginButton);
        Controls.Add(logoutButton);
        Controls.Add(statusLabel);
        Controls.Add(contentPanel);
        Font = new Font("Segoe UI", 9F);
        Margin = new Padding(3, 4, 3, 4);
        MinimumSize = new Size(1410, 1018);
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "EgészségÚt – Pácienskezelő";
        Shown += Form1_Shown;
        headerPanel.ResumeLayout(false);
        headerPanel.PerformLayout();
        contentPanel.ResumeLayout(false);
        contentPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)patientGrid).EndInit();
        tabs.ResumeLayout(false);
        activitiesTab.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)activityGrid).EndInit();
        bookingsTab.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)bookingGrid).EndInit();
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
