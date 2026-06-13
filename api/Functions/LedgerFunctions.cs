using CafeApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;

namespace CafeApp.Functions;

public class LedgerEntry
{
    public required string Type { get; set; }
    public int EntityId { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
}

public class LedgerResponse
{
    public List<LedgerEntry> Entries { get; set; } = [];
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetBalance { get; set; }
}

public class LedgerFunctions(AppDbContext db)
{
    [Authorize]
    [Function("GetLedger")]
    public async Task<IActionResult> GetLedger(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ledger")] HttpRequestData req)
    {
        var fromDate = DateTime.TryParse(req.Query["from"], out var f) ? f : DateTime.UtcNow.AddMonths(-1);
        var toDate = DateTime.TryParse(req.Query["to"], out var t) ? t : DateTime.UtcNow;

        var entries = new List<LedgerEntry>();

        var orders = await db.Orders
            .Where(o => o.PlacedAt >= fromDate && o.PlacedAt <= toDate)
            .ToListAsync();

        foreach (var order in orders)
        {
            entries.Add(new LedgerEntry
            {
                Type = "Order",
                EntityId = order.Id,
                Date = order.PlacedAt,
                Amount = order.TotalAmount
            });
        }

        var expenses = await db.Expenses
            .Where(e => e.ExpenseDate >= fromDate && e.ExpenseDate <= toDate)
            .ToListAsync();

        foreach (var expense in expenses)
        {
            entries.Add(new LedgerEntry
            {
                Type = "Expense",
                EntityId = expense.Id,
                Date = expense.ExpenseDate,
                Description = expense.Description,
                Amount = -expense.Amount
            });
        }

        entries = entries.OrderByDescending(e => e.Date).ToList();

        var totalRevenue = orders.Sum(o => o.TotalAmount);
        var totalExpenses = expenses.Sum(e => e.Amount);
        var netBalance = totalRevenue - totalExpenses;

        return new OkObjectResult(new LedgerResponse
        {
            Entries = entries,
            TotalRevenue = totalRevenue,
            TotalExpenses = totalExpenses,
            NetBalance = netBalance
        });
    }
}
