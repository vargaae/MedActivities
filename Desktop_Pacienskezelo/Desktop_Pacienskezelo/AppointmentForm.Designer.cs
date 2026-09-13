namespace Desktop_Pacienskezelo;

partial class AppointmentForm
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
        doctorLabel = new Label();
        doctorBox = new ComboBox();
        dateLabel = new Label();
        datePicker = new DateTimePicker();
        slotsButton = new Button();
        hourLabel = new Label();
        hourBox = new ComboBox();
        noteLabel = new Label();
        noteBox = new TextBox();
        saveButton = new Button();
        cancelButton = new Button();
        SuspendLayout();
        // doctorLabel
        doctorLabel.Name = "doctorLabel";
        doctorLabel.Location = new Point(20, 24);
        doctorLabel.Size = new Size(140, 24);
        doctorLabel.TabIndex = 0;
        doctorLabel.Text = "Kezelőorvos *";
        // doctorBox
        doctorBox.Name = "doctorBox";
        doctorBox.Location = new Point(180, 24);
        doctorBox.Size = new Size(355, 28);
        doctorBox.TabIndex = 1;
        doctorBox.DropDownStyle = ComboBoxStyle.DropDownList;
        doctorBox.SelectedIndexChanged += SelectionChanged;
        // dateLabel
        dateLabel.Name = "dateLabel";
        dateLabel.Location = new Point(20, 75);
        dateLabel.Size = new Size(140, 24);
        dateLabel.TabIndex = 2;
        dateLabel.Text = "Dátum *";
        // datePicker
        datePicker.Name = "datePicker";
        datePicker.Location = new Point(180, 75);
        datePicker.Size = new Size(355, 28);
        datePicker.TabIndex = 3;
        datePicker.Format = DateTimePickerFormat.Short;
        datePicker.ValueChanged += SelectionChanged;
        // slotsButton
        slotsButton.Name = "slotsButton";
        slotsButton.Location = new Point(180, 120);
        slotsButton.Size = new Size(355, 34);
        slotsButton.TabIndex = 4;
        slotsButton.Text = "Szabad időpontok lekérése";
        slotsButton.BackColor = Color.FromArgb(15, 118, 110);
        slotsButton.ForeColor = Color.White;
        slotsButton.FlatStyle = FlatStyle.Flat;
        slotsButton.UseVisualStyleBackColor = false;
        slotsButton.Click += SlotsButton_Click;
        // hourLabel
        hourLabel.Name = "hourLabel";
        hourLabel.Location = new Point(20, 170);
        hourLabel.Size = new Size(140, 24);
        hourLabel.TabIndex = 5;
        hourLabel.Text = "Kezdő óra *";
        // hourBox
        hourBox.Name = "hourBox";
        hourBox.Location = new Point(180, 170);
        hourBox.Size = new Size(355, 28);
        hourBox.TabIndex = 6;
        hourBox.DropDownStyle = ComboBoxStyle.DropDownList;
        // noteLabel
        noteLabel.Name = "noteLabel";
        noteLabel.Location = new Point(20, 220);
        noteLabel.Size = new Size(140, 24);
        noteLabel.TabIndex = 7;
        noteLabel.Text = "Megjegyzés";
        // noteBox
        noteBox.Name = "noteBox";
        noteBox.Location = new Point(180, 220);
        noteBox.Size = new Size(355, 95);
        noteBox.TabIndex = 8;
        noteBox.MaxLength = 1000;
        noteBox.Multiline = true;
        // saveButton
        saveButton.Name = "saveButton";
        saveButton.Location = new Point(245, 340);
        saveButton.Size = new Size(160, 34);
        saveButton.TabIndex = 9;
        saveButton.Text = "Foglalás mentése";
        saveButton.BackColor = Color.FromArgb(15, 118, 110);
        saveButton.ForeColor = Color.White;
        saveButton.FlatStyle = FlatStyle.Flat;
        saveButton.UseVisualStyleBackColor = false;
        saveButton.Click += SaveButton_Click;
        // cancelButton
        cancelButton.Name = "cancelButton";
        cancelButton.Location = new Point(420, 340);
        cancelButton.Size = new Size(115, 34);
        cancelButton.TabIndex = 10;
        cancelButton.Text = "Mégse";
        cancelButton.BackColor = Color.FromArgb(15, 118, 110);
        cancelButton.ForeColor = Color.White;
        cancelButton.FlatStyle = FlatStyle.Flat;
        cancelButton.UseVisualStyleBackColor = false;
        cancelButton.DialogResult = DialogResult.Cancel;
        this.Controls.Add(doctorLabel);
        this.Controls.Add(doctorBox);
        this.Controls.Add(dateLabel);
        this.Controls.Add(datePicker);
        this.Controls.Add(slotsButton);
        this.Controls.Add(hourLabel);
        this.Controls.Add(hourBox);
        this.Controls.Add(noteLabel);
        this.Controls.Add(noteBox);
        this.Controls.Add(saveButton);
        this.Controls.Add(cancelButton);
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(565, 400);
        MinimumSize = new Size(581, 439);
        Font = new Font("Segoe UI", 9F);
        BackColor = Color.FromArgb(241, 247, 246);
        Name = "AppointmentForm";
        Text = "Időpontfoglalás";
        StartPosition = FormStartPosition.CenterScreen;
        CancelButton = cancelButton;
        ResumeLayout(false);
        PerformLayout();
    }
    #endregion
    private Label doctorLabel;
    private ComboBox doctorBox;
    private Label dateLabel;
    private DateTimePicker datePicker;
    private Button slotsButton;
    private Label hourLabel;
    private ComboBox hourBox;
    private Label noteLabel;
    private TextBox noteBox;
    private Button saveButton;
    private Button cancelButton;
}
