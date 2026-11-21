using System.ComponentModel;
using BookStoreManagement.Models;

namespace BookStoreManagement.UI.Forms;

// Dialog used for both "Add Customer" and "Edit Customer".
public class CustomerEditForm : Form
{
    private readonly TextBox txtFirst = new TextBox();
    private readonly TextBox txtLast = new TextBox();
    private readonly TextBox txtEmail = new TextBox();
    private readonly TextBox txtPhone = new TextBox();
    private readonly TextBox txtCity = new TextBox();
    private readonly Customer customer;

    // the customer that was filled in by the user (read it when DialogResult is OK)
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Customer Customer => customer;

    public CustomerEditForm(Customer? existing)
    {
        customer = existing ?? new Customer();

        Text = existing == null ? "Add Customer" : "Edit Customer";
        ClientSize = new Size(500, 370);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.White;
        Font = Theme.FontRegular;

        foreach (TextBox t in new[] { txtFirst, txtLast, txtEmail, txtPhone, txtCity })
            Theme.StyleTextBox(t);

        txtFirst.MaxLength = 50;
        txtLast.MaxLength = 50;
        txtEmail.MaxLength = 100;
        txtPhone.MaxLength = 20;
        txtCity.MaxLength = 50;

        txtFirst.Text = customer.FirstName;
        txtLast.Text = customer.LastName;
        txtEmail.Text = customer.Email;
        txtPhone.Text = customer.Phone;
        txtCity.Text = customer.City;

        // ---- layout ----
        TableLayoutPanel table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Padding = new Padding(28, 24, 28, 0)
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddRow(table, 0, "First name", txtFirst);
        AddRow(table, 1, "Last name", txtLast);
        AddRow(table, 2, "Email", txtEmail);
        AddRow(table, 3, "Phone", txtPhone);
        AddRow(table, 4, "City", txtCity);

        Button btnSave = Theme.CreateButton("Save", Theme.Primary, 110);
        Button btnCancel = Theme.CreateButton("Cancel", Theme.Gray, 110);
        btnSave.Click += (s, e) => Save();
        btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

        FlowLayoutPanel buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 70,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(28, 14, 28, 0)
        };
        buttons.Controls.Add(btnSave);
        buttons.Controls.Add(btnCancel);

        Controls.Add(table);
        Controls.Add(buttons);
    }

    private static void AddRow(TableLayoutPanel table, int row, string label, Control input)
    {
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        table.RowCount = row + 1;

        table.Controls.Add(new Label
        {
            Text = label,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Theme.Muted
        }, 0, row);

        input.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        table.Controls.Add(input, 1, row);
    }

    private void Save()
    {
        if (string.IsNullOrWhiteSpace(txtFirst.Text) || string.IsNullOrWhiteSpace(txtLast.Text))
        {
            Dialogs.Info("First name and last name are required.");
            return;
        }

        if (!txtEmail.Text.Contains('@'))
        {
            Dialogs.Info("Please enter a valid e-mail address.");
            return;
        }

        customer.FirstName = txtFirst.Text.Trim();
        customer.LastName = txtLast.Text.Trim();
        customer.Email = txtEmail.Text.Trim();
        customer.Phone = txtPhone.Text.Trim();
        customer.City = txtCity.Text.Trim();

        DialogResult = DialogResult.OK;
    }
}
