using System.Text.Json.Serialization;
using CafeApp.Data;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services;

public class LedgerEntryResponse
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("entityId")]
    public int EntityId { get; set; }

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
}

public class LedgerResponseData
{
    [JsonPropertyName("entries")]
    public List<LedgerEntryResponse> Entries { get; set; } = [];

    [JsonPropertyName("totalRevenue")]
    public decimal TotalRevenue { get; set; }

    [JsonPropertyName("totalExpenses")]
    public decimal TotalExpenses { get; set; }

    [JsonPropertyName("netBalance")]
    public decimal NetBalance { get; set; }
}

public class LedgerService(IDbContextFactory<AppDbContext> dbFactory)
{
    public async Task<LedgerResponseData> GetLedgerAsync(DateTime? from = null, DateTime? to = null)
    {
        var fromDate = from ?? DateTime.UtcNow.AddMonths(-1);
        var toDate = to ?? DateTime.UtcNow;

        await using var db = await dbFactory.CreateDbContextAsync();

        var orderEntries = await db.Orders
            .Where(o => o.PlacedAt >= fromDate && o.PlacedAt <= toDate)
            .Select(o => new LedgerEntryResponse
            {
                Type = "Order",
                EntityId = o.Id,
                Date = o.PlacedAt,
                Description = o.Notes,
                Amount = o.TotalAmount
            })
            .ToListAsync();

        var expenseEntries = await db.Expenses
            .Where(e => e.ExpenseDate >= fromDate && e.ExpenseDate <= toDate)
            .Select(e => new LedgerEntryResponse
            {
                Type = "Expense",
                EntityId = e.Id,
                Date = e.ExpenseDate,
                Description = e.Description,
                Amount = -e.Amount
            })
            .ToListAsync();

        var entries = orderEntries
            .Concat(expenseEntries)
            .OrderByDescending(e => e.Date)
            .ToList();

        var totalRevenue = orderEntries.Sum(e => e.Amount);
        var totalExpenses = expenseEntries.Sum(e => Math.Abs(e.Amount));

        return new LedgerResponseData
        {
            Entries = entries,
            TotalRevenue = totalRevenue,
            TotalExpenses = totalExpenses,
            NetBalance = totalRevenue - totalExpenses
        };
    }
}