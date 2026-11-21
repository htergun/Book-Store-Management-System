namespace BookStoreManagement.Models;

public class Book
{
    public int BookID { get; set; }
    public string Title { get; set; } = "";
    public int AuthorID { get; set; }
    public int CategoryID { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int? PublishYear { get; set; }

    // text shown in combo boxes
    public override string ToString() => Title + "  (stock: " + Stock + ")";
}
