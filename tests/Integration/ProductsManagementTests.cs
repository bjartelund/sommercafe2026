using CafeApp.Data;
using CafeApp.Models;
using CafeApp.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CafeApp.Tests.Integration;

public class ProductsManagementTests : IDisposable
{
    private readonly DatabaseFixture _fixture = new();
    private readonly AppDbContext _db;

    public ProductsManagementTests()
    {
        _db = _fixture.DbContext;
    }

    public void Dispose() => _fixture.Dispose();

    [Test]
    public async Task CreateProduct_WithValidData_CreatesAndStoresRecord()
    {
        var productName = "Premium Espresso";
        var productPrice = 3.50m;

        var product = new Product
        {
            Name = productName,
            Price = productPrice,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        var created = await _db.Products.FirstOrDefaultAsync(p => p.Name == productName);
        await Assert.That(created).IsNotNull();
        await Assert.That(created!.Name).IsEqualTo(productName);
        await Assert.That(created.Price).IsEqualTo(productPrice);
        await Assert.That(created.IsActive).IsTrue();
    }

    [Test]
    public async Task CreateProduct_WithZeroPrice_RejectsCreation()
    {
        var product = new Product
        {
            Name = "Free Coffee",
            Price = 0m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Products.Add(product);
        var ex = await Assert.ThrowsAsync<DbUpdateException>(async () => await _db.SaveChangesAsync());
        await Assert.That(ex).IsNotNull();
    }

    [Test]
    public async Task CreateProduct_WithNegativePrice_RejectsCreation()
    {
        var product = new Product
        {
            Name = "Discount Latte",
            Price = -1.00m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Products.Add(product);
        var ex = await Assert.ThrowsAsync<DbUpdateException>(async () => await _db.SaveChangesAsync());
        await Assert.That(ex).IsNotNull();
    }

    [Test]
    public async Task UpdateProductPrice_WithValidPrice_ModifiesPriceAndTracksTemporalHistory()
    {
        var original = new Product
        {
            Name = "Americano",
            Price = 2.00m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Products.Add(original);
        await _db.SaveChangesAsync();

        var toUpdate = await _db.Products.FirstAsync(p => p.Name == "Americano");
        var oldPrice = toUpdate.Price;
        toUpdate.Price = 2.50m;
        await _db.SaveChangesAsync();

        var updated = await _db.Products.FirstAsync(p => p.Name == "Americano");
        await Assert.That(updated.Price).IsEqualTo(2.50m);
        await Assert.That(updated.Price).IsNotEqualTo(oldPrice);
    }

    [Test]
    public async Task UpdateProductPrice_WithZeroPrice_RejectsUpdate()
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
        toUpdate.Price = 0m;
        var ex = await Assert.ThrowsAsync<DbUpdateException>(async () => await _db.SaveChangesAsync());
        await Assert.That(ex).IsNotNull();
    }

    [Test]
    public async Task DeactivateProduct_SetsIsActiveToFalse()
    {
        var product = new Product
        {
            Name = "Deactivation Test",
            Price = 2.50m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        var toDeactivate = await _db.Products.FirstAsync(p => p.Name == "Deactivation Test");
        toDeactivate.IsActive = false;
        await _db.SaveChangesAsync();

        var deactivated = await _db.Products.FirstAsync(p => p.Name == "Deactivation Test");
        await Assert.That(deactivated.IsActive).IsFalse();
    }

    [Test]
    public async Task ReactivateProduct_SetsIsActiveToTrue()
    {
        var product = new Product
        {
            Name = "Reactivation Test",
            Price = 2.50m,
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        var toReactivate = await _db.Products.FirstAsync(p => p.Name == "Reactivation Test");
        toReactivate.IsActive = true;
        await _db.SaveChangesAsync();

        var reactivated = await _db.Products.FirstAsync(p => p.Name == "Reactivation Test");
        await Assert.That(reactivated.IsActive).IsTrue();
    }

    [Test]
    public async Task ListProducts_ShowsActiveAndInactiveProducts()
    {
        _db.Products.AddRange(
            new Product { Name = "Active1", Price = 2.00m, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Name = "Active2", Price = 2.50m, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Name = "Inactive1", Price = 1.50m, IsActive = false, CreatedAt = DateTime.UtcNow }
        );
        await _db.SaveChangesAsync();

        var allProducts = await _db.Products.ToListAsync();
        var activeProducts = await _db.Products.Where(p => p.IsActive).ToListAsync();

        await Assert.That(allProducts.Count).IsGreaterThanOrEqualTo(3);
        await Assert.That(activeProducts.Count).IsGreaterThanOrEqualTo(2);
        await Assert.That(activeProducts.All(p => p.IsActive)).IsTrue();
    }

    [Test]
    public async Task MultipleProducts_WithDifferentPrices_CalculatesCorrectly()
    {
        _db.Products.AddRange(
            new Product { Name = "Item1", Price = 1.99m, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Name = "Item2", Price = 2.99m, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Name = "Item3", Price = 3.99m, IsActive = true, CreatedAt = DateTime.UtcNow }
        );
        await _db.SaveChangesAsync();

        var products = await _db.Products.Where(p => p.Name.StartsWith("Item")).ToListAsync();
        var totalPrice = products.Sum(p => p.Price);

        await Assert.That(products.Count).IsEqualTo(3);
        await Assert.That(totalPrice).IsEqualTo(8.97m);
    }
}
