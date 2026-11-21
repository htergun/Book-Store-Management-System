using BookStoreManagement.UI.Pages;

namespace BookStoreManagement.UI;

// Main window: dark sidebar on the left, the selected page on the right.
public class MainForm : Form
{
    private readonly Panel contentPanel = new Panel();
    private readonly List<Button> navButtons = new List<Button>();

    public MainForm()
    {
        Text = "BookStoreManagement";
        ClientSize = new Size(1280, 780);
        MinimumSize = new Size(1180, 720);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Theme.Background;
        Font = Theme.FontRegular;

        // ---- sidebar ----
        Panel sidebar = new Panel { Dock = DockStyle.Left, Width = 240, BackColor = Theme.Sidebar };

        Label logo = new Label
        {
            Text = "BookStore",
            Dock = DockStyle.Top,
            Height = 90,
            Font = new Font("Segoe UI Semibold", 18F),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(26, 0, 0, 0)
        };

        Label footer = new Label
        {
            Text = "BookStoreDB  |  SQL Server",
            Dock = DockStyle.Bottom,
            Height = 50,
            Font = new Font("Segoe UI", 9F),
            ForeColor = Theme.Muted,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(26, 0, 0, 0)
        };

        FlowLayoutPanel nav = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Theme.Sidebar
        };
        nav.Controls.Add(CreateNavButton("Dashboard", () => new DashboardPage()));
        nav.Controls.Add(CreateNavButton("Books", () => new BooksPage()));
        nav.Controls.Add(CreateNavButton("Customers", () => new CustomersPage()));
        nav.Controls.Add(CreateNavButton("Orders", () => new OrdersPage()));

        // the control that is added first is docked last
        sidebar.Controls.Add(nav);
        sidebar.Controls.Add(footer);
        sidebar.Controls.Add(logo);

        // ---- content ----
        contentPanel.Dock = DockStyle.Fill;
        contentPanel.BackColor = Theme.Background;

        Controls.Add(contentPanel);
        Controls.Add(sidebar);

        ShowPage(new DashboardPage(), navButtons[0]);
    }

    private Button CreateNavButton(string text, Func<UserControl> pageFactory)
    {
        Button b = new Button
        {
            Text = text,
            Width = 240,
            Height = 48,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Theme.SidebarText,
            BackColor = Theme.Sidebar,
            Font = new Font("Segoe UI", 11F),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(26, 0, 0, 0),
            Margin = new Padding(0),
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        b.FlatAppearance.BorderSize = 0;
        b.FlatAppearance.MouseOverBackColor = Theme.SidebarHover;
        b.Click += (s, e) => ShowPage(pageFactory(), b);

        navButtons.Add(b);
        return b;
    }

    private void ShowPage(UserControl page, Button active)
    {
        // remove and dispose the old page
        foreach (Control old in contentPanel.Controls.OfType<UserControl>().ToList())
        {
            contentPanel.Controls.Remove(old);
            old.Dispose();
        }

        page.Dock = DockStyle.Fill;
        contentPanel.Controls.Add(page);

        // highlight the active menu button
        foreach (Button b in navButtons)
        {
            bool isActive = b == active;
            b.BackColor = isActive ? Theme.Primary : Theme.Sidebar;
            b.ForeColor = isActive ? Color.White : Theme.SidebarText;
        }
    }
}
