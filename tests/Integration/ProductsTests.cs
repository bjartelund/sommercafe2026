using CafeApp.Data;
using CafeApp.Models;
using CafeApp.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CafeApp.Tests.Integration;

public class ProductsTests : IDisposable
{
    private readonly DatabaseFixture _fixture = new();
    private readonly AppDbContext _db;

    public ProductsTests()
    {
        _db = _fixture.DbContext;
    }

    public void Dispose() => _fixture.Dispose();

    [Test]
    public async Task CreateProduct_StoresRecord()
    {
        var product = new Product
        {
            Name = "Espresso",
            Price = 2.50m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        var retrieved = await _db.Products.FirstOrDefaultAsync(p => p.Name == "Espresso");
        await Assert.That(retrieved).IsNotNull();
        await Assert.That(retrieved!.Price).IsEqualTo(2.50m);
        await Assert.That(retrieved.IsActive).IsTrue();
    }

    [Test]
    public async Task GetActiveProducts_FiltersInactiveProducts()
    {
        _db.Products.AddRange(
            new Product { Name = "Active Coffee", Price = 2.00m, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Name = "Inactive Tea", Price = 1.50m, IsActive = false, CreatedAt = DateTime.UtcNow }
        );
        await _db.SaveChangesAsync();

        var active = await _db.Products.Where(p => p.IsActive).ToListAsync();
        await Assert.That(active.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(active.All(p => p.IsActive)).IsTrue();
    }

    [Test]
    public async Task UpdateProductPrice_ModifiesPrice()
    {
        var product = new Product
        {
            Name = "Cappuccino",
            Price = 3.00m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        var toUpdate = await _db.Products.FirstAsync(p => p.Name == "Cappuccino");
        toUpdate.Price = 3.50m;
        await _db.SaveChangesAsync();

        var updated = await _db.Products.FirstAsync(p => p.Name == "Cappuccino");
        await Assert.That(updated.Price).IsEqualTo(3.50m);
    }

    [Test]
    public async Task DeactivateProduct_SetsIsActiveToFalse()
    {
        var product = new Product
        {
            Name = "Muffin",
            Price = 2.50m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        var toDeactivate = await _db.Products.FirstAsync(p => p.Name == "Muffin");
        toDeactivate.IsActive = false;
        await _db.SaveChangesAsync();

        var deactivated = await _db.Products.FirstAsync(p => p.Name == "Muffin");
        await Assert.That(deactivated.IsActive).IsFalse();
    }
}
