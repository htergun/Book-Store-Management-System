namespace BookStoreManagement.Models;

public class Customer
{
    public int CustomerID { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string City { get; set; } = "";

    public string FullName => FirstName + " " + LastName;

    public override string ToString() => FullName;
}
