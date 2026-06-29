using System.Net.Http.Json;
using System.Text.Json;
using api.Contracts;

namespace api.Services;

// Calls the separate StockTradingAgentsAI service. This keeps the research
// engine outside the C# API while still making the backend the browser gateway.
public sealed class TradingAgentsClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;

    public TradingAgentsClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<StockTradingAgentsResponse?> AnalyzeStockAsync(StockTradingAgentsRequest request)
    {
        using var response = await _httpClient.PostAsJsonAsync("analyze-stock", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<StockTradingAgentsResponse>(JsonOptions);
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

    public async Task<TradingAgentsModelInfoResponse?> ModelInfoAsync()
    {
        using var response = await _httpClient.GetAsync("model-info");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TradingAgentsModelInfoResponse>(JsonOptions);
    }

    public static bool IsAiException(Exception exception)
    {
        return exception is HttpRequestException or TaskCanceledException;
    }
}
