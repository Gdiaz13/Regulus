using System.Net.Http.Json;
using System.Text.Json;
using api.Contracts;

namespace api.Services;

// Training traffic targets the one real specialist directly. Prediction traffic
// continues to flow through RegulasCoreAI and the manager hierarchy.
public sealed class StockTechAiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;

    public StockTechAiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AiTrainResponse> TrainAsync(AiTrainRequest request, CancellationToken token)
    {
        using var response = await _httpClient.PostAsJsonAsync("train", request, JsonOptions, token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AiTrainResponse>(JsonOptions, token)
            ?? throw new InvalidOperationException("StockTechAI returned an empty training response.");
    }
}
