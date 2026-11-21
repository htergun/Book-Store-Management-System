namespace BookStoreManagement.Models;

public class Order
{
    public int OrderID { get; set; }
    public int CustomerID { get; set; }
    public int BookID { get; set; }
    public int Quantity { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Today;
}
