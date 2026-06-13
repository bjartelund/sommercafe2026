using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Client.Services;

public class EmployeeResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("entraObjectId")]
    public string? EntraObjectId { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}

public class EmployeeService(HttpClient httpClient)
{
    public async Task<EmployeeResponse?> GetOrCreateEmployeeAsync()
    {
        try
        {
            var response = await httpClient.PostAsync("api/employees/me", new StringContent(""));
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<EmployeeResponse>(json);
            }
        }
        catch { }
        return null;
    }
}
