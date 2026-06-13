using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Client.Services;

public class WorkSessionResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("employeeId")]
    public int EmployeeId { get; set; }

    [JsonPropertyName("sessionDate")]
    public DateTime SessionDate { get; set; }

    [JsonPropertyName("startTime")]
    public TimeSpan StartTime { get; set; }

    [JsonPropertyName("endTime")]
    public TimeSpan EndTime { get; set; }

    [JsonPropertyName("durationMinutes")]
    public int DurationMinutes { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("employee")]
    public EmployeeResponse? Employee { get; set; }
}

public class WorkSessionsService(HttpClient httpClient)
{
    public async Task<List<WorkSessionResponse>> GetWorkSessionsAsync(DateTime? from = null, DateTime? to = null)
    {
        try
        {
            var fromDate = from ?? DateTime.UtcNow.AddMonths(-1);
            var toDate = to ?? DateTime.UtcNow.AddDays(1);
            var url = $"api/work-sessions?from={fromDate:O}&to={toDate:O}";
            var response = await httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"WorkSessions API status: {response.StatusCode}");
            Console.WriteLine($"WorkSessions API response (first 500 chars): {content.Substring(0, Math.Min(500, content.Length))}");

            if (response.IsSuccessStatusCode && !content.StartsWith("<"))
            {
                return JsonSerializer.Deserialize<List<WorkSessionResponse>>(content) ?? [];
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WorkSessions error: {ex.GetType().Name}: {ex.Message}");
        }
        return [];
    }

    public async Task<WorkSessionResponse?> CreateWorkSessionAsync(DateTime sessionDate, TimeSpan startTime, TimeSpan endTime, string? notes = null)
    {
        try
        {
            var request = new { sessionDate, startTime, endTime, notes };
            var response = await httpClient.PostAsJsonAsync("api/work-sessions", request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<WorkSessionResponse>(json);
            }
        }
        catch { }
        return null;
    }

    public async Task<WorkSessionResponse?> UpdateWorkSessionAsync(int id, DateTime sessionDate, TimeSpan startTime, TimeSpan endTime, string? notes = null)
    {
        try
        {
            var request = new { sessionDate, startTime, endTime, notes };
            var response = await httpClient.PutAsJsonAsync($"api/work-sessions/{id}", request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<WorkSessionResponse>(json);
            }
        }
        catch { }
        return null;
    }

    public async Task<bool> DeleteWorkSessionAsync(int id)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"api/work-sessions/{id}");
            return response.IsSuccessStatusCode;
        }
        catch { }
        return false;
    }
}
