using CafeApp.Data;
using CafeApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CafeApp.Functions;

public class ProductsFunctions(AppDbContext db)
{
    [Authorize]
    [Function("GetProducts")]
    public async Task<IActionResult> GetProducts(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products")] HttpRequestData req)
    {
        var includeInactiveStr = req.Query["includeInactive"] ?? "false";
        var includeInactive = bool.TryParse(includeInactiveStr, out var val) && val;

        var products = await db.Products
            .Where(p => includeInactive || p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return new OkObjectResult(products);
    }

    [Authorize]
    [Function("GetProduct")]
    public async Task<IActionResult> GetProduct(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/{id}")] HttpRequestData req,
        int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product == null)
            return new NotFoundResult();

        return new OkObjectResult(product);
    }

    [Authorize]
    [Function("CreateProduct")]
    public async Task<IActionResult> CreateProduct(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "products")] HttpRequestData req)
    {
        JsonDocument? body = null;
        try { body = await JsonDocument.ParseAsync(req.Body); } catch { }

        var name = body?.RootElement.GetProperty("name").GetString();
        var priceStr = body?.RootElement.GetProperty("price").GetRawText() ?? "0";
        var price = decimal.TryParse(priceStr, out var p) ? p : 0m;

        if (string.IsNullOrWhiteSpace(name) || price < 0.01m)
            return new BadRequestObjectResult("Name required and price must be >= 0.01");

        var existing = await db.Products.FirstOrDefaultAsync(p => p.Name == name);
        if (existing != null)
            return new BadRequestObjectResult("Product name must be unique");

        var product = new Product
        {
            Name = name,
            Price = price,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Products.Add(product);
        await db.SaveChangesAsync();

        return new CreatedResult($"/api/products/{product.Id}", product);
    }

    [Authorize]
    [Function("UpdateProduct")]
    public async Task<IActionResult> UpdateProduct(
        [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "products/{id}")] HttpRequestData req,
        int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product == null)
            return new NotFoundResult();

        JsonDocument? body = null;
        try { body = await JsonDocument.ParseAsync(req.Body); } catch { }

        if (body != null && body.RootElement.TryGetProperty("price", out var priceElem))
        {
            if (decimal.TryParse(priceElem.GetRawText(), out var newPrice))
                product.Price = newPrice;
        }

        if (body != null && body.RootElement.TryGetProperty("isActive", out var isActiveElem))
        {
            if (bool.TryParse(isActiveElem.GetRawText(), out var isActive))
                product.IsActive = isActive;
        }

        await db.SaveChangesAsync();
        return new OkObjectResult(product);
    }
}
