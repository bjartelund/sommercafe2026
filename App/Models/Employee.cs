namespace CafeApp.Models;

public class Employee
{
    public int Id { get; set; }
    public required string EntraObjectId { get; set; }
    public required string DisplayName { get; set; }
    public required string Email { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
