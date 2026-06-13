using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Client.Services;

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

public class ProductsService(HttpClient httpClient)
{
    public async Task<List<ProductResponse>> GetActiveProductsAsync()
    {
        try
        {
            var response = await httpClient.GetAsync("api/products");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<ProductResponse>>(json) ?? [];
            }
        }
        catch { }
        return [];
    }

    public async Task<ProductResponse?> GetProductAsync(int id)
    {
        try
        {
            var response = await httpClient.GetAsync($"api/products/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ProductResponse>(json);
            }
        }
        catch { }
        return null;
    }

    public async Task<ProductResponse?> CreateProductAsync(string name, decimal price)
    {
        try
        {
            var request = new { name, price };
            var response = await httpClient.PostAsJsonAsync("api/products", request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ProductResponse>(json);
            }
        }
        catch { }
        return null;
    }

    public async Task<ProductResponse?> UpdateProductAsync(int id, decimal? price, bool? isActive)
    {
        try
        {
            var request = new { price, isActive };
            var response = await httpClient.PatchAsJsonAsync($"api/products/{id}", request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ProductResponse>(json);
            }
        }
        catch { }
        return null;
    }
}
