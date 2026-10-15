namespace BookStoreManagement;

// Main menu. The Orders button will be added in the next step.
public class MainForm : Form
{
    public MainForm()
    {
        Text = "BookStoreManagement - Main Menu";
        Width = 700;
        Height = 450;
        StartPosition = FormStartPosition.CenterScreen;

        var lblWelcome = new Label
        {
            Text = $"Welcome, {UserSession.Username}  (Role: {UserSession.Role})",
            Font = new Font("Segoe UI", 14),
            AutoSize = true,
            Location = new Point(30, 30)
        };

        var btnCustomers = MakeMenuButton("Customers", 30, 90);
        btnCustomers.Click += (s, e) => new CustomerForm().ShowDialog();

        var btnProducts = MakeMenuButton("Products", 250, 90);
        btnProducts.Click += (s, e) => new ProductForm().ShowDialog();

        Controls.Add(lblWelcome);
        Controls.Add(btnCustomers);
        Controls.Add(btnProducts);
    }

    private static Button MakeMenuButton(string text, int x, int y)
    {
        return new Button
        {
            Text = text,
            Font = new Font("Segoe UI", 11),
            Size = new Size(200, 50),
            Location = new Point(x, y)
        };
    }
}