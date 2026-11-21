using System.Data;
using BookStoreManagement.Models;

namespace BookStoreManagement.Data;

// Authors and categories are only read (they fill the combo boxes).
public static class LookupRepository
{
    public static List<Author> GetAuthors()
    {
        DataTable table = Database.GetTable(
            "SELECT AuthorID, FirstName, LastName, Country FROM Authors ORDER BY FirstName, LastName");

        List<Author> list = new List<Author>();
        foreach (DataRow row in table.Rows)
        {
            list.Add(new Author
            {
                AuthorID = Convert.ToInt32(row["AuthorID"]),
                FirstName = row["FirstName"].ToString() ?? "",
                LastName = row["LastName"].ToString() ?? "",
                Country = row["Country"].ToString() ?? ""
            });
        }
        return list;
    }

    public static List<Category> GetCategories()
    {
        DataTable table = Database.GetTable(
            "SELECT CategoryID, CategoryName FROM Categories ORDER BY CategoryName");

        List<Category> list = new List<Category>();
        foreach (DataRow row in table.Rows)
        {
            list.Add(new Category
            {
                CategoryID = Convert.ToInt32(row["CategoryID"]),
                CategoryName = row["CategoryName"].ToString() ?? ""
            });
        }
        return list;
    }
}
