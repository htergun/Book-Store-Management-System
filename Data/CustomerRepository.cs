using System.Data;
using BookStoreManagement.Models;

namespace BookStoreManagement.Data;

public static class CustomerRepository
{
    public static DataTable GetAll(string search = "")
    {
        string sql = @"SELECT CustomerID, FirstName, LastName, Email, Phone, City
                       FROM Customers
                       WHERE FirstName LIKE @s OR LastName LIKE @s OR Email LIKE @s OR City LIKE @s
                       ORDER BY CustomerID";

        return Database.GetTable(sql, Database.Param("@s", "%" + search + "%"));
    }

    // simple list (used by the order page combo box)
    public static List<Customer> GetList()
    {
        DataTable table = Database.GetTable(
            "SELECT CustomerID, FirstName, LastName, Email, Phone, City FROM Customers ORDER BY FirstName, LastName");

        List<Customer> list = new List<Customer>();
        foreach (DataRow row in table.Rows)
        {
            list.Add(new Customer
            {
                CustomerID = Convert.ToInt32(row["CustomerID"]),
                FirstName = row["FirstName"].ToString() ?? "",
                LastName = row["LastName"].ToString() ?? "",
                Email = row["Email"].ToString() ?? "",
                Phone = row["Phone"].ToString() ?? "",
                City = row["City"].ToString() ?? ""
            });
        }
        return list;
    }

    // throws SqlException (2627 / 2601) if the e-mail already exists
    public static void Add(Customer c)
    {
        string sql = @"INSERT INTO Customers (FirstName, LastName, Email, Phone, City)
                       VALUES (@first, @last, @email, @phone, @city)";

        Database.Execute(sql,
            Database.Param("@first", c.FirstName),
            Database.Param("@last", c.LastName),
            Database.Param("@email", c.Email),
            Database.Param("@phone", c.Phone),
            Database.Param("@city", c.City));
    }

    public static void Update(Customer c)
    {
        string sql = @"UPDATE Customers
                       SET FirstName = @first, LastName = @last, Email = @email, Phone = @phone, City = @city
                       WHERE CustomerID = @id";

        Database.Execute(sql,
            Database.Param("@first", c.FirstName),
            Database.Param("@last", c.LastName),
            Database.Param("@email", c.Email),
            Database.Param("@phone", c.Phone),
            Database.Param("@city", c.City),
            Database.Param("@id", c.CustomerID));
    }

    // throws SqlException (number 547) if the customer has orders
    public static void Delete(int customerId)
    {
        Database.Execute("DELETE FROM Customers WHERE CustomerID = @id", Database.Param("@id", customerId));
    }

    public static int Count()
    {
        return Convert.ToInt32(Database.Scalar("SELECT COUNT(*) FROM Customers"));
    }
}
