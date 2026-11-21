namespace BookStoreManagement.Models;

public class Category
{
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = "";

    public override string ToString() => CategoryName;
}
