namespace BookStoreManagement.Models;

public class Author
{
    public int AuthorID { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Country { get; set; } = "";

    public string FullName => FirstName + " " + LastName;

    public override string ToString() => FullName;
}
