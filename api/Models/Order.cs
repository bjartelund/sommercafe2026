namespace CafeApp.Models;

public class Order
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateTime PlacedAt { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }

    public virtual Employee? Employee { get; set; }
    public virtual ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
}
