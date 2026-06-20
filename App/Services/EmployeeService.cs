using System.Text.Json.Serialization;
using CafeApp.Data;
using CafeApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services;

public class EmployeeResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("entraObjectId")]
    public string? EntraObjectId { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}

public class EmployeeService(IDbContextFactory<AppDbContext> dbFactory)
{
    private const string LocalEmployeeObjectId = "local-dev-employee";

    public async Task<EmployeeResponse?> GetOrCreateEmployeeAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var employee = await db.Employees
            .Where(e => e.IsActive)
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync();

        if (employee == null)
        {
            employee = new Employee
            {
                EntraObjectId = LocalEmployeeObjectId,
                DisplayName = "Cafe Employee",
                Email = "employee@local.dev",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            db.Employees.Add(employee);
            await db.SaveChangesAsync();
        }

        return Map(employee);
    }

    public async Task<int> GetCurrentEmployeeIdAsync()
    {
        var employee = await GetOrCreateEmployeeAsync();
        return employee?.Id ?? 0;
    }

    private static EmployeeResponse Map(Employee employee)
    {
        return new EmployeeResponse
        {
            Id = employee.Id,
            EntraObjectId = employee.EntraObjectId,
            DisplayName = employee.DisplayName,
            Email = employee.Email,
            IsActive = employee.IsActive,
            CreatedAt = employee.CreatedAt
        };
    }
}