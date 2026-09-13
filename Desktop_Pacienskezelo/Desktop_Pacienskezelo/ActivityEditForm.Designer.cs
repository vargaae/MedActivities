namespace Desktop_Pacienskezelo;

partial class ActivityEditForm
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
        label0 = new Label();
        titleBox = new TextBox();
        label1 = new Label();
        datePicker = new DateTimePicker();
        label2 = new Label();
        patientBox = new ComboBox();
        label3 = new Label();
        categoryBox = new TextBox();
        label4 = new Label();
        cityBox = new TextBox();
        label5 = new Label();
        venueBox = new TextBox();
        label6 = new Label();
        statusBox = new ComboBox();
        label7 = new Label();
        descriptionBox = new TextBox();
        doctorLabel = new Label();
        doctorList = new CheckedListBox();
        saveButton = new Button();
        cancelButton = new Button();
        SuspendLayout();
        // label0
        label0.Name = "label0";
        label0.Location = new Point(20, 20);
        label0.Size = new Size(155, 24);
        label0.TabIndex = 0;
        label0.Text = "Esemény címe *";
        // titleBox
        titleBox.Name = "titleBox";
        titleBox.Location = new Point(185, 20);
        titleBox.Size = new Size(390, 28);
        titleBox.TabIndex = 1;
        titleBox.MaxLength = 200;
        // label1
        label1.Name = "label1";
        label1.Location = new Point(20, 66);
        label1.Size = new Size(155, 24);
        label1.TabIndex = 2;
        label1.Text = "Dátum és idő *";
        // datePicker
        datePicker.Name = "datePicker";
        datePicker.Location = new Point(185, 66);
        datePicker.Size = new Size(390, 28);
        datePicker.TabIndex = 3;
        datePicker.Format = DateTimePickerFormat.Custom;
        datePicker.CustomFormat = "yyyy.MM.dd. HH:mm";
        // label2
        label2.Name = "label2";
        label2.Location = new Point(20, 112);
        label2.Size = new Size(155, 24);
        label2.TabIndex = 4;
        label2.Text = "Páciens *";
        // patientBox
        patientBox.Name = "patientBox";
        patientBox.Location = new Point(185, 112);
        patientBox.Size = new Size(390, 28);
        patientBox.TabIndex = 5;
        patientBox.DropDownStyle = ComboBoxStyle.DropDownList;
        // label3
        label3.Name = "label3";
        label3.Location = new Point(20, 158);
        label3.Size = new Size(155, 24);
        label3.TabIndex = 6;
        label3.Text = "Kategória *";
        // categoryBox
        categoryBox.Name = "categoryBox";
        categoryBox.Location = new Point(185, 158);
        categoryBox.Size = new Size(390, 28);
        categoryBox.TabIndex = 7;
        categoryBox.MaxLength = 100;
        // label4
        label4.Name = "label4";
        label4.Location = new Point(20, 204);
        label4.Size = new Size(155, 24);
        label4.TabIndex = 8;
        label4.Text = "Város";
        // cityBox
        cityBox.Name = "cityBox";
        cityBox.Location = new Point(185, 204);
        cityBox.Size = new Size(390, 28);
        cityBox.TabIndex = 9;
        cityBox.MaxLength = 200;
        // label5
        label5.Name = "label5";
        label5.Location = new Point(20, 250);
        label5.Size = new Size(155, 24);
        label5.TabIndex = 10;
        label5.Text = "Helyszín *";
        // venueBox
        venueBox.Name = "venueBox";
        venueBox.Location = new Point(185, 250);
        venueBox.Size = new Size(390, 28);
        venueBox.TabIndex = 11;
        venueBox.MaxLength = 300;
        // label6
        label6.Name = "label6";
        label6.Location = new Point(20, 296);
        label6.Size = new Size(155, 24);
        label6.TabIndex = 12;
        label6.Text = "Állapot";
        // statusBox
        statusBox.Name = "statusBox";
        statusBox.Location = new Point(185, 296);
        statusBox.Size = new Size(390, 28);
        statusBox.TabIndex = 13;
        statusBox.DropDownStyle = ComboBoxStyle.DropDownList;
        statusBox.Items.AddRange(new object[] { "Scheduled", "Cancelled", "Completed", "NoShow" });
        // label7
        label7.Name = "label7";
        label7.Location = new Point(20, 342);
        label7.Size = new Size(155, 24);
        label7.TabIndex = 14;
        label7.Text = "Leírás *";
        // descriptionBox
        descriptionBox.Name = "descriptionBox";
        descriptionBox.Location = new Point(185, 342);
        descriptionBox.Size = new Size(390, 110);
        descriptionBox.TabIndex = 15;
        descriptionBox.MaxLength = 5000;
        descriptionBox.Multiline = true;
        descriptionBox.ScrollBars = ScrollBars.Vertical;
        // doctorLabel
        doctorLabel.Name = "doctorLabel";
        doctorLabel.Location = new Point(600, 20);
        doctorLabel.Size = new Size(220, 24);
        doctorLabel.TabIndex = 16;
        doctorLabel.Text = "Kezelőorvosok *";
        // doctorList
        doctorList.Name = "doctorList";
        doctorList.Location = new Point(600, 55);
        doctorList.Size = new Size(235, 370);
        doctorList.TabIndex = 17;
        doctorList.CheckOnClick = true;
        // saveButton
        saveButton.Name = "saveButton";
        saveButton.Location = new Point(600, 465);
        saveButton.Size = new Size(110, 34);
        saveButton.TabIndex = 18;
        saveButton.Text = "Mentés";
        saveButton.BackColor = Color.FromArgb(15, 118, 110);
        saveButton.ForeColor = Color.White;
        saveButton.FlatStyle = FlatStyle.Flat;
        saveButton.UseVisualStyleBackColor = false;
        saveButton.Click += SaveButton_Click;
        // cancelButton
        cancelButton.Name = "cancelButton";
        cancelButton.Location = new Point(725, 465);
        cancelButton.Size = new Size(110, 34);
        cancelButton.TabIndex = 19;
        cancelButton.Text = "Mégse";
        cancelButton.BackColor = Color.FromArgb(15, 118, 110);
        cancelButton.ForeColor = Color.White;
        cancelButton.FlatStyle = FlatStyle.Flat;
        cancelButton.UseVisualStyleBackColor = false;
        cancelButton.DialogResult = DialogResult.Cancel;
        this.Controls.Add(label0);
        this.Controls.Add(titleBox);
        this.Controls.Add(label1);
        this.Controls.Add(datePicker);
        this.Controls.Add(label2);
        this.Controls.Add(patientBox);
        this.Controls.Add(label3);
        this.Controls.Add(categoryBox);
        this.Controls.Add(label4);
        this.Controls.Add(cityBox);
        this.Controls.Add(label5);
        this.Controls.Add(venueBox);
        this.Controls.Add(label6);
        this.Controls.Add(statusBox);
        this.Controls.Add(label7);
        this.Controls.Add(descriptionBox);
        this.Controls.Add(doctorLabel);
        this.Controls.Add(doctorList);
        this.Controls.Add(saveButton);
        this.Controls.Add(cancelButton);
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(860, 525);
        MinimumSize = new Size(876, 564);
        Font = new Font("Segoe UI", 9F);
        BackColor = Color.FromArgb(241, 247, 246);
        Name = "ActivityEditForm";
        Text = "Egészségügyi esemény";
        StartPosition = FormStartPosition.CenterScreen;
        AcceptButton = saveButton;
        CancelButton = cancelButton;
        ResumeLayout(false);
        PerformLayout();
    }
    #endregion
    private Label label0;
    private TextBox titleBox;
    private Label label1;
    private DateTimePicker datePicker;
    private Label label2;
    private ComboBox patientBox;
    private Label label3;
    private TextBox categoryBox;
    private Label label4;
    private TextBox cityBox;
    private Label label5;
    private TextBox venueBox;
    private Label label6;
    private ComboBox statusBox;
    private Label label7;
    private TextBox descriptionBox;
    private Label doctorLabel;
    private CheckedListBox doctorList;
    private Button saveButton;
    private Button cancelButton;
}
