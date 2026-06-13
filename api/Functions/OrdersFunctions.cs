using CafeApp.Data;
using CafeApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Json;

namespace CafeApp.Functions;

public class OrdersFunctions(AppDbContext db, ILogger<OrdersFunctions> logger)
{
    [Authorize]
    [Function("CreateOrder")]
    public async Task<IActionResult> CreateOrder(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders")] HttpRequestData req,
        ClaimsPrincipal claimsPrincipal)
    {
        logger.LogInformation("CreateOrder invoked");
        // Try to get employee from claims, fallback to last created employee (for local dev with SWA mock auth)
        Employee employee = null;

        if (claimsPrincipal != null)
        {
            var objectId = claimsPrincipal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
                ?? claimsPrincipal.FindFirst("oid")?.Value;
            employee = await db.Employees.FirstOrDefaultAsync(e => e.EntraObjectId == objectId);
        }

        // Fallback: use the last created employee (for local development)
        if (employee == null)
        {
            employee = await db.Employees.OrderByDescending(e => e.CreatedAt).FirstOrDefaultAsync();
        }

        if (employee == null)
            return new BadRequestObjectResult("No employee found. Please create an employee first.");

        JsonDocument? body = null;
        try { body = await JsonDocument.ParseAsync(req.Body); } catch { }

        if (body == null || !body.RootElement.TryGetProperty("lines", out var linesElem) || linesElem.GetArrayLength() == 0)
            return new BadRequestObjectResult("Order must contain at least one line");

        var order = new Order
        {
            EmployeeId = employee.Id,
            PlacedAt = DateTime.UtcNow,
            Notes = body.RootElement.TryGetProperty("notes", out var notesElem) ? notesElem.GetString() : null
        };

        decimal totalAmount = 0m;

        foreach (var line in linesElem.EnumerateArray())
        {
            int productId = line.GetProperty("productId").GetInt32();
            int quantity = line.GetProperty("quantity").GetInt32();

            var product = await db.Products.FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);
            if (product == null)
                return new BadRequestObjectResult($"Product {productId} not found or inactive");

            var orderLine = new OrderLine
            {
                ProductId = productId,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = quantity,
                LineTotal = product.Price * quantity
            };

            order.OrderLines.Add(orderLine);
            totalAmount += orderLine.LineTotal;
        }

        order.TotalAmount = totalAmount;
        db.Orders.Add(order);
        logger.LogInformation("Saving order with {LineCount} lines and total {Total}", order.OrderLines.Count, order.TotalAmount);
        await db.SaveChangesAsync();
        logger.LogInformation("Order {OrderId} saved successfully", order.Id);

        return new CreatedResult($"/api/orders/{order.Id}", order);
    }

    [Authorize]
    [Function("GetOrders")]
    public async Task<IActionResult> GetOrders(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders")] HttpRequestData req)
    {
        var fromDate = DateTime.TryParse(req.Query["from"] ?? "", out var f) ? f : DateTime.UtcNow.AddMonths(-1);
        var toDate = DateTime.TryParse(req.Query["to"] ?? "", out var t) ? t : DateTime.UtcNow;

        var orders = await db.Orders
            .Where(o => o.PlacedAt >= fromDate && o.PlacedAt <= toDate)
            .Include(o => o.OrderLines)
            .OrderByDescending(o => o.PlacedAt)
            .ToListAsync();

        return new OkObjectResult(orders);
    }

    [Authorize]
    [Function("GetOrder")]
    public async Task<IActionResult> GetOrder(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders/{id}")] HttpRequestData req,
        int id)
    {
        var order = await db.Orders
            .Include(o => o.OrderLines)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return new NotFoundResult();

        return new OkObjectResult(order);
    }
}
