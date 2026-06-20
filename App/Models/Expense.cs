namespace CafeApp.Models;

public class Expense
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public required string Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
