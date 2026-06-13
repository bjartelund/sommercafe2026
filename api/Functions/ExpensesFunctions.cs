using CafeApp.Data;
using CafeApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace CafeApp.Functions;

public class ExpensesFunctions(AppDbContext db)
{
    [Authorize]
    [Function("CreateExpense")]
    public async Task<IActionResult> CreateExpense(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "expenses")] HttpRequestData req,
        ClaimsPrincipal claimsPrincipal)
    {
        var objectId = claimsPrincipal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
            ?? claimsPrincipal.FindFirst("oid")?.Value;

        var employee = await db.Employees.FirstOrDefaultAsync(e => e.EntraObjectId == objectId);
        if (employee == null)
            return new BadRequestObjectResult("Employee not found");

        JsonDocument? body = null;
        try { body = await JsonDocument.ParseAsync(req.Body); } catch { }

        var description = body?.RootElement.GetProperty("description").GetString() ?? "";
        var amount = 0m;
        if (body?.RootElement.TryGetProperty("amount", out var amountElem) ?? false)
            decimal.TryParse(amountElem.GetRawText(), out amount);

        var expenseDate = DateTime.UtcNow.Date;
        if (body?.RootElement.TryGetProperty("expenseDate", out var dateElem) ?? false)
            DateTime.TryParse(dateElem.GetString() ?? "", out expenseDate);

        if (string.IsNullOrWhiteSpace(description) || amount <= 0 || expenseDate > DateTime.UtcNow.Date)
            return new BadRequestObjectResult("Description required, amount > 0, date not future");

        var expense = new Expense
        {
            EmployeeId = employee.Id,
            Description = description,
            Amount = amount,
            ExpenseDate = expenseDate,
            CreatedAt = DateTime.UtcNow
        };

        db.Expenses.Add(expense);
        await db.SaveChangesAsync();

        return new CreatedResult($"/api/expenses/{expense.Id}", expense);
    }

    [Authorize]
    [Function("GetExpenses")]
    public async Task<IActionResult> GetExpenses(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "expenses")] HttpRequestData req,
        ClaimsPrincipal claimsPrincipal)
    {
        var objectId = claimsPrincipal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
            ?? claimsPrincipal.FindFirst("oid")?.Value;

        var employee = await db.Employees.FirstOrDefaultAsync(e => e.EntraObjectId == objectId);
        if (employee == null)
            return new BadRequestObjectResult("Employee not found");

        var expenses = await db.Expenses
            .Where(e => e.EmployeeId == employee.Id)
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync();

        return new OkObjectResult(expenses);
    }

    [Authorize]
    [Function("UpdateExpense")]
    public async Task<IActionResult> UpdateExpense(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "expenses/{id}")] HttpRequestData req,
        int id)
    {
        var expense = await db.Expenses.FindAsync(id);
        if (expense == null)
            return new NotFoundResult();

        JsonDocument? body = null;
        try { body = await JsonDocument.ParseAsync(req.Body); } catch { }

        if (body?.RootElement.TryGetProperty("amount", out var amountElem) ?? false)
            if (decimal.TryParse(amountElem.GetRawText(), out var amt))
                expense.Amount = amt;

        expense.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return new OkObjectResult(expense);
    }

    [Authorize]
    [Function("DeleteExpense")]
    public async Task<IActionResult> DeleteExpense(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "expenses/{id}")] HttpRequestData req,
        int id)
    {
        var expense = await db.Expenses.FindAsync(id);
        if (expense == null)
            return new NotFoundResult();

        db.Expenses.Remove(expense);
        await db.SaveChangesAsync();

        return new OkResult();
    }
}
