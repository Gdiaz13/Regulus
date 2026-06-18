using System.Net;

namespace api.Services;

public sealed class FinancialModelingPrepClient
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public FinancialModelingPrepClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<IResult> GetAsync(string path, IReadOnlyDictionary<string, string?> query)
    {
        var apiKey = GetApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return MissingApiKey();
        }
        return await SendAsync(path, query, apiKey);
    }

    private async Task<IResult> SendAsync(string path, IReadOnlyDictionary<string, string?> query, string apiKey)
    {
        using var response = await _httpClient.GetAsync(BuildPath(path, query, apiKey));
        var content = await response.Content.ReadAsStringAsync();
        return ToResult(response, content);
    }

    private string? GetApiKey()
    {
        var configuredKey = _configuration["FinancialModelingPrep:ApiKey"];
        return string.IsNullOrWhiteSpace(configuredKey)
            ? _configuration["FMP_API_KEY"]
            : configuredKey;
    }

    private static IResult ToResult(HttpResponseMessage response, string content)
    {
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/json";
        if (response.IsSuccessStatusCode)
        {
            return Results.Content(content, contentType);
        }
        return Results.Json(new ErrorResponse(ErrorMessage(content)), statusCode: (int)response.StatusCode);
    }

    private static string BuildPath(string path, IReadOnlyDictionary<string, string?> query, string apiKey)
    {
        var pairs = query.Where(HasValue).Select(FormatPair).Append(FormatPair("apikey", apiKey));
        return $"{path}?{string.Join("&", pairs)}";
    }

    private static bool HasValue(KeyValuePair<string, string?> pair)
    {
        return !string.IsNullOrWhiteSpace(pair.Value);
    }

    private static string FormatPair(KeyValuePair<string, string?> pair)
    {
        return FormatPair(pair.Key, pair.Value ?? string.Empty);
    }

    private static string FormatPair(string key, string value)
    {
        return $"{WebUtility.UrlEncode(key)}={WebUtility.UrlEncode(value)}";
    }

    private static IResult MissingApiKey()
    {
        return Results.Problem("Missing Financial Modeling Prep API key.", statusCode: 500);
    }

    private static string ErrorMessage(string content)
    {
        return string.IsNullOrWhiteSpace(content) ? "Market data request failed." : content;
    }

    private sealed record ErrorResponse(string Message);
}
