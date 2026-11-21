using System.ComponentModel;
using BookStoreManagement.Data;
using BookStoreManagement.Models;

namespace BookStoreManagement.UI.Forms;

// Dialog used for both "Add Book" and "Edit Book".
public class BookEditForm : Form
{
    private readonly TextBox txtTitle = new TextBox();
    private readonly ComboBox cmbAuthor = new ComboBox();
    private readonly ComboBox cmbCategory = new ComboBox();
    private readonly NumericUpDown nudPrice = new NumericUpDown();
    private readonly NumericUpDown nudStock = new NumericUpDown();
    private readonly NumericUpDown nudYear = new NumericUpDown();
    private readonly Book book;

    // the book that was filled in by the user (read it when DialogResult is OK)
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Book Book => book;

    public BookEditForm(Book? existing)
    {
        book = existing ?? new Book();

        Text = existing == null ? "Add Book" : "Edit Book";
        ClientSize = new Size(500, 400);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.White;
        Font = Theme.FontRegular;

        // fill the combo boxes
        List<Author> authors = LookupRepository.GetAuthors();
        List<Category> categories = LookupRepository.GetCategories();
        cmbAuthor.DataSource = authors;
        cmbCategory.DataSource = categories;
        Theme.StyleComboBox(cmbAuthor);
        Theme.StyleComboBox(cmbCategory);

        Theme.StyleTextBox(txtTitle);
        txtTitle.MaxLength = 150;

        nudPrice.DecimalPlaces = 2;
        nudPrice.Minimum = 0.01m;
        nudPrice.Maximum = 100000m;
        nudStock.Minimum = 0;
        nudStock.Maximum = 100000;
        nudYear.Minimum = 1000;
        nudYear.Maximum = 2100;

        // current values
        txtTitle.Text = book.Title;
        if (existing != null)
        {
            cmbAuthor.SelectedItem = authors.FirstOrDefault(a => a.AuthorID == book.AuthorID);
            cmbCategory.SelectedItem = categories.FirstOrDefault(c => c.CategoryID == book.CategoryID);
        }
        nudPrice.Value = Math.Clamp(existing == null ? 10m : book.Price, 0.01m, 100000m);
        nudStock.Value = Math.Clamp(book.Stock, 0, 100000);
        nudYear.Value = Math.Clamp(book.PublishYear ?? DateTime.Now.Year, 1000, 2100);

        // ---- layout ----
        TableLayoutPanel table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Padding = new Padding(28, 24, 28, 0)
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddRow(table, 0, "Title", txtTitle);
        AddRow(table, 1, "Author", cmbAuthor);
        AddRow(table, 2, "Category", cmbCategory);
        AddRow(table, 3, "Price", nudPrice);
        AddRow(table, 4, "Stock", nudStock);
        AddRow(table, 5, "Publish year", nudYear);

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
        if (string.IsNullOrWhiteSpace(txtTitle.Text))
        {
            Dialogs.Info("Title is required.");
            return;
        }

        if (cmbAuthor.SelectedItem is not Author author || cmbCategory.SelectedItem is not Category category)
        {
            Dialogs.Info("Please select an author and a category.");
            return;
        }

        book.Title = txtTitle.Text.Trim();
        book.AuthorID = author.AuthorID;
        book.CategoryID = category.CategoryID;
        book.Price = nudPrice.Value;
        book.Stock = (int)nudStock.Value;
        book.PublishYear = (int)nudYear.Value;

        DialogResult = DialogResult.OK;
    }
}
