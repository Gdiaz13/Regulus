using System.Net.Http.Json;
using System.Text.Json;
using api.Contracts;

namespace api.Services;

// Calls RegulasCoreAI (the commander AI) and nothing else. Keeping all AI HTTP
// talk in this one client means the rest of the backend never touches the AI
// services directly.
public sealed class RegulasAiClient
{
    // Web defaults give camelCase + case-insensitive matching for the Python JSON.
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;

    public RegulasAiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AiOverview?> PredictAsync(IReadOnlyList<AiPredictRequest> requests)
    {
        using var response = await _httpClient.PostAsJsonAsync("predict", requests, JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AiOverview>(JsonOptions);
    }

    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            using var response = await _httpClient.GetAsync("health");
            return response.IsSuccessStatusCode;
        }
        catch (Exception exception) when (IsAiException(exception))
        {
            return false;
        }
    }

    public static bool IsAiException(Exception exception)
    {
        return exception is HttpRequestException or TaskCanceledException;
    }
}
