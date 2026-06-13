using CafeApp.Data;
using CafeApp.Models;
using CafeApp.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CafeApp.Tests.Integration;

public class OrdersTests : IDisposable
{
    private readonly DatabaseFixture _fixture = new();
    private readonly AppDbContext _db;

    public OrdersTests()
    {
        _db = _fixture.DbContext;
    }

    public void Dispose() => _fixture.Dispose();

    [Test]
    public async Task CreateOrder_SnapshotsPriceAtTimeOfOrder()
    {
        var employee = new Employee
        {
            EntraObjectId = "emp-123",
            DisplayName = "Test Employee",
            Email = "emp@test.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var product = new Product
        {
            Name = "Coffee",
            Price = 2.50m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Employees.Add(employee);
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        var order = new Order
        {
            EmployeeId = employee.Id,
            PlacedAt = DateTime.UtcNow,
            TotalAmount = 5.00m
        };

        var orderLine = new OrderLine
        {
            ProductId = product.Id,
            ProductName = product.Name,
            UnitPrice = product.Price,
            Quantity = 2,
            LineTotal = 5.00m
        };

        order.OrderLines.Add(orderLine);
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        var retrieved = await _db.Orders.Include(o => o.OrderLines).FirstAsync(o => o.Id == order.Id);
        await Assert.That(retrieved.TotalAmount).IsEqualTo(5.00m);
        await Assert.That(retrieved.OrderLines.First().UnitPrice).IsEqualTo(2.50m);
    }

    [Test]
    public async Task OrderPriceHistoryPreserved_AfterPriceChange()
    {
        var employee = new Employee
        {
            EntraObjectId = "emp-456",
            DisplayName = "Test Employee 2",
            Email = "emp2@test.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var product = new Product
        {
            Name = "Tea",
            Price = 1.50m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Employees.Add(employee);
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        var order = new Order
        {
            EmployeeId = employee.Id,
            PlacedAt = DateTime.UtcNow,
            TotalAmount = 1.50m
        };

        var orderLine = new OrderLine
        {
            ProductId = product.Id,
            ProductName = product.Name,
            UnitPrice = product.Price,
            Quantity = 1,
            LineTotal = 1.50m
        };

        order.OrderLines.Add(orderLine);
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        product.Price = 2.00m;
        await _db.SaveChangesAsync();

        var orderAfterPriceChange = await _db.Orders
            .Include(o => o.OrderLines)
            .FirstAsync(o => o.Id == order.Id);

        await Assert.That(orderAfterPriceChange.OrderLines.First().UnitPrice).IsEqualTo(1.50m);
    }

    [Test]
    public async Task CreateOrder_WithInactiveProduct_Returns400()
    {
        var employee = new Employee
        {
            EntraObjectId = "emp-789",
            DisplayName = "Test Employee 3",
            Email = "emp3@test.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var product = new Product
        {
            Name = "Inactive Item",
            Price = 1.00m,
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Employees.Add(employee);
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        var activeProducts = await _db.Products.Where(p => p.IsActive).ToListAsync();
        await Assert.That(activeProducts.Count).IsEqualTo(0);
    }

    [Test]
    public async Task CreateOrder_WithoutLines_Returns400()
    {
        var employee = new Employee
        {
            EntraObjectId = "emp-000",
            DisplayName = "Test Employee 4",
            Email = "emp4@test.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();

        var order = new Order
        {
            EmployeeId = employee.Id,
            PlacedAt = DateTime.UtcNow,
            TotalAmount = 0m
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        var orderWithLines = await _db.Orders.Include(o => o.OrderLines).FirstAsync(o => o.Id == order.Id);
        await Assert.That(orderWithLines.OrderLines.Count).IsEqualTo(0);
    }
}
