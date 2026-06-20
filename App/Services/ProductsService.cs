using System.Text.Json.Serialization;
using CafeApp.Data;
using CafeApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services;

public class ProductResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}

public class ProductPriceHistoryEntryResponse
{
    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("sysStartTime")]
    public DateTime SysStartTime { get; set; }

    [JsonPropertyName("sysEndTime")]
    public DateTime SysEndTime { get; set; }
}

public class ProductsService(IDbContextFactory<AppDbContext> dbFactory)
{
    public async Task<List<ProductResponse>> GetActiveProductsAsync(bool includeInactive = false)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Products
            .Where(p => includeInactive || p.IsActive)
            .OrderBy(p => p.Name)
            .Select(p => Map(p))
            .ToListAsync();
    }

    public async Task<ProductResponse?> GetProductAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var product = await db.Products.FindAsync(id);
        return product == null ? null : Map(product);
    }

    public async Task<ProductResponse?> CreateProductAsync(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name) || price < 0.01m)
            return null;

        await using var db = await dbFactory.CreateDbContextAsync();

        var exists = await db.Products.AnyAsync(p => p.Name == name);
        if (exists)
            return null;

        var product = new Product
        {
            Name = name,
            Price = price,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Products.Add(product);
        await db.SaveChangesAsync();

        return Map(product);
    }

    public async Task<ProductResponse?> UpdateProductAsync(int id, decimal? price, bool? isActive)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var product = await db.Products.FindAsync(id);
        if (product == null)
            return null;

        if (price.HasValue && price.Value >= 0.01m)
            product.Price = price.Value;

        if (isActive.HasValue)
            product.IsActive = isActive.Value;

        await db.SaveChangesAsync();
        return Map(product);
    }

    public async Task<List<ProductPriceHistoryEntryResponse>> GetPriceHistoryAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var exists = await db.Products.AnyAsync(p => p.Id == id);
        if (!exists)
            return [];

        return await db.Products
            .TemporalAll()
            .Where(p => p.Id == id)
            .OrderByDescending(p => p.SysStartTime)
            .Select(p => new ProductPriceHistoryEntryResponse
            {
                Price = p.Price,
                SysStartTime = p.SysStartTime,
                SysEndTime = p.SysEndTime
            })
            .ToListAsync();
    }

    private static ProductResponse Map(Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt
        };
    }
}