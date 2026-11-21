using System.Data;
using BookStoreManagement.Data;
using BookStoreManagement.UI.Components;
using Microsoft.Data.SqlClient;

namespace BookStoreManagement.UI.Pages;

// Home page: four number cards, the latest orders and the books that are almost out of stock.
public class DashboardPage : UserControl
{
    private readonly StatCard cardBooks = new StatCard("Total books", Theme.Primary);
    private readonly StatCard cardCustomers = new StatCard("Customers", Theme.Info);
    private readonly StatCard cardOrders = new StatCard("Orders", Theme.Success);
    private readonly StatCard cardRevenue = new StatCard("Revenue", Theme.Warning);
    private readonly DataGridView gridRecent = new DataGridView();
    private readonly DataGridView gridLowStock = new DataGridView();

    public DashboardPage()
    {
        BackColor = Theme.Background;

        // ---- number cards ----
        TableLayoutPanel stats = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1 };
        stats.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        for (int i = 0; i < 4; i++)
            stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        cardBooks.Margin = new Padding(0, 0, 16, 0);
        cardCustomers.Margin = new Padding(0, 0, 16, 0);
        cardOrders.Margin = new Padding(0, 0, 16, 0);
        cardRevenue.Margin = new Padding(0);
        stats.Controls.Add(cardBooks, 0, 0);
        stats.Controls.Add(cardCustomers, 1, 0);
        stats.Controls.Add(cardOrders, 2, 0);
        stats.Controls.Add(cardRevenue, 3, 0);

        // ---- two tables ----
        Theme.StyleGrid(gridRecent);
        Theme.StyleGrid(gridLowStock);

        CardPanel recentCard = CreateGridCard("Latest orders", gridRecent);
        CardPanel lowStockCard = CreateGridCard("Low stock (5 or less)", gridLowStock);
        recentCard.Margin = new Padding(0, 20, 16, 0);
        lowStockCard.Margin = new Padding(0, 20, 0, 0);

        TableLayoutPanel tables = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        tables.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        tables.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
        tables.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        tables.Controls.Add(recentCard, 0, 0);
        tables.Controls.Add(lowStockCard, 1, 0);

        TableLayoutPanel layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(28)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(Theme.CreateHeader("Dashboard", "Overview of the bookstore"), 0, 0);
        layout.Controls.Add(stats, 0, 1);
        layout.Controls.Add(tables, 0, 2);
        Controls.Add(layout);

        Load += (s, e) => LoadData();
    }

    // card with a title on top and a table below
    private static CardPanel CreateGridCard(string title, DataGridView grid)
    {
        Label lblTitle = new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Height = 46,
            Font = Theme.FontBold,
            ForeColor = Theme.Ink,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(16, 0, 0, 0)
        };

        grid.Dock = DockStyle.Fill;

        CardPanel card = new CardPanel { Dock = DockStyle.Fill };
        card.Controls.Add(grid);       // added first, so it fills what is left
        card.Controls.Add(lblTitle);
        return card;
    }

    private void LoadData()
    {
        try
        {
            cardBooks.Value = BookRepository.Count().ToString();
            cardCustomers.Value = CustomerRepository.Count().ToString();
            cardOrders.Value = OrderRepository.Count().ToString();
            cardRevenue.Value = OrderRepository.TotalRevenue().ToString("N2");

            gridRecent.DataSource = OrderRepository.GetAll(6);
            gridRecent.Columns["OrderID"]!.Visible = false;
            gridRecent.Columns["Book"]!.FillWeight = 130;
            gridRecent.Columns["Quantity"]!.FillWeight = 50;
            gridRecent.Columns["Total"]!.DefaultCellStyle.Format = "N2";

            gridLowStock.DataSource = BookRepository.GetLowStock();
            gridLowStock.Columns["Title"]!.FillWeight = 160;
            gridLowStock.Columns["Stock"]!.FillWeight = 40;
        }
        catch (SqlException ex)
        {
            Dialogs.DatabaseError(ex);
        }
    }
}
