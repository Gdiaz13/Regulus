using System.Net;
using System.Net.Http.Json;
using Regulas.MauiApp.Models;

namespace Regulas.MauiApp.Services;

// MAUI talks only to Regulas.Api. Provider keys and AI services stay server-side.
public sealed class RegulasApiClient : IRegulasApiClient
{
    private readonly HttpClient _httpClient;

    public RegulasApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<ApiClientResult<ApiHealth>> GetHealthAsync(CancellationToken cancellationToken)
    {
        return GetAsync<ApiHealth>("api/health", cancellationToken);
    }

    public async Task<ApiClientResult<IReadOnlyList<PortfolioStock>>> GetPortfolioStocksAsync(CancellationToken token)
    {
        var result = await GetAsync<List<PortfolioStock>>("api/stocks", token);
        return result.Ok && result.Data is not null
            ? ApiClientResult<IReadOnlyList<PortfolioStock>>.Success(result.Data)
            : ApiClientResult<IReadOnlyList<PortfolioStock>>.Failure(result.Message);
    }

    private async Task<ApiClientResult<T>> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        try
        {
            RefreshBaseAddress();
            using var response = await _httpClient.GetAsync(path, cancellationToken);
            return await ToResult<T>(response, cancellationToken);
        }
        catch (Exception exception) when (IsConnectionFailure(exception))
        {
            return ApiClientResult<T>.Failure("Unable to reach Regulas.Api.");
        }
    }

    private void RefreshBaseAddress()
    {
        var nextUri = new Uri(ApiBaseUrl.Current);
        if (_httpClient.BaseAddress != nextUri)
        {
            _httpClient.BaseAddress = nextUri;
        }
    }

    private static async Task<ApiClientResult<T>> ToResult<T>(HttpResponseMessage response, CancellationToken token)
    {
        if (!response.IsSuccessStatusCode)
        {
            return ApiClientResult<T>.Failure(await ErrorMessage(response));
        }
        var value = await response.Content.ReadFromJsonAsync<T>(cancellationToken: token);
        return value is null ? ApiClientResult<T>.Failure("Regulas.Api returned no data.") : ApiClientResult<T>.Success(value);
    }

    private static async Task<string> ErrorMessage(HttpResponseMessage response)
    {
        var message = await response.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(message) ? StatusMessage(response.StatusCode) : message;
    }

    private static string StatusMessage(HttpStatusCode statusCode)
    {
        return $"Regulas.Api returned {(int)statusCode}.";
    }

    private static bool IsConnectionFailure(Exception exception)
    {
        return exception is HttpRequestException or TaskCanceledException;
    }
}
