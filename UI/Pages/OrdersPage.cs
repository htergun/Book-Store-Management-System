using BookStoreManagement.Data;
using BookStoreManagement.Models;
using BookStoreManagement.UI.Components;
using Microsoft.Data.SqlClient;

namespace BookStoreManagement.UI.Pages;

public class OrdersPage : UserControl
{
    private readonly ComboBox cmbCustomer = new ComboBox();
    private readonly ComboBox cmbBook = new ComboBox();
    private readonly NumericUpDown nudQuantity = new NumericUpDown();
    private readonly Label lblTotal = new Label();
    private readonly DataGridView grid = new DataGridView();

    public OrdersPage()
    {
        BackColor = Theme.Background;

        // ---- "new order" card ----
        Theme.StyleComboBox(cmbCustomer);
        Theme.StyleComboBox(cmbBook);
        nudQuantity.Font = Theme.FontRegular;
        nudQuantity.Minimum = 1;
        nudQuantity.Maximum = 1000;
        nudQuantity.Value = 1;

        lblTotal.Text = "0.00";
        lblTotal.Font = new Font("Segoe UI Semibold", 13F);
        lblTotal.ForeColor = Theme.Primary;
        lblTotal.Height = 30;
        lblTotal.TextAlign = ContentAlignment.MiddleLeft;

        Button btnPlace = Theme.CreateButton("Place Order", Theme.Success, 130);
        Panel buttonField = new Panel { Width = 130, Height = 66 };
        btnPlace.Location = new Point(0, 24);
        buttonField.Controls.Add(btnPlace);

        FlowLayoutPanel form = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Padding = new Padding(20, 16, 0, 0)
        };
        form.Controls.Add(Theme.CreateField("Customer", cmbCustomer, 200));
        form.Controls.Add(Theme.CreateField("Book", cmbBook, 270));
        form.Controls.Add(Theme.CreateField("Quantity", nudQuantity, 80));
        form.Controls.Add(Theme.CreateField("Total", lblTotal, 100));
        form.Controls.Add(buttonField);

        CardPanel orderCard = new CardPanel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 20) };
        orderCard.Controls.Add(form);

        // ---- orders table ----
        Theme.StyleGrid(grid);
        grid.Dock = DockStyle.Fill;
        CardPanel gridCard = new CardPanel { Dock = DockStyle.Fill, Padding = new Padding(1) };
        gridCard.Controls.Add(grid);

        TableLayoutPanel layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(28)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 128));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(Theme.CreateHeader("Orders", "Place a new order and see the order history"), 0, 0);
        layout.Controls.Add(orderCard, 0, 1);
        layout.Controls.Add(gridCard, 0, 2);
        Controls.Add(layout);

        // events
        cmbBook.SelectedIndexChanged += (s, e) => UpdateTotal();
        nudQuantity.ValueChanged += (s, e) => UpdateTotal();
        btnPlace.Click += (s, e) => PlaceOrder();
        Load += (s, e) => LoadData();
    }

    private void LoadData()
    {
        try
        {
            // keep the current selection after reloading
            int? selectedCustomer = (cmbCustomer.SelectedItem as Customer)?.CustomerID;
            int? selectedBook = (cmbBook.SelectedItem as Book)?.BookID;

            List<Customer> customers = CustomerRepository.GetList();
            List<Book> books = BookRepository.GetList();
            cmbCustomer.DataSource = customers;
            cmbBook.DataSource = books;

            Customer? customerMatch = customers.FirstOrDefault(c => c.CustomerID == selectedCustomer);
            if (customerMatch != null) cmbCustomer.SelectedItem = customerMatch;

            Book? bookMatch = books.FirstOrDefault(b => b.BookID == selectedBook);
            if (bookMatch != null) cmbBook.SelectedItem = bookMatch;

            grid.DataSource = OrderRepository.GetAll();
            grid.Columns["OrderID"]!.HeaderText = "Order";
            grid.Columns["OrderID"]!.FillWeight = 35;
            grid.Columns["Book"]!.FillWeight = 130;
            grid.Columns["Quantity"]!.FillWeight = 50;
            grid.Columns["Total"]!.DefaultCellStyle.Format = "N2";

            UpdateTotal();
        }
        catch (SqlException ex)
        {
            Dialogs.DatabaseError(ex);
        }
    }

    private void UpdateTotal()
    {
        if (cmbBook.SelectedItem is Book book)
            lblTotal.Text = (book.Price * nudQuantity.Value).ToString("N2");
        else
            lblTotal.Text = "0.00";
    }

    private void PlaceOrder()
    {
        if (cmbCustomer.SelectedItem is not Customer customer || cmbBook.SelectedItem is not Book book)
        {
            Dialogs.Info("Please select a customer and a book.");
            return;
        }

        Order order = new Order
        {
            CustomerID = customer.CustomerID,
            BookID = book.BookID,
            Quantity = (int)nudQuantity.Value
        };

        try
        {
            string? error = OrderRepository.PlaceOrder(order);
            if (error != null)
            {
                Dialogs.Error(error);
                return;
            }

            Dialogs.Info("Order placed successfully.");
            nudQuantity.Value = 1;
            LoadData();   // refresh the stock numbers and the order list
        }
        catch (SqlException ex)
        {
            Dialogs.DatabaseError(ex);
        }
    }
}
