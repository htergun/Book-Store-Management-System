using BookStoreManagement.Data;
using BookStoreManagement.Models;
using BookStoreManagement.UI.Components;
using BookStoreManagement.UI.Forms;
using Microsoft.Data.SqlClient;

namespace BookStoreManagement.UI.Pages;

public class BooksPage : UserControl
{
    private readonly TextBox txtSearch = new TextBox();
    private readonly DataGridView grid = new DataGridView();

    public BooksPage()
    {
        BackColor = Theme.Background;

        // toolbar: search box + buttons
        Theme.StyleTextBox(txtSearch);
        txtSearch.PlaceholderText = "Search by title, author or category...";
        txtSearch.Width = 340;
        txtSearch.Margin = new Padding(0, 6, 20, 0);

        Button btnAdd = Theme.CreateButton("+ Add Book", Theme.Primary, 130);
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

        // table inside a card
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
        layout.Controls.Add(Theme.CreateHeader("Books", "Manage the books of the store"), 0, 0);
        layout.Controls.Add(toolbar, 0, 1);
        layout.Controls.Add(card, 0, 2);
        Controls.Add(layout);

        // events
        txtSearch.TextChanged += (s, e) => LoadData();
        btnAdd.Click += (s, e) => AddBook();
        btnEdit.Click += (s, e) => EditBook();
        btnDelete.Click += (s, e) => DeleteBook();
        grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) EditBook(); };
        grid.CellFormatting += Grid_CellFormatting;
        Load += (s, e) => LoadData();
    }

    private void LoadData()
    {
        try
        {
            grid.DataSource = BookRepository.GetAll(txtSearch.Text.Trim());
            grid.Columns["AuthorID"]!.Visible = false;
            grid.Columns["CategoryID"]!.Visible = false;
            grid.Columns["BookID"]!.FillWeight = 35;
            grid.Columns["Title"]!.FillWeight = 130;
            grid.Columns["Price"]!.DefaultCellStyle.Format = "N2";
        }
        catch (SqlException ex)
        {
            Dialogs.DatabaseError(ex);
        }
    }

    // red = out of stock, orange = low stock
    private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || grid.Columns[e.ColumnIndex].Name != "Stock" || e.Value == null)
            return;

        if (int.TryParse(e.Value.ToString(), out int stock))
        {
            if (stock == 0)
            {
                e.CellStyle!.ForeColor = Theme.Danger;
                e.CellStyle.Font = Theme.FontBold;
            }
            else if (stock <= 5)
            {
                e.CellStyle!.ForeColor = Theme.Warning;
                e.CellStyle.Font = Theme.FontBold;
            }
        }
    }

    private Book? GetSelectedBook()
    {
        if (grid.CurrentRow == null)
        {
            Dialogs.Info("Please select a book first.");
            return null;
        }

        DataGridViewRow row = grid.CurrentRow;
        return new Book
        {
            BookID = Convert.ToInt32(row.Cells["BookID"].Value),
            Title = row.Cells["Title"].Value?.ToString() ?? "",
            AuthorID = Convert.ToInt32(row.Cells["AuthorID"].Value),
            CategoryID = Convert.ToInt32(row.Cells["CategoryID"].Value),
            Price = Convert.ToDecimal(row.Cells["Price"].Value),
            Stock = Convert.ToInt32(row.Cells["Stock"].Value),
            PublishYear = row.Cells["Year"].Value == DBNull.Value ? null : Convert.ToInt32(row.Cells["Year"].Value)
        };
    }

    private void AddBook()
    {
        try
        {
            using BookEditForm dialog = new BookEditForm(null);
            if (dialog.ShowDialog(FindForm()) == DialogResult.OK)
            {
                BookRepository.Add(dialog.Book);
                LoadData();
            }
        }
        catch (SqlException ex)
        {
            Dialogs.DatabaseError(ex);
        }
    }

    private void EditBook()
    {
        Book? selected = GetSelectedBook();
        if (selected == null) return;

        try
        {
            using BookEditForm dialog = new BookEditForm(selected);
            if (dialog.ShowDialog(FindForm()) == DialogResult.OK)
            {
                BookRepository.Update(dialog.Book);
                LoadData();
            }
        }
        catch (SqlException ex)
        {
            Dialogs.DatabaseError(ex);
        }
    }

    private void DeleteBook()
    {
        Book? selected = GetSelectedBook();
        if (selected == null) return;

        if (!Dialogs.Confirm("Delete \"" + selected.Title + "\"?")) return;

        try
        {
            BookRepository.Delete(selected.BookID);
            LoadData();
        }
        catch (SqlException ex) when (ex.Number == 547) // foreign key violation
        {
            Dialogs.Info("This book cannot be deleted because it has orders.");
        }
        catch (SqlException ex)
        {
            Dialogs.DatabaseError(ex);
        }
    }
}
