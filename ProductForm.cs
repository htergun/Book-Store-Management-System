using System.Data;
using Microsoft.Data.SqlClient;

namespace BookStoreManagement;

public class ProductForm : Form
{
    private readonly TextBox txtSearch = new();
    private readonly DataGridView dgvProducts = new();
    private readonly TextBox txtTitle = new();
    private readonly TextBox txtAuthor = new();
    private readonly ComboBox cmbCategory = new();
    private readonly NumericUpDown numPrice = new();
    private readonly NumericUpDown numStock = new();
    private readonly Button btnAdd = new();
    private readonly Button btnUpdate = new();
    private readonly Button btnDelete = new();
    private readonly Button btnClear = new();

    private int selectedProductId = 0;   // 0 = no product selected

    public ProductForm()
    {
        BuildUi();
        LoadCategories();
        LoadProducts();
    }

    // ---------- UI ----------
    private void BuildUi()
    {
        Text = "Products";
        Width = 1000;
        Height = 620;
        StartPosition = FormStartPosition.CenterScreen;

        // Search panel (top)
        var topPanel = new Panel { Dock = DockStyle.Fill };
        txtSearch.Location = new Point(70, 14);
        txtSearch.Width = 220;
        txtSearch.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { LoadProducts(txtSearch.Text.Trim()); e.SuppressKeyPress = true; }
        };
        var btnSearch = new Button { Text = "Search", Location = new Point(300, 12), Size = new Size(90, 28) };
        btnSearch.Click += (s, e) => LoadProducts(txtSearch.Text.Trim());
        var btnShowAll = new Button { Text = "Show All", Location = new Point(400, 12), Size = new Size(90, 28) };
        btnShowAll.Click += (s, e) => { txtSearch.Clear(); LoadProducts(); };
        var lblHint = new Label
        {
            Text = "Red rows are out of stock.",
            ForeColor = Color.Firebrick,
            AutoSize = true,
            Location = new Point(510, 18)
        };
        topPanel.Controls.AddRange(new Control[]
        {
            MakeLabel("Search:", 15, 18), txtSearch, btnSearch, btnShowAll, lblHint
        });

        // Grid (middle)
        dgvProducts.Dock = DockStyle.Fill;
        dgvProducts.ReadOnly = true;
        dgvProducts.AllowUserToAddRows = false;
        dgvProducts.MultiSelect = false;
        dgvProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvProducts.SelectionChanged += (s, e) => FillInputsFromSelection();
        dgvProducts.DataBindingComplete += (s, e) => HighlightOutOfStock();

        // Input panel (bottom)
        var bottomPanel = new Panel { Dock = DockStyle.Fill };
        txtTitle.Location = new Point(15, 32); txtTitle.Width = 200;
        txtAuthor.Location = new Point(230, 32); txtAuthor.Width = 170;

        cmbCategory.Location = new Point(415, 32);
        cmbCategory.Width = 130;
        cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;

        numPrice.Location = new Point(560, 32);
        numPrice.Width = 90;
        numPrice.DecimalPlaces = 2;
        numPrice.Maximum = 100000;

        numStock.Location = new Point(665, 32);
        numStock.Width = 80;
        numStock.Maximum = 100000;

        btnAdd.Text = "Add"; btnAdd.Location = new Point(15, 80); btnAdd.Size = new Size(100, 32);
        btnUpdate.Text = "Update"; btnUpdate.Location = new Point(125, 80); btnUpdate.Size = new Size(100, 32);
        btnDelete.Text = "Delete"; btnDelete.Location = new Point(235, 80); btnDelete.Size = new Size(100, 32);
        btnClear.Text = "Clear"; btnClear.Location = new Point(345, 80); btnClear.Size = new Size(100, 32);

        btnAdd.Click += (s, e) => AddProduct();
        btnUpdate.Click += (s, e) => UpdateProduct();
        btnDelete.Click += (s, e) => DeleteProduct();
        btnClear.Click += (s, e) => ClearInputs();

        // Role-based permission: only admins can delete
        btnDelete.Enabled = UserSession.IsAdmin;
        var lblRoleNote = new Label
        {
            Text = "Only admins can delete products.",
            ForeColor = Color.Firebrick,
            AutoSize = true,
            Location = new Point(460, 88),
            Visible = !UserSession.IsAdmin
        };

        bottomPanel.Controls.AddRange(new Control[]
        {
            MakeLabel("Title", 15, 12), txtTitle,
            MakeLabel("Author", 230, 12), txtAuthor,
            MakeLabel("Category", 415, 12), cmbCategory,
            MakeLabel("Price", 560, 12), numPrice,
            MakeLabel("Stock", 665, 12), numStock,
            btnAdd, btnUpdate, btnDelete, btnClear, lblRoleNote
        });

        // Layout: 3 rows (search / grid / inputs)
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 130));
        layout.Controls.Add(topPanel, 0, 0);
        layout.Controls.Add(dgvProducts, 0, 1);
        layout.Controls.Add(bottomPanel, 0, 2);
        Controls.Add(layout);
    }

    private static Label MakeLabel(string text, int x, int y)
    {
        return new Label { Text = text, AutoSize = true, Location = new Point(x, y) };
    }

    // ---------- DATA ----------
    private void LoadCategories()
    {
        try
        {
            using var connection = DbHelper.GetConnection();
            using var adapter = new SqlDataAdapter(
                "SELECT category_id, category_name FROM Categories ORDER BY category_name", connection);

            var table = new DataTable();
            adapter.Fill(table);

            cmbCategory.DataSource = table;
            cmbCategory.DisplayMember = "category_name";
            cmbCategory.ValueMember = "category_id";
            cmbCategory.SelectedIndex = -1;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Could not load categories: " + ex.Message, "Error");
        }
    }

    private void LoadProducts(string search = "")
    {
        try
        {
            using var connection = DbHelper.GetConnection();

            // JOIN with Categories to show the category name; parameterized LIKE search
            const string sql =
                "SELECT p.product_id AS ID, p.title AS Title, p.author AS Author, " +
                "c.category_name AS Category, p.price AS Price, p.stock AS Stock " +
                "FROM Products p JOIN Categories c ON c.category_id = p.category_id " +
                "WHERE p.title LIKE @s OR p.author LIKE @s OR c.category_name LIKE @s " +
                "ORDER BY p.product_id";

            using var adapter = new SqlDataAdapter(sql, connection);
            adapter.SelectCommand.Parameters.Add("@s", SqlDbType.NVarChar, 100).Value = "%" + search + "%";

            var table = new DataTable();
            adapter.Fill(table);
            dgvProducts.DataSource = table;

            if (dgvProducts.Columns.Count > 5)
                dgvProducts.Columns[4].DefaultCellStyle.Format = "N2";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Could not load products: " + ex.Message, "Error");
        }
    }

    private void HighlightOutOfStock()
    {
        foreach (DataGridViewRow row in dgvProducts.Rows)
        {
            if (row.Cells[5].Value != null && Convert.ToInt32(row.Cells[5].Value) == 0)
                row.DefaultCellStyle.BackColor = Color.MistyRose;
        }
    }

    private void FillInputsFromSelection()
    {
        if (dgvProducts.CurrentRow == null)
        {
            selectedProductId = 0;
            return;
        }

        var cells = dgvProducts.CurrentRow.Cells;
        selectedProductId = Convert.ToInt32(cells[0].Value);
        txtTitle.Text = cells[1].Value?.ToString() ?? "";
        txtAuthor.Text = cells[2].Value?.ToString() ?? "";
        cmbCategory.SelectedIndex = cmbCategory.FindStringExact(cells[3].Value?.ToString() ?? "");
        numPrice.Value = Math.Min(Convert.ToDecimal(cells[4].Value), numPrice.Maximum);
        numStock.Value = Math.Min(Convert.ToDecimal(cells[5].Value), numStock.Maximum);
    }

    private void ClearInputs()
    {
        selectedProductId = 0;
        txtTitle.Clear();
        txtAuthor.Clear();
        cmbCategory.SelectedIndex = -1;
        numPrice.Value = 0;
        numStock.Value = 0;
        dgvProducts.ClearSelection();
    }

    private bool InputsAreValid()
    {
        if (string.IsNullOrWhiteSpace(txtTitle.Text))
        {
            MessageBox.Show("Title is required.", "Missing data");
            return false;
        }
        if (cmbCategory.SelectedValue == null)
        {
            MessageBox.Show("Select a category.", "Missing data");
            return false;
        }
        if (numPrice.Value <= 0)
        {
            MessageBox.Show("Price must be greater than 0.", "Invalid data");
            return false;
        }
        return true;
    }

    // Adds the parameters shared by INSERT and UPDATE
    private void AddProductParameters(SqlCommand cmd)
    {
        cmd.Parameters.Add("@title", SqlDbType.NVarChar, 100).Value = txtTitle.Text.Trim();
        cmd.Parameters.Add("@author", SqlDbType.NVarChar, 100).Value =
            string.IsNullOrWhiteSpace(txtAuthor.Text) ? DBNull.Value : txtAuthor.Text.Trim();

        // Decimal needs precision and scale, otherwise the cents would be cut off
        var price = new SqlParameter("@price", SqlDbType.Decimal) { Precision = 10, Scale = 2, Value = numPrice.Value };
        cmd.Parameters.Add(price);

        cmd.Parameters.Add("@stock", SqlDbType.Int).Value = (int)numStock.Value;
        cmd.Parameters.Add("@cat", SqlDbType.Int).Value = Convert.ToInt32(cmbCategory.SelectedValue);
    }

    // ---------- ADD / UPDATE / DELETE ----------
    private void AddProduct()
    {
        if (!InputsAreValid()) return;

        try
        {
            using var connection = DbHelper.GetConnection();
            connection.Open();

            const string sql =
                "INSERT INTO Products (title, author, price, stock, category_id) " +
                "VALUES (@title, @author, @price, @stock, @cat)";

            using var cmd = new SqlCommand(sql, connection);
            AddProductParameters(cmd);
            cmd.ExecuteNonQuery();

            ClearInputs();
            LoadProducts(txtSearch.Text.Trim());
            MessageBox.Show("Product added.", "Success");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Could not add product: " + ex.Message, "Error");
        }
    }

    private void UpdateProduct()
    {
        if (selectedProductId == 0)
        {
            MessageBox.Show("Select a product from the list first.", "No selection");
            return;
        }
        if (!InputsAreValid()) return;

        try
        {
            using var connection = DbHelper.GetConnection();
            connection.Open();

            const string sql =
                "UPDATE Products SET title = @title, author = @author, price = @price, " +
                "stock = @stock, category_id = @cat WHERE product_id = @id";

            using var cmd = new SqlCommand(sql, connection);
            AddProductParameters(cmd);
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = selectedProductId;
            cmd.ExecuteNonQuery();

            LoadProducts(txtSearch.Text.Trim());
            MessageBox.Show("Product updated.", "Success");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Could not update product: " + ex.Message, "Error");
        }
    }

    private void DeleteProduct()
    {
        if (!UserSession.IsAdmin) return;   // double check on the code side too

        if (selectedProductId == 0)
        {
            MessageBox.Show("Select a product from the list first.", "No selection");
            return;
        }

        var answer = MessageBox.Show("Delete this product?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (answer != DialogResult.Yes) return;

        try
        {
            using var connection = DbHelper.GetConnection();
            connection.Open();

            using var cmd = new SqlCommand("DELETE FROM Products WHERE product_id = @id", connection);
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = selectedProductId;
            cmd.ExecuteNonQuery();

            ClearInputs();
            LoadProducts(txtSearch.Text.Trim());
            MessageBox.Show("Product deleted.", "Success");
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            // 547 = foreign key violation
            MessageBox.Show("This product exists in orders and cannot be deleted.", "Cannot delete");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Could not delete product: " + ex.Message, "Error");
        }
    }
}