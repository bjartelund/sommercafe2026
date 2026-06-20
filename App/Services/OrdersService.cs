using System.Text.Json.Serialization;
using CafeApp.Data;
using CafeApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services;

public class OrderLineResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("orderId")]
    public int OrderId { get; set; }

    [JsonPropertyName("productId")]
    public int ProductId { get; set; }

    [JsonPropertyName("productName")]
    public string? ProductName { get; set; }

    [JsonPropertyName("unitPrice")]
    public decimal UnitPrice { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("lineTotal")]
    public decimal LineTotal { get; set; }
}

public class OrderResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("employeeId")]
    public int EmployeeId { get; set; }

    [JsonPropertyName("placedAt")]
    public DateTime PlacedAt { get; set; }

    [JsonPropertyName("totalAmount")]
    public decimal TotalAmount { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("orderLines")]
    public List<OrderLineResponse> OrderLines { get; set; } = [];
}

public class OrdersService(IDbContextFactory<AppDbContext> dbFactory, EmployeeService employeeService)
{
    public async Task<OrderResponse?> CreateOrderAsync(List<(int ProductId, int Quantity)> lines, string? notes = null)
    {
        if (lines.Count == 0)
            return null;

        await using var db = await dbFactory.CreateDbContextAsync();
        var employeeId = await employeeService.GetCurrentEmployeeIdAsync();
        if (employeeId <= 0)
            return null;

        var order = new Order
        {
            EmployeeId = employeeId,
            PlacedAt = DateTime.UtcNow,
            Notes = notes
        };

        decimal totalAmount = 0;

        foreach (var line in lines)
        {
            if (line.Quantity <= 0)
                continue;

            var product = await db.Products.FirstOrDefaultAsync(p => p.Id == line.ProductId && p.IsActive);
            if (product == null)
                return null;

            var orderLine = new OrderLine
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = line.Quantity,
                LineTotal = product.Price * line.Quantity
            };

            totalAmount += orderLine.LineTotal;
            order.OrderLines.Add(orderLine);
        }

        if (order.OrderLines.Count == 0)
            return null;

        order.TotalAmount = totalAmount;
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        return await GetOrderAsync(order.Id);
    }

    public async Task<List<OrderResponse>> GetOrdersAsync(DateTime? from = null, DateTime? to = null)
    {
        var fromDate = from ?? DateTime.UtcNow.AddMonths(-1);
        var toDate = to ?? DateTime.UtcNow;

        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Orders
            .Where(o => o.PlacedAt >= fromDate && o.PlacedAt <= toDate)
            .Include(o => o.OrderLines)
            .OrderByDescending(o => o.PlacedAt)
            .Select(o => Map(o))
            .ToListAsync();
    }

    public async Task<OrderResponse?> GetOrderAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var order = await db.Orders
            .Include(o => o.OrderLines)
            .FirstOrDefaultAsync(o => o.Id == id);

        return order == null ? null : Map(order);
    }

    private static OrderResponse Map(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            EmployeeId = order.EmployeeId,
            PlacedAt = order.PlacedAt,
            TotalAmount = order.TotalAmount,
            Notes = order.Notes,
            OrderLines = order.OrderLines
                .Select(ol => new OrderLineResponse
                {
                    Id = ol.Id,
                    OrderId = ol.OrderId,
                    ProductId = ol.ProductId,
                    ProductName = ol.ProductName,
                    UnitPrice = ol.UnitPrice,
                    Quantity = ol.Quantity,
                    LineTotal = ol.LineTotal
                })
                .ToList()
        };
    }
}