using System.Net.Http.Json;
using System.Text.Json;
using api.Contracts;

namespace api.Services;

// Training traffic targets the one real specialist directly. Prediction traffic
// continues to flow through RegulasCoreAI and the manager hierarchy.
public sealed class StockTechAiClient
{
    public const long MaxResponseBytes = 512 * 1024;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;

    public StockTechAiClient(HttpClient httpClient)
    {
        httpClient.MaxResponseContentBufferSize = MaxResponseBytes;
        _httpClient = httpClient;
    }

    public async Task<AiTrainResponse> TrainAsync(AiTrainRequest request, CancellationToken token)
    {
        using var response = await _httpClient.PostAsJsonAsync("train", request, JsonOptions, token);
        response.EnsureSuccessStatusCode();
        return await ReadTrainingAsync(response, token);
    }

    private static async Task<AiTrainResponse> ReadTrainingAsync(
        HttpResponseMessage response, CancellationToken token)
    {
        try
        {
            var training = await response.Content.ReadFromJsonAsync<AiTrainResponse>(JsonOptions, token)
                ?? throw new InvalidOperationException("StockTechAI returned an empty training response.");
            return StockTechTrainingProtocol.Validate(training);
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("StockTechAI returned malformed training JSON.", exception);
        }
    }
}
