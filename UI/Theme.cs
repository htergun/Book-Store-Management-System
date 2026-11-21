namespace BookStoreManagement.UI;

// Colors, fonts and small helper methods that give every page the same modern look.
public static class Theme
{
    // colors
    public static readonly Color Sidebar = ColorTranslator.FromHtml("#1e293b");
    public static readonly Color SidebarHover = ColorTranslator.FromHtml("#334155");
    public static readonly Color SidebarText = ColorTranslator.FromHtml("#cbd5e1");
    public static readonly Color Background = ColorTranslator.FromHtml("#f1f5f9");
    public static readonly Color Border = ColorTranslator.FromHtml("#e2e8f0");
    public static readonly Color Ink = ColorTranslator.FromHtml("#0f172a");
    public static readonly Color Muted = ColorTranslator.FromHtml("#64748b");
    public static readonly Color Primary = ColorTranslator.FromHtml("#4f46e5");
    public static readonly Color Success = ColorTranslator.FromHtml("#10b981");
    public static readonly Color Danger = ColorTranslator.FromHtml("#ef4444");
    public static readonly Color Warning = ColorTranslator.FromHtml("#f59e0b");
    public static readonly Color Info = ColorTranslator.FromHtml("#0ea5e9");
    public static readonly Color Gray = ColorTranslator.FromHtml("#64748b");

    // fonts
    public static readonly Font FontRegular = new Font("Segoe UI", 10F);
    public static readonly Font FontBold = new Font("Segoe UI Semibold", 10F);
    public static readonly Font FontTitle = new Font("Segoe UI Semibold", 22F);
    public static readonly Font FontBig = new Font("Segoe UI Semibold", 24F);

    public static Color Darken(Color c, int amount = 20)
    {
        return Color.FromArgb(Math.Max(0, c.R - amount), Math.Max(0, c.G - amount), Math.Max(0, c.B - amount));
    }

    // flat colored button with hover effect
    public static Button CreateButton(string text, Color back, int width = 120)
    {
        Button b = new Button
        {
            Text = text,
            Width = width,
            Height = 38,
            FlatStyle = FlatStyle.Flat,
            BackColor = back,
            ForeColor = Color.White,
            Font = FontBold,
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        b.FlatAppearance.BorderSize = 0;
        b.FlatAppearance.MouseOverBackColor = Darken(back);
        b.FlatAppearance.MouseDownBackColor = Darken(back, 40);
        return b;
    }

    // page title + subtitle
    public static Panel CreateHeader(string title, string subtitle)
    {
        Panel p = new Panel { Dock = DockStyle.Fill };
        p.Controls.Add(new Label
        {
            Text = subtitle,
            Font = FontRegular,
            ForeColor = Muted,
            Location = new Point(2, 46),
            AutoSize = true
        });
        p.Controls.Add(new Label
        {
            Text = title,
            Font = FontTitle,
            ForeColor = Ink,
            Location = new Point(0, 0),
            AutoSize = true
        });
        return p;
    }

    // small label above a control (used in forms)
    public static Panel CreateField(string label, Control control, int width)
    {
        Panel p = new Panel { Width = width, Height = 66, Margin = new Padding(0, 0, 16, 0) };
        p.Controls.Add(new Label
        {
            Text = label,
            Font = FontRegular,
            ForeColor = Muted,
            Location = new Point(0, 0),
            AutoSize = true
        });
        control.Location = new Point(0, 24);
        control.Width = width;
        p.Controls.Add(control);
        return p;
    }

    public static void StyleTextBox(TextBox t)
    {
        t.Font = FontRegular;
        t.BorderStyle = BorderStyle.FixedSingle;
    }

    public static void StyleComboBox(ComboBox c)
    {
        c.Font = FontRegular;
        c.DropDownStyle = ComboBoxStyle.DropDownList;
        c.FlatStyle = FlatStyle.Flat;
    }

    // modern flat table
    public static void StyleGrid(DataGridView g)
    {
        g.BorderStyle = BorderStyle.None;
        g.BackgroundColor = Color.White;
        g.GridColor = Border;
        g.EnableHeadersVisualStyles = false;
        g.ReadOnly = true;
        g.AllowUserToAddRows = false;
        g.AllowUserToDeleteRows = false;
        g.AllowUserToResizeRows = false;
        g.MultiSelect = false;
        g.RowHeadersVisible = false;
        g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        g.ColumnHeadersHeight = 42;
        g.RowTemplate.Height = 38;
        g.Font = FontRegular;

        Color headerBack = ColorTranslator.FromHtml("#f8fafc");
        g.ColumnHeadersDefaultCellStyle.BackColor = headerBack;
        g.ColumnHeadersDefaultCellStyle.ForeColor = Muted;
        g.ColumnHeadersDefaultCellStyle.Font = FontBold;
        g.ColumnHeadersDefaultCellStyle.SelectionBackColor = headerBack;
        g.ColumnHeadersDefaultCellStyle.SelectionForeColor = Muted;
        g.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);

        g.DefaultCellStyle.BackColor = Color.White;
        g.DefaultCellStyle.ForeColor = Ink;
        g.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#e0e7ff");
        g.DefaultCellStyle.SelectionForeColor = Ink;
        g.DefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
    }
}
