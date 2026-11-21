using BookStoreManagement.Data;
using BookStoreManagement.Models;
using BookStoreManagement.UI.Components;
using BookStoreManagement.UI.Forms;
using Microsoft.Data.SqlClient;

namespace BookStoreManagement.UI.Pages;

public class CustomersPage : UserControl
{
    private readonly TextBox txtSearch = new TextBox();
    private readonly DataGridView grid = new DataGridView();

    public CustomersPage()
    {
        BackColor = Theme.Background;

        Theme.StyleTextBox(txtSearch);
        txtSearch.PlaceholderText = "Search by name, e-mail or city...";
        txtSearch.Width = 340;
        txtSearch.Margin = new Padding(0, 6, 20, 0);

        Button btnAdd = Theme.CreateButton("+ Add Customer", Theme.Primary, 150);
        Button btnEdit = Theme.CreateButton("Edit", Theme.Info, 100);
        Button btnDelete = Theme.CreateButton("Delete", Theme.Danger, 100);
        btnAdd.Margin = new Padding(0, 0, 10, 0);
        btnEdit.Margin = new Padding(0, 0, 10, 0);

        FlowLayoutPanel toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        toolbar.Controls.Add(txtSearch);
        toolbar.Controls.Add(btnAdd);
        toolbar.Controls.Add(btnEdit);
        toolbar.Controls.Add(btnDelete);

        Theme.StyleGrid(grid);
        grid.Dock = DockStyle.Fill;
        CardPanel card = new CardPanel { Dock = DockStyle.Fill, Padding = new Padding(1) };
        card.Controls.Add(grid);

        TableLayoutPanel layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(28)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(Theme.CreateHeader("Customers", "People who buy books from the store"), 0, 0);
        layout.Controls.Add(toolbar, 0, 1);
        layout.Controls.Add(card, 0, 2);
        Controls.Add(layout);

        txtSearch.TextChanged += (s, e) => LoadData();
        btnAdd.Click += (s, e) => AddCustomer();
        btnEdit.Click += (s, e) => EditCustomer();
        btnDelete.Click += (s, e) => DeleteCustomer();
        grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) EditCustomer(); };
        Load += (s, e) => LoadData();
    }

    private void LoadData()
    {
        try
        {
            grid.DataSource = CustomerRepository.GetAll(txtSearch.Text.Trim());
            grid.Columns["CustomerID"]!.HeaderText = "ID";
            grid.Columns["CustomerID"]!.FillWeight = 30;
            grid.Columns["FirstName"]!.HeaderText = "First name";
            grid.Columns["LastName"]!.HeaderText = "Last name";
            grid.Columns["Email"]!.FillWeight = 140;
        }
        catch (SqlException ex)
        {
            Dialogs.DatabaseError(ex);
        }
    }

    private Customer? GetSelectedCustomer()
    {
        if (grid.CurrentRow == null)
        {
            Dialogs.Info("Please select a customer first.");
            return null;
        }

        DataGridViewRow row = grid.CurrentRow;
        return new Customer
        {
            CustomerID = Convert.ToInt32(row.Cells["CustomerID"].Value),
            FirstName = row.Cells["FirstName"].Value?.ToString() ?? "",
            LastName = row.Cells["LastName"].Value?.ToString() ?? "",
            Email = row.Cells["Email"].Value?.ToString() ?? "",
            Phone = row.Cells["Phone"].Value?.ToString() ?? "",
            City = row.Cells["City"].Value?.ToString() ?? ""
        };
    }

    private void AddCustomer()
    {
        using CustomerEditForm dialog = new CustomerEditForm(null);
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;

        try
        {
            CustomerRepository.Add(dialog.Customer);
            LoadData();
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601) // duplicate e-mail
        {
            Dialogs.Info("A customer with this e-mail already exists.");
        }
        catch (SqlException ex)
        {
            Dialogs.DatabaseError(ex);
        }
    }

    private void EditCustomer()
    {
        Customer? selected = GetSelectedCustomer();
        if (selected == null) return;

        using CustomerEditForm dialog = new CustomerEditForm(selected);
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;

        try
        {
            CustomerRepository.Update(dialog.Customer);
            LoadData();
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            Dialogs.Info("A customer with this e-mail already exists.");
        }
        catch (SqlException ex)
        {
            Dialogs.DatabaseError(ex);
        }
    }

    private void DeleteCustomer()
    {
        Customer? selected = GetSelectedCustomer();
        if (selected == null) return;

        if (!Dialogs.Confirm("Delete customer \"" + selected.FullName + "\"?")) return;

        try
        {
            CustomerRepository.Delete(selected.CustomerID);
            LoadData();
        }
        catch (SqlException ex) when (ex.Number == 547) // foreign key violation
        {
            Dialogs.Info("This customer cannot be deleted because he/she has orders.");
        }
        catch (SqlException ex)
        {
            Dialogs.DatabaseError(ex);
        }
    }
}
