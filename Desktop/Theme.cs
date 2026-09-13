namespace MedActivities.Patient.Sqlite.WinForms;

internal static class Theme
{
    public static readonly Color Ink = Color.FromArgb(19, 40, 66);
    public static readonly Color Blue = Color.FromArgb(35, 86, 207);
    public static readonly Color BlueHover = Color.FromArgb(25, 69, 177);
    public static readonly Color Muted = Color.FromArgb(98, 113, 135);
    public static readonly Color Line = Color.FromArgb(227, 234, 241);
    public static readonly Color Page = Color.FromArgb(245, 248, 251);
    public static readonly Color SoftBlue = Color.FromArgb(238, 244, 252);
    public static readonly Color White = Color.White;

    public static Font Regular(float size = 9.5f) => new("Segoe UI", size, FontStyle.Regular);
    public static Font Medium(float size = 9.5f) => new("Segoe UI Semibold", size, FontStyle.Bold);

    public static void Apply(Form form)
    {
        form.BackColor = Page;
        form.ForeColor = Ink;
        form.Font = Regular();
    }

    public static void StyleButton(Button button, bool primary = false)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = primary ? 0 : 1;
        button.FlatAppearance.BorderColor = primary ? Blue : Line;
        button.BackColor = primary ? Blue : White;
        button.ForeColor = primary ? White : Ink;
        button.Font = Medium(9.25f);
        button.Cursor = Cursors.Hand;
        button.Padding = new Padding(12, 0, 12, 0);
        button.MinimumSize = new Size(0, 38);
        button.FlatAppearance.MouseOverBackColor = primary ? BlueHover : SoftBlue;
        button.FlatAppearance.MouseDownBackColor = primary ? BlueHover : Color.FromArgb(225, 235, 247);
    }

    public static void StyleTextBox(TextBox textBox)
    {
        textBox.BackColor = White;
        textBox.ForeColor = Ink;
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.Font = Regular();
        textBox.Margin = new Padding(0, 3, 0, 3);
    }

    public static void StyleGrid(DataGridView grid)
    {
        grid.BackgroundColor = White;
        grid.BorderStyle = BorderStyle.None;
        grid.GridColor = Line;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.RowHeadersVisible = false;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = SoftBlue,
            ForeColor = Ink,
            Font = Medium(8.75f),
            Padding = new Padding(8, 0, 8, 0),
            SelectionBackColor = SoftBlue,
            SelectionForeColor = Ink
        };
        grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = White,
            ForeColor = Ink,
            Font = Regular(9f),
            SelectionBackColor = Color.FromArgb(224, 235, 255),
            SelectionForeColor = Ink,
            Padding = new Padding(8, 6, 8, 6)
        };
        grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.FromArgb(250, 252, 254),
            ForeColor = Ink,
            SelectionBackColor = Color.FromArgb(224, 235, 255),
            SelectionForeColor = Ink
        };
        grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
        grid.RowTemplate.Height = 38;
    }

    public static Panel Surface() => new()
    {
        BackColor = White,
        Padding = new Padding(18),
        Margin = new Padding(8)
    };
}
