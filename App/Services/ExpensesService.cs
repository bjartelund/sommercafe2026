using System.Text.Json.Serialization;
using CafeApp.Data;
using CafeApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services;

public class ExpenseResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("employeeId")]
    public int EmployeeId { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("expenseDate")]
    public DateTime ExpenseDate { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }
}

public class ExpensesService(IDbContextFactory<AppDbContext> dbFactory, EmployeeService employeeService)
{
    public async Task<List<ExpenseResponse>> GetExpensesAsync(DateTime? from = null, DateTime? to = null)
    {
        var fromDate = from ?? DateTime.UtcNow.AddMonths(-1);
        var toDate = to ?? DateTime.UtcNow.AddDays(1);

        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Expenses
            .Where(e => e.ExpenseDate >= fromDate && e.ExpenseDate <= toDate)
            .OrderByDescending(e => e.ExpenseDate)
            .Select(e => Map(e))
            .ToListAsync();
    }

    public async Task<ExpenseResponse?> CreateExpenseAsync(string description, decimal amount, DateTime expenseDate)
    {
        if (string.IsNullOrWhiteSpace(description) || amount < 0.01m)
            return null;

        await using var db = await dbFactory.CreateDbContextAsync();
        var employeeId = await employeeService.GetCurrentEmployeeIdAsync();
        if (employeeId <= 0)
            return null;

        var expense = new Expense
        {
            EmployeeId = employeeId,
            Description = description,
            Amount = amount,
            ExpenseDate = expenseDate,
            CreatedAt = DateTime.UtcNow
        };

        db.Expenses.Add(expense);
        await db.SaveChangesAsync();

        return Map(expense);
    }

    public async Task<ExpenseResponse?> UpdateExpenseAsync(int id, string description, decimal amount, DateTime expenseDate)
    {
        if (string.IsNullOrWhiteSpace(description) || amount < 0.01m)
            return null;

        await using var db = await dbFactory.CreateDbContextAsync();
        var expense = await db.Expenses.FindAsync(id);
        if (expense == null)
            return null;

        expense.Description = description;
        expense.Amount = amount;
        expense.ExpenseDate = expenseDate;
        expense.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Map(expense);
    }

    public async Task<bool> DeleteExpenseAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var expense = await db.Expenses.FindAsync(id);
        if (expense == null)
            return false;

        db.Expenses.Remove(expense);
        await db.SaveChangesAsync();
        return true;
    }

    private static ExpenseResponse Map(Expense expense)
    {
        return new ExpenseResponse
        {
            Id = expense.Id,
            EmployeeId = expense.EmployeeId,
            Description = expense.Description,
            Amount = expense.Amount,
            ExpenseDate = expense.ExpenseDate,
            CreatedAt = expense.CreatedAt,
            UpdatedAt = expense.UpdatedAt
        };
    }
}