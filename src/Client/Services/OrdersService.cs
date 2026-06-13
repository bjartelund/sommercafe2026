using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Client.Services;

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

public class OrdersService(HttpClient httpClient)
{
    public async Task<OrderResponse?> CreateOrderAsync(List<(int ProductId, int Quantity)> lines, string? notes = null)
    {
        try
        {
            var request = new
            {
                lines = lines.Select(l => new { productId = l.ProductId, quantity = l.Quantity }).ToList(),
                notes
            };
            var response = await httpClient.PostAsJsonAsync("api/orders", request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<OrderResponse>(json);
            }
        }
        catch { }
        return null;
    }

    public async Task<List<OrderResponse>> GetOrdersAsync(DateTime? from = null, DateTime? to = null)
    {
        try
        {
            var fromDate = from ?? DateTime.UtcNow.AddMonths(-1);
            var toDate = to ?? DateTime.UtcNow;
            var response = await httpClient.GetAsync($"api/orders?from={fromDate:O}&to={toDate:O}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<OrderResponse>>(json) ?? [];
            }
        }
        catch { }
        return [];
    }

    public async Task<OrderResponse?> GetOrderAsync(int id)
    {
        try
        {
            var response = await httpClient.GetAsync($"api/orders/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<OrderResponse>(json);
            }
        }
        catch { }
        return null;
    }
}
