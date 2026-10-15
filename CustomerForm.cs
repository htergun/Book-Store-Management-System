using System.Data;
using Microsoft.Data.SqlClient;

namespace BookStoreManagement;

public class CustomerForm : Form
{
    private readonly TextBox txtSearch = new();
    private readonly DataGridView dgvCustomers = new();
    private readonly TextBox txtFirstName = new();
    private readonly TextBox txtLastName = new();
    private readonly TextBox txtCity = new();
    private readonly TextBox txtEmail = new();
    private readonly Button btnAdd = new();
    private readonly Button btnUpdate = new();
    private readonly Button btnDelete = new();
    private readonly Button btnClear = new();

    private int selectedCustomerId = 0;   // 0 = no customer selected

    public CustomerForm()
    {
        BuildUi();
        LoadCustomers();
    }

    // ---------- UI ----------
    private void BuildUi()
    {
        Text = "Customers";
        Width = 900;
        Height = 600;
        StartPosition = FormStartPosition.CenterScreen;

        // Search panel (top)
        var topPanel = new Panel { Dock = DockStyle.Fill };
        txtSearch.Location = new Point(70, 14);
        txtSearch.Width = 220;
        txtSearch.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { LoadCustomers(txtSearch.Text.Trim()); e.SuppressKeyPress = true; }
        };
        var btnSearch = new Button { Text = "Search", Location = new Point(300, 12), Size = new Size(90, 28) };
        btnSearch.Click += (s, e) => LoadCustomers(txtSearch.Text.Trim());
        var btnShowAll = new Button { Text = "Show All", Location = new Point(400, 12), Size = new Size(90, 28) };
        btnShowAll.Click += (s, e) => { txtSearch.Clear(); LoadCustomers(); };
        topPanel.Controls.AddRange(new Control[]
        {
            MakeLabel("Search:", 15, 18), txtSearch, btnSearch, btnShowAll
        });

        // Grid (middle)
        dgvCustomers.Dock = DockStyle.Fill;
        dgvCustomers.ReadOnly = true;
        dgvCustomers.AllowUserToAddRows = false;
        dgvCustomers.MultiSelect = false;
        dgvCustomers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvCustomers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvCustomers.SelectionChanged += (s, e) => FillInputsFromSelection();

        // Input panel (bottom)
        var bottomPanel = new Panel { Dock = DockStyle.Fill };
        txtFirstName.Location = new Point(15, 32); txtFirstName.Width = 160;
        txtLastName.Location = new Point(190, 32); txtLastName.Width = 160;
        txtCity.Location = new Point(365, 32); txtCity.Width = 140;
        txtEmail.Location = new Point(520, 32); txtEmail.Width = 220;

        btnAdd.Text = "Add"; btnAdd.Location = new Point(15, 80); btnAdd.Size = new Size(100, 32);
        btnUpdate.Text = "Update"; btnUpdate.Location = new Point(125, 80); btnUpdate.Size = new Size(100, 32);
        btnDelete.Text = "Delete"; btnDelete.Location = new Point(235, 80); btnDelete.Size = new Size(100, 32);
        btnClear.Text = "Clear"; btnClear.Location = new Point(345, 80); btnClear.Size = new Size(100, 32);

        btnAdd.Click += (s, e) => AddCustomer();
        btnUpdate.Click += (s, e) => UpdateCustomer();
        btnDelete.Click += (s, e) => DeleteCustomer();
        btnClear.Click += (s, e) => ClearInputs();

        // Role-based permission: only admins can delete
        btnDelete.Enabled = UserSession.IsAdmin;
        var lblRoleNote = new Label
        {
            Text = "Only admins can delete customers.",
            ForeColor = Color.Firebrick,
            AutoSize = true,
            Location = new Point(460, 88),
            Visible = !UserSession.IsAdmin
        };

        bottomPanel.Controls.AddRange(new Control[]
        {
            MakeLabel("First name", 15, 12), txtFirstName,
            MakeLabel("Last name", 190, 12), txtLastName,
            MakeLabel("City", 365, 12), txtCity,
            MakeLabel("Email", 520, 12), txtEmail,
            btnAdd, btnUpdate, btnDelete, btnClear, lblRoleNote
        });

        // Layout: 3 rows (search / grid / inputs)
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 130));
        layout.Controls.Add(topPanel, 0, 0);
        layout.Controls.Add(dgvCustomers, 0, 1);
        layout.Controls.Add(bottomPanel, 0, 2);
        Controls.Add(layout);
    }

    private static Label MakeLabel(string text, int x, int y)
    {
        return new Label { Text = text, AutoSize = true, Location = new Point(x, y) };
    }

    // ---------- DATA ----------
    private void LoadCustomers(string search = "")
    {
        try
        {
            using var connection = DbHelper.GetConnection();

            // Parameterized LIKE search (no string concatenation inside the SQL)
            const string sql =
                "SELECT customer_id AS ID, first_name AS [First Name], last_name AS [Last Name], " +
                "city AS City, email AS Email, registration_date AS Registered " +
                "FROM Customers " +
                "WHERE first_name LIKE @s OR last_name LIKE @s OR city LIKE @s " +
                "ORDER BY customer_id";

            using var adapter = new SqlDataAdapter(sql, connection);
            adapter.SelectCommand.Parameters.Add("@s", SqlDbType.NVarChar, 100).Value = "%" + search + "%";

            var table = new DataTable();
            adapter.Fill(table);
            dgvCustomers.DataSource = table;

            if (dgvCustomers.Columns.Count > 5)
                dgvCustomers.Columns[5].DefaultCellStyle.Format = "yyyy-MM-dd";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Could not load customers: " + ex.Message, "Error");
        }
    }

    private void FillInputsFromSelection()
    {
        if (dgvCustomers.CurrentRow == null)
        {
            selectedCustomerId = 0;
            return;
        }

        var cells = dgvCustomers.CurrentRow.Cells;
        selectedCustomerId = Convert.ToInt32(cells[0].Value);
        txtFirstName.Text = cells[1].Value?.ToString() ?? "";
        txtLastName.Text = cells[2].Value?.ToString() ?? "";
        txtCity.Text = cells[3].Value?.ToString() ?? "";
        txtEmail.Text = cells[4].Value?.ToString() ?? "";
    }

    private void ClearInputs()
    {
        selectedCustomerId = 0;
        txtFirstName.Clear();
        txtLastName.Clear();
        txtCity.Clear();
        txtEmail.Clear();
        dgvCustomers.ClearSelection();
    }

    // Empty text becomes NULL in the database
    private static object ToDbValue(string text)
    {
        return string.IsNullOrWhiteSpace(text) ? DBNull.Value : text.Trim();
    }

    private bool InputsAreValid()
    {
        if (string.IsNullOrWhiteSpace(txtFirstName.Text) || string.IsNullOrWhiteSpace(txtLastName.Text))
        {
            MessageBox.Show("First name and last name are required.", "Missing data");
            return false;
        }
        return true;
    }

    // ---------- ADD / UPDATE / DELETE ----------
    private void AddCustomer()
    {
        if (!InputsAreValid()) return;

        try
        {
            using var connection = DbHelper.GetConnection();
            connection.Open();

            const string sql =
                "INSERT INTO Customers (first_name, last_name, city, email, registration_date) " +
                "VALUES (@fn, @ln, @city, @email, @date)";

            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.Add("@fn", SqlDbType.NVarChar, 50).Value = txtFirstName.Text.Trim();
            cmd.Parameters.Add("@ln", SqlDbType.NVarChar, 50).Value = txtLastName.Text.Trim();
            cmd.Parameters.Add("@city", SqlDbType.NVarChar, 50).Value = ToDbValue(txtCity.Text);
            cmd.Parameters.Add("@email", SqlDbType.NVarChar, 100).Value = ToDbValue(txtEmail.Text);
            cmd.Parameters.Add("@date", SqlDbType.Date).Value = DateTime.Today;
            cmd.ExecuteNonQuery();

            ClearInputs();
            LoadCustomers(txtSearch.Text.Trim());
            MessageBox.Show("Customer added.", "Success");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Could not add customer: " + ex.Message, "Error");
        }
    }

    private void UpdateCustomer()
    {
        if (selectedCustomerId == 0)
        {
            MessageBox.Show("Select a customer from the list first.", "No selection");
            return;
        }
        if (!InputsAreValid()) return;

        try
        {
            using var connection = DbHelper.GetConnection();
            connection.Open();

            const string sql =
                "UPDATE Customers SET first_name = @fn, last_name = @ln, city = @city, email = @email " +
                "WHERE customer_id = @id";

            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.Add("@fn", SqlDbType.NVarChar, 50).Value = txtFirstName.Text.Trim();
            cmd.Parameters.Add("@ln", SqlDbType.NVarChar, 50).Value = txtLastName.Text.Trim();
            cmd.Parameters.Add("@city", SqlDbType.NVarChar, 50).Value = ToDbValue(txtCity.Text);
            cmd.Parameters.Add("@email", SqlDbType.NVarChar, 100).Value = ToDbValue(txtEmail.Text);
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = selectedCustomerId;
            cmd.ExecuteNonQuery();

            LoadCustomers(txtSearch.Text.Trim());
            MessageBox.Show("Customer updated.", "Success");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Could not update customer: " + ex.Message, "Error");
        }
    }

    private void DeleteCustomer()
    {
        if (!UserSession.IsAdmin) return;   // double check on the code side too

        if (selectedCustomerId == 0)
        {
            MessageBox.Show("Select a customer from the list first.", "No selection");
            return;
        }

        var answer = MessageBox.Show("Delete this customer?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (answer != DialogResult.Yes) return;

        try
        {
            using var connection = DbHelper.GetConnection();
            connection.Open();

            using var cmd = new SqlCommand("DELETE FROM Customers WHERE customer_id = @id", connection);
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = selectedCustomerId;
            cmd.ExecuteNonQuery();

            ClearInputs();
            LoadCustomers(txtSearch.Text.Trim());
            MessageBox.Show("Customer deleted.", "Success");
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            // 547 = foreign key violation
            MessageBox.Show("This customer has orders and cannot be deleted.", "Cannot delete");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Could not delete customer: " + ex.Message, "Error");
        }
    }
}