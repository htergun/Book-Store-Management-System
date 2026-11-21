using System.ComponentModel;
namespace BookStoreManagement.UI.Components;

// Dashboard card: small title, big number and a colored strip on the left.
public class StatCard : CardPanel
{
    private readonly Label lblValue = new Label();

    public StatCard(string title, Color accent)
    {
        Dock = DockStyle.Fill;

        Panel strip = new Panel { Dock = DockStyle.Left, Width = 6, BackColor = accent };

        Label lblTitle = new Label
        {
            Text = title,
            Font = Theme.FontRegular,
            ForeColor = Theme.Muted,
            Location = new Point(24, 18),
            AutoSize = true
        };

        lblValue.Text = "-";
        lblValue.Font = Theme.FontBig;
        lblValue.ForeColor = Theme.Ink;
        lblValue.Location = new Point(22, 46);
        lblValue.AutoSize = true;

        Controls.Add(lblTitle);
        Controls.Add(lblValue);
        Controls.Add(strip);
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Value
    {
        get => lblValue.Text;
        set => lblValue.Text = value;
    }
}
