using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Client.Services;

public class ExpenseResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("employeeId")]
    public int EmployeeId { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("expenseDate")]
    public DateTime ExpenseDate { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}

public class ExpensesService(HttpClient httpClient)
{
    public async Task<List<ExpenseResponse>> GetExpensesAsync(DateTime? from = null, DateTime? to = null)
    {
        try
        {
            var fromDate = from ?? DateTime.UtcNow.AddMonths(-1);
            var toDate = to ?? DateTime.UtcNow.AddDays(1);
            var response = await httpClient.GetAsync($"api/expenses?from={fromDate:O}&to={toDate:O}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<ExpenseResponse>>(json) ?? [];
            }
        }
        catch { }
        return [];
    }

    public async Task<ExpenseResponse?> CreateExpenseAsync(string description, decimal amount, DateTime expenseDate)
    {
        try
        {
            var request = new { description, amount, expenseDate };
            var response = await httpClient.PostAsJsonAsync("api/expenses", request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ExpenseResponse>(json);
            }
        }
        catch { }
        return null;
    }

    public async Task<ExpenseResponse?> UpdateExpenseAsync(int id, string description, decimal amount, DateTime expenseDate)
    {
        try
        {
            var request = new { description, amount, expenseDate };
            var response = await httpClient.PutAsJsonAsync($"api/expenses/{id}", request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ExpenseResponse>(json);
            }
        }
        catch { }
        return null;
    }

    public async Task<bool> DeleteExpenseAsync(int id)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"api/expenses/{id}");
            return response.IsSuccessStatusCode;
        }
        catch { }
        return false;
    }
}
