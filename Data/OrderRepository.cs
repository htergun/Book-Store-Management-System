using System.Data;
using BookStoreManagement.Models;
using Microsoft.Data.SqlClient;

namespace BookStoreManagement.Data;

public static class OrderRepository
{
    // all orders with customer, book and total price (top = 0 means all rows)
    public static DataTable GetAll(int top = 0)
    {
        string topPart = top > 0 ? "TOP (" + top + ") " : "";

        string sql = "SELECT " + topPart + @"o.OrderID, cu.FirstName + ' ' + cu.LastName AS Customer,
                              b.Title AS Book, o.Quantity, b.Price * o.Quantity AS Total,
                              CONVERT(VARCHAR(10), o.OrderDate, 23) AS [Date]
                       FROM Orders o
                       INNER JOIN Customers cu ON o.CustomerID = cu.CustomerID
                       INNER JOIN Books b      ON o.BookID = b.BookID
                       ORDER BY o.OrderDate DESC, o.OrderID DESC";

        return Database.GetTable(sql);
    }

    // Places an order: checks the stock, inserts the order and decreases the stock
    // in ONE transaction. Returns null if everything is OK, otherwise an error message.
    public static string? PlaceOrder(Order order)
    {
        using SqlConnection conn = Database.CreateConnection();
        conn.Open();
        using SqlTransaction tran = conn.BeginTransaction();

        try
        {
            // 1) check the stock
            using SqlCommand check = new SqlCommand("SELECT Stock FROM Books WHERE BookID = @id", conn, tran);
            check.Parameters.AddWithValue("@id", order.BookID);
            object? result = check.ExecuteScalar();

            if (result == null)
            {
                tran.Rollback();
                return "Book not found.";
            }

            int stock = Convert.ToInt32(result);
            if (stock < order.Quantity)
            {
                tran.Rollback();
                return "Not enough stock. Available: " + stock;
            }

            // 2) insert the order
            using SqlCommand insert = new SqlCommand(
                "INSERT INTO Orders (CustomerID, BookID, Quantity) VALUES (@c, @b, @q)", conn, tran);
            insert.Parameters.AddWithValue("@c", order.CustomerID);
            insert.Parameters.AddWithValue("@b", order.BookID);
            insert.Parameters.AddWithValue("@q", order.Quantity);
            insert.ExecuteNonQuery();

            // 3) decrease the stock
            using SqlCommand update = new SqlCommand(
                "UPDATE Books SET Stock = Stock - @q WHERE BookID = @b", conn, tran);
            update.Parameters.AddWithValue("@q", order.Quantity);
            update.Parameters.AddWithValue("@b", order.BookID);
            update.ExecuteNonQuery();

            tran.Commit();
            return null;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    public static int Count()
    {
        return Convert.ToInt32(Database.Scalar("SELECT COUNT(*) FROM Orders"));
    }

    public static decimal TotalRevenue()
    {
        object? value = Database.Scalar(
            "SELECT ISNULL(SUM(b.Price * o.Quantity), 0) FROM Orders o INNER JOIN Books b ON o.BookID = b.BookID");
        return Convert.ToDecimal(value);
    }
}
