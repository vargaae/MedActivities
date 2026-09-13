namespace Desktop_Pacienskezelo;

partial class PatientEditForm
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
        nameBox = new TextBox();
        label1 = new Label();
        tajBox = new TextBox();
        label2 = new Label();
        birthPicker = new DateTimePicker();
        label3 = new Label();
        emailBox = new TextBox();
        label4 = new Label();
        phoneBox = new TextBox();
        label5 = new Label();
        addressBox = new TextBox();
        label6 = new Label();
        notesBox = new TextBox();
        saveButton = new Button();
        cancelButton = new Button();
        SuspendLayout();
        // label0
        label0.Name = "label0";
        label0.Location = new Point(24, 24);
        label0.Size = new Size(160, 24);
        label0.TabIndex = 0;
        label0.Text = "Név *";
        // nameBox
        nameBox.Name = "nameBox";
        nameBox.Location = new Point(195, 24);
        nameBox.Size = new Size(385, 28);
        nameBox.TabIndex = 1;
        nameBox.MaxLength = 100;
        // label1
        label1.Name = "label1";
        label1.Location = new Point(24, 72);
        label1.Size = new Size(160, 24);
        label1.TabIndex = 2;
        label1.Text = "TAJ (9 számjegy) *";
        // tajBox
        tajBox.Name = "tajBox";
        tajBox.Location = new Point(195, 72);
        tajBox.Size = new Size(385, 28);
        tajBox.TabIndex = 3;
        tajBox.MaxLength = 9;
        // label2
        label2.Name = "label2";
        label2.Location = new Point(24, 120);
        label2.Size = new Size(160, 24);
        label2.TabIndex = 4;
        label2.Text = "Születési dátum *";
        // birthPicker
        birthPicker.Name = "birthPicker";
        birthPicker.Location = new Point(195, 120);
        birthPicker.Size = new Size(385, 28);
        birthPicker.TabIndex = 5;
        birthPicker.Format = DateTimePickerFormat.Short;
        // label3
        label3.Name = "label3";
        label3.Location = new Point(24, 168);
        label3.Size = new Size(160, 24);
        label3.TabIndex = 6;
        label3.Text = "E-mail";
        // emailBox
        emailBox.Name = "emailBox";
        emailBox.Location = new Point(195, 168);
        emailBox.Size = new Size(385, 28);
        emailBox.TabIndex = 7;
        emailBox.MaxLength = 200;
        // label4
        label4.Name = "label4";
        label4.Location = new Point(24, 216);
        label4.Size = new Size(160, 24);
        label4.TabIndex = 8;
        label4.Text = "Telefon";
        // phoneBox
        phoneBox.Name = "phoneBox";
        phoneBox.Location = new Point(195, 216);
        phoneBox.Size = new Size(385, 28);
        phoneBox.TabIndex = 9;
        phoneBox.MaxLength = 40;
        // label5
        label5.Name = "label5";
        label5.Location = new Point(24, 264);
        label5.Size = new Size(160, 24);
        label5.TabIndex = 10;
        label5.Text = "Lakcím";
        // addressBox
        addressBox.Name = "addressBox";
        addressBox.Location = new Point(195, 264);
        addressBox.Size = new Size(385, 28);
        addressBox.TabIndex = 11;
        addressBox.MaxLength = 300;
        // label6
        label6.Name = "label6";
        label6.Location = new Point(24, 312);
        label6.Size = new Size(160, 24);
        label6.TabIndex = 12;
        label6.Text = "Általános megjegyzés";
        // notesBox
        notesBox.Name = "notesBox";
        notesBox.Location = new Point(195, 312);
        notesBox.Size = new Size(385, 110);
        notesBox.TabIndex = 13;
        notesBox.MaxLength = 2000;
        notesBox.Multiline = true;
        notesBox.ScrollBars = ScrollBars.Vertical;
        // saveButton
        saveButton.Name = "saveButton";
        saveButton.Location = new Point(360, 448);
        saveButton.Size = new Size(105, 34);
        saveButton.TabIndex = 14;
        saveButton.Text = "Mentés";
        saveButton.BackColor = Color.FromArgb(15, 118, 110);
        saveButton.ForeColor = Color.White;
        saveButton.FlatStyle = FlatStyle.Flat;
        saveButton.UseVisualStyleBackColor = false;
        saveButton.Click += SaveButton_Click;
        // cancelButton
        cancelButton.Name = "cancelButton";
        cancelButton.Location = new Point(475, 448);
        cancelButton.Size = new Size(105, 34);
        cancelButton.TabIndex = 15;
        cancelButton.Text = "Mégse";
        cancelButton.BackColor = Color.FromArgb(15, 118, 110);
        cancelButton.ForeColor = Color.White;
        cancelButton.FlatStyle = FlatStyle.Flat;
        cancelButton.UseVisualStyleBackColor = false;
        cancelButton.DialogResult = DialogResult.Cancel;
        this.Controls.Add(label0);
        this.Controls.Add(nameBox);
        this.Controls.Add(label1);
        this.Controls.Add(tajBox);
        this.Controls.Add(label2);
        this.Controls.Add(birthPicker);
        this.Controls.Add(label3);
        this.Controls.Add(emailBox);
        this.Controls.Add(label4);
        this.Controls.Add(phoneBox);
        this.Controls.Add(label5);
        this.Controls.Add(addressBox);
        this.Controls.Add(label6);
        this.Controls.Add(notesBox);
        this.Controls.Add(saveButton);
        this.Controls.Add(cancelButton);
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(605, 505);
        MinimumSize = new Size(621, 544);
        Font = new Font("Segoe UI", 9F);
        BackColor = Color.FromArgb(241, 247, 246);
        Name = "PatientEditForm";
        Text = "Páciens";
        StartPosition = FormStartPosition.CenterScreen;
        AcceptButton = saveButton;
        CancelButton = cancelButton;
        ResumeLayout(false);
        PerformLayout();
    }
    #endregion
    private Label label0;
    private TextBox nameBox;
    private Label label1;
    private TextBox tajBox;
    private Label label2;
    private DateTimePicker birthPicker;
    private Label label3;
    private TextBox emailBox;
    private Label label4;
    private TextBox phoneBox;
    private Label label5;
    private TextBox addressBox;
    private Label label6;
    private TextBox notesBox;
    private Button saveButton;
    private Button cancelButton;
}
