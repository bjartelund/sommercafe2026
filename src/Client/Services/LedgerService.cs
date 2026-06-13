using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Client.Services;

public class LedgerEntryResponse
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("entityId")]
    public int EntityId { get; set; }

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
}

public class LedgerResponseData
{
    [JsonPropertyName("entries")]
    public List<LedgerEntryResponse> Entries { get; set; } = [];

    [JsonPropertyName("totalRevenue")]
    public decimal TotalRevenue { get; set; }

    [JsonPropertyName("totalExpenses")]
    public decimal TotalExpenses { get; set; }

    [JsonPropertyName("netBalance")]
    public decimal NetBalance { get; set; }
}

public class LedgerService(HttpClient httpClient)
{
    public async Task<LedgerResponseData?> GetLedgerAsync(DateTime? from = null, DateTime? to = null)
    {
        try
        {
            var fromDate = from ?? DateTime.UtcNow.AddMonths(-1);
            var toDate = to ?? DateTime.UtcNow;
            var response = await httpClient.GetAsync($"api/ledger?from={fromDate:O}&to={toDate:O}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<LedgerResponseData>(json);
            }
        }
        catch { }
        return null;
    }
}
