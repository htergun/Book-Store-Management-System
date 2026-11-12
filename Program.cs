using System.Globalization;
using Microsoft.Data.SqlClient;

namespace BookStoreManagement;

internal class Program
{
    // Change the server name if you use SQL Server Express (e.g. .\SQLEXPRESS)
    private const string ConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;" +
        "Database=BookStoreDB;" +
        "Trusted_Connection=True;TrustServerCertificate=True;";

    private static void Main()
    {
        bool running = true;

        while (running)
        {
            ShowMenu();
            string choice = Console.ReadLine()?.Trim() ?? "";
            Console.WriteLine();

            try
            {
                switch (choice)
                {
                    case "1": ListBooks(); break;
                    case "2": SearchBook(); break;
                    case "3": AddBook(); break;
                    case "4": UpdateStock(); break;
                    case "5": DeleteBook(); break;
                    case "6": ListCustomers(); break;
                    case "7": AddCustomer(); break;
                    case "8": PlaceOrder(); break;
                    case "9": ListOrders(); break;
                    case "0": running = false; break;
                    default: Console.WriteLine("Invalid choice."); break;
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Database error: " + ex.Message);
            }

            if (running)
            {
                Console.WriteLine("\nPress Enter to continue...");
                Console.ReadLine();
            }
        }

        Console.WriteLine("Goodbye!");
    }

    private static void ShowMenu()
    {
        Console.Clear();
        Console.WriteLine("=====================================");
        Console.WriteLine("      BOOKSTORE MANAGEMENT SYSTEM    ");
        Console.WriteLine("=====================================");
        Console.WriteLine("1 - List all books");
        Console.WriteLine("2 - Search book by title");
        Console.WriteLine("3 - Add new book");
        Console.WriteLine("4 - Update book stock");
        Console.WriteLine("5 - Delete book");
        Console.WriteLine("6 - List customers");
        Console.WriteLine("7 - Add new customer");
        Console.WriteLine("8 - Place an order");
        Console.WriteLine("9 - List all orders");
        Console.WriteLine("0 - Exit");
        Console.Write("\nYour choice: ");
    }

    // ---------------- BOOKS ----------------

    private static void ListBooks()
    {
        string sql = @"SELECT b.BookID, b.Title, a.FirstName + ' ' + a.LastName AS Author,
                              c.CategoryName AS Category, b.Price, b.Stock
                       FROM Books b
                       INNER JOIN Authors a    ON b.AuthorID = a.AuthorID
                       INNER JOIN Categories c ON b.CategoryID = c.CategoryID
                       ORDER BY b.BookID";
        PrintQuery(sql);
    }

    private static void SearchBook()
    {
        string keyword = ReadText("Enter part of the title: ");
        string sql = @"SELECT BookID, Title, Price, Stock
                       FROM Books
                       WHERE Title LIKE @keyword";
        PrintQuery(sql, new SqlParameter("@keyword", "%" + keyword + "%"));
    }

    private static void AddBook()
    {
        PrintQuery("SELECT AuthorID, FirstName, LastName FROM Authors");
        PrintQuery("SELECT CategoryID, CategoryName FROM Categories");

        string title = ReadText("Title: ");
        int authorId = ReadInt("Author ID: ");
        int categoryId = ReadInt("Category ID: ");
        decimal price = ReadDecimal("Price: ");
        int stock = ReadInt("Stock: ");
        int year = ReadInt("Publish year: ");

        string sql = @"INSERT INTO Books (Title, AuthorID, CategoryID, Price, Stock, PublishYear)
                       VALUES (@title, @authorId, @categoryId, @price, @stock, @year)";

        RunNonQuery(sql,
            new SqlParameter("@title", title),
            new SqlParameter("@authorId", authorId),
            new SqlParameter("@categoryId", categoryId),
            new SqlParameter("@price", price),
            new SqlParameter("@stock", stock),
            new SqlParameter("@year", year));

        Console.WriteLine("Book added successfully.");
    }

    private static void UpdateStock()
    {
        int bookId = ReadInt("Book ID: ");
        int newStock = ReadInt("New stock value: ");

        int affected = RunNonQuery("UPDATE Books SET Stock = @stock WHERE BookID = @id",
            new SqlParameter("@stock", newStock),
            new SqlParameter("@id", bookId));

        Console.WriteLine(affected > 0 ? "Stock updated." : "Book not found.");
    }

    private static void DeleteBook()
    {
        int bookId = ReadInt("Book ID to delete: ");

        try
        {
            int affected = RunNonQuery("DELETE FROM Books WHERE BookID = @id",
                new SqlParameter("@id", bookId));
            Console.WriteLine(affected > 0 ? "Book deleted." : "Book not found.");
        }
        catch (SqlException ex) when (ex.Number == 547) // foreign key violation
        {
            Console.WriteLine("This book cannot be deleted because it has orders.");
        }
    }

    // ---------------- CUSTOMERS ----------------

    private static void ListCustomers()
    {
        PrintQuery("SELECT CustomerID, FirstName, LastName, Email, Phone, City FROM Customers");
    }

    private static void AddCustomer()
    {
        string firstName = ReadText("First name: ");
        string lastName = ReadText("Last name: ");
        string email = ReadText("Email: ");
        string phone = ReadText("Phone: ");
        string city = ReadText("City: ");

        string sql = @"INSERT INTO Customers (FirstName, LastName, Email, Phone, City)
                       VALUES (@fn, @ln, @email, @phone, @city)";

        RunNonQuery(sql,
            new SqlParameter("@fn", firstName),
            new SqlParameter("@ln", lastName),
            new SqlParameter("@email", email),
            new SqlParameter("@phone", phone),
            new SqlParameter("@city", city));

        Console.WriteLine("Customer added successfully.");
    }

    // ---------------- ORDERS ----------------

    private static void PlaceOrder()
    {
        int customerId = ReadInt("Customer ID: ");
        int bookId = ReadInt("Book ID: ");
        int quantity = ReadInt("Quantity: ");

        using SqlConnection conn = new SqlConnection(ConnectionString);
        conn.Open();
        using SqlTransaction tran = conn.BeginTransaction();

        try
        {
            // 1) check the stock
            using SqlCommand check = new SqlCommand("SELECT Stock FROM Books WHERE BookID = @id", conn, tran);
            check.Parameters.AddWithValue("@id", bookId);
            object? result = check.ExecuteScalar();

            if (result == null)
            {
                Console.WriteLine("Book not found.");
                tran.Rollback();
                return;
            }

            if ((int)result < quantity)
            {
                Console.WriteLine("Not enough stock. Available: " + result);
                tran.Rollback();
                return;
            }

            // 2) insert the order
            using SqlCommand insert = new SqlCommand(
                "INSERT INTO Orders (CustomerID, BookID, Quantity) VALUES (@c, @b, @q)", conn, tran);
            insert.Parameters.AddWithValue("@c", customerId);
            insert.Parameters.AddWithValue("@b", bookId);
            insert.Parameters.AddWithValue("@q", quantity);
            insert.ExecuteNonQuery();

            // 3) decrease the stock
            using SqlCommand update = new SqlCommand(
                "UPDATE Books SET Stock = Stock - @q WHERE BookID = @b", conn, tran);
            update.Parameters.AddWithValue("@q", quantity);
            update.Parameters.AddWithValue("@b", bookId);
            update.ExecuteNonQuery();

            tran.Commit();
            Console.WriteLine("Order placed successfully.");
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    private static void ListOrders()
    {
        string sql = @"SELECT o.OrderID, cu.FirstName + ' ' + cu.LastName AS Customer,
                              b.Title, o.Quantity, b.Price * o.Quantity AS TotalPrice,
                              CONVERT(VARCHAR(10), o.OrderDate, 23) AS OrderDate
                       FROM Orders o
                       INNER JOIN Customers cu ON o.CustomerID = cu.CustomerID
                       INNER JOIN Books b      ON o.BookID = b.BookID
                       ORDER BY o.OrderDate, o.OrderID";
        PrintQuery(sql);
    }

    // ---------------- HELPER METHODS ----------------

    // Runs a SELECT query and prints the result as a simple table
    private static void PrintQuery(string sql, params SqlParameter[] parameters)
    {
        using SqlConnection conn = new SqlConnection(ConnectionString);
        conn.Open();
        using SqlCommand cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddRange(parameters);
        using SqlDataReader reader = cmd.ExecuteReader();

        int columnCount = reader.FieldCount;

        // read the header and all rows first, so the column widths can be calculated
        string[] headers = new string[columnCount];
        for (int i = 0; i < columnCount; i++)
            headers[i] = reader.GetName(i);

        List<string[]> rows = new List<string[]>();
        while (reader.Read())
        {
            string[] row = new string[columnCount];
            for (int i = 0; i < columnCount; i++)
                row[i] = reader.IsDBNull(i) ? "" : reader.GetValue(i).ToString() ?? "";
            rows.Add(row);
        }

        // width of a column = longest text in it (maximum 30 characters)
        int[] widths = new int[columnCount];
        for (int i = 0; i < columnCount; i++)
        {
            widths[i] = headers[i].Length;
            foreach (string[] row in rows)
                widths[i] = Math.Max(widths[i], row[i].Length);
            widths[i] = Math.Min(widths[i], 30);
        }

        PrintRow(headers, widths);
        Console.WriteLine(new string('-', widths.Sum() + 2 * columnCount));
        foreach (string[] row in rows)
            PrintRow(row, widths);

        Console.WriteLine("\n" + rows.Count + " row(s).");
    }

    private static void PrintRow(string[] values, int[] widths)
    {
        for (int i = 0; i < values.Length; i++)
        {
            string text = values[i];
            if (text.Length > widths[i])
                text = text.Substring(0, widths[i] - 1) + ".";
            Console.Write(text.PadRight(widths[i] + 2));
        }
        Console.WriteLine();
    }

    // Runs INSERT / UPDATE / DELETE and returns the number of affected rows
    private static int RunNonQuery(string sql, params SqlParameter[] parameters)
    {
        using SqlConnection conn = new SqlConnection(ConnectionString);
        conn.Open();
        using SqlCommand cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddRange(parameters);
        return cmd.ExecuteNonQuery();
    }

    private static string ReadText(string message)
    {
        Console.Write(message);
        return Console.ReadLine()?.Trim() ?? "";
    }

    private static int ReadInt(string message)
    {
        while (true)
        {
            Console.Write(message);
            if (int.TryParse(Console.ReadLine(), out int value))
                return value;
            Console.WriteLine("Please enter a valid whole number.");
        }
    }

    private static decimal ReadDecimal(string message)
    {
        while (true)
        {
            Console.Write(message);
            // accepts both 9.99 and 9,99
            string input = (Console.ReadLine() ?? "").Replace(',', '.');
            if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value))
                return value;
            Console.WriteLine("Please enter a valid number.");
        }
    }
}
