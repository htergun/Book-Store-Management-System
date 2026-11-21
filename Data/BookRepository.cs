using System.Data;
using BookStoreManagement.Models;
using Microsoft.Data.SqlClient;

namespace BookStoreManagement.Data;

public static class BookRepository
{
    // all books with author and category names (search is optional)
    public static DataTable GetAll(string search = "")
    {
        string sql = @"SELECT b.BookID, b.Title, a.FirstName + ' ' + a.LastName AS Author,
                              c.CategoryName AS Category, b.Price, b.Stock,
                              b.PublishYear AS [Year], b.AuthorID, b.CategoryID
                       FROM Books b
                       INNER JOIN Authors a    ON b.AuthorID = a.AuthorID
                       INNER JOIN Categories c ON b.CategoryID = c.CategoryID
                       WHERE b.Title LIKE @s
                          OR a.FirstName + ' ' + a.LastName LIKE @s
                          OR c.CategoryName LIKE @s
                       ORDER BY b.BookID";

        return Database.GetTable(sql, Database.Param("@s", "%" + search + "%"));
    }

    // simple list (used by the order page combo box)
    public static List<Book> GetList()
    {
        DataTable table = Database.GetTable(
            "SELECT BookID, Title, AuthorID, CategoryID, Price, Stock, PublishYear FROM Books ORDER BY Title");

        List<Book> list = new List<Book>();
        foreach (DataRow row in table.Rows)
        {
            list.Add(new Book
            {
                BookID = Convert.ToInt32(row["BookID"]),
                Title = row["Title"].ToString() ?? "",
                AuthorID = Convert.ToInt32(row["AuthorID"]),
                CategoryID = Convert.ToInt32(row["CategoryID"]),
                Price = Convert.ToDecimal(row["Price"]),
                Stock = Convert.ToInt32(row["Stock"]),
                PublishYear = row["PublishYear"] == DBNull.Value ? null : Convert.ToInt32(row["PublishYear"])
            });
        }
        return list;
    }

    public static void Add(Book book)
    {
        string sql = @"INSERT INTO Books (Title, AuthorID, CategoryID, Price, Stock, PublishYear)
                       VALUES (@title, @author, @category, @price, @stock, @year)";

        Database.Execute(sql,
            Database.Param("@title", book.Title),
            Database.Param("@author", book.AuthorID),
            Database.Param("@category", book.CategoryID),
            Database.Param("@price", book.Price),
            Database.Param("@stock", book.Stock),
            Database.Param("@year", book.PublishYear));
    }

    public static void Update(Book book)
    {
        string sql = @"UPDATE Books
                       SET Title = @title, AuthorID = @author, CategoryID = @category,
                           Price = @price, Stock = @stock, PublishYear = @year
                       WHERE BookID = @id";

        Database.Execute(sql,
            Database.Param("@title", book.Title),
            Database.Param("@author", book.AuthorID),
            Database.Param("@category", book.CategoryID),
            Database.Param("@price", book.Price),
            Database.Param("@stock", book.Stock),
            Database.Param("@year", book.PublishYear),
            Database.Param("@id", book.BookID));
    }

    // throws SqlException (number 547) if the book has orders
    public static void Delete(int bookId)
    {
        Database.Execute("DELETE FROM Books WHERE BookID = @id", Database.Param("@id", bookId));
    }

    public static int Count()
    {
        return Convert.ToInt32(Database.Scalar("SELECT COUNT(*) FROM Books"));
    }

    // books with 5 or less copies
    public static DataTable GetLowStock()
    {
        return Database.GetTable("SELECT Title, Stock FROM Books WHERE Stock <= 5 ORDER BY Stock, Title");
    }
}
