namespace Desktop_Pacienskezelo;

public sealed record DeclarationPolicy(string Version, string Title, string Highlight, string Text, bool RequiresSignature);
public sealed record DeclarationAcceptance(bool Accepted, string Version, string SignedName);
public sealed class DeclarationRequiredException(DeclarationPolicy policy) : InvalidOperationException("A nyilatkozat elfogadása szükséges.")
{
    public DeclarationPolicy Policy { get; } = policy;
}

public sealed class DeclarationDialog : Form
{
    public DeclarationAcceptance? Acceptance { get; private set; }
    public DeclarationDialog(DeclarationPolicy policy)
    {
        Text = policy.Title;
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(760, 640);
        MinimumSize = new Size(600, 500);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6, Padding = new Padding(16) };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        for (var i = 1; i < 6; i++) layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var text = new RichTextBox
        {
            ReadOnly = true,
            Dock = DockStyle.Fill,
            DetectUrls = false,
            Text = policy.Highlight + "\n\n" + policy.Text + "\n\nVerzió: " + policy.Version +
                "\n\nEzzel elfogadja a GDPR szerinti adatkezelési feltételeket, és hozzájárul " +
                "személyes és egészségügyi adatai EgészségÚt alkalmazásban történő korlátozott kezeléséhez."
        };
        text.Select(0, policy.Highlight.Length);
        using (var bold = new Font(text.Font, FontStyle.Bold)) text.SelectionFont = bold;
        text.Select(0, 0);
        var nameLabel = new Label { Text = "Nyilatkozattevő teljes neve", AutoSize = true, Visible = policy.RequiresSignature };
        var name = new TextBox { Dock = DockStyle.Top, MaxLength = 100, Visible = policy.RequiresSignature, AccessibleName = "Nyilatkozattevő teljes neve" };
        var accept = new CheckBox { AutoSize = true, Text = "Elolvastam a nyilatkozatot, és személyesen elfogadom a benne foglaltakat." };
        var hint = new Label { AutoSize = true, Text = "Naplózott elfogadás, nem minősített elektronikus aláírás." };
        var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        var ok = new Button { Text = "Elfogadás", AutoSize = true, Enabled = false };
        var cancel = new Button { Text = "Mégsem", AutoSize = true, DialogResult = DialogResult.Cancel };
        void ValidateAcceptance() => ok.Enabled = accept.Checked && (!policy.RequiresSignature || name.Text.Trim().Length >= 3);
        name.TextChanged += (_, _) => ValidateAcceptance();
        accept.CheckedChanged += (_, _) => ValidateAcceptance();
        ok.Click += (_, _) => { Acceptance = new(true, policy.Version, name.Text.Trim()); DialogResult = DialogResult.OK; };
        buttons.Controls.Add(ok); buttons.Controls.Add(cancel);
        layout.Controls.Add(text, 0, 0); layout.Controls.Add(nameLabel, 0, 1); layout.Controls.Add(name, 0, 2);
        layout.Controls.Add(accept, 0, 3); layout.Controls.Add(hint, 0, 4); layout.Controls.Add(buttons, 0, 5);
        Controls.Add(layout); AcceptButton = ok; CancelButton = cancel;
    }
}
