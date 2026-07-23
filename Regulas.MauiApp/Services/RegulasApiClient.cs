using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Regulas.MauiApp.Models;

namespace Regulas.MauiApp.Services;

// MAUI talks only to Regulas.Api. Provider keys and AI services stay server-side.
public sealed class RegulasApiClient : IRegulasApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;
    private readonly IAuthTokenStore _tokens;

    public RegulasApiClient(HttpClient httpClient, IAuthTokenStore tokens)
    {
        _httpClient = httpClient;
        _tokens = tokens;
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

    public async Task<ApiClientResult<IReadOnlyList<CompanySearchResult>>> SearchCompaniesAsync(string query, CancellationToken token)
    {
        var result = await GetAsync<List<CompanySearchResult>>(SearchPath(query), token);
        return result.Ok && result.Data is not null
            ? ApiClientResult<IReadOnlyList<CompanySearchResult>>.Success(result.Data)
            : ApiClientResult<IReadOnlyList<CompanySearchResult>>.Failure(result.Message);
    }

    // The provider returns profiles as a one-item list; unwrap it for the detail screen.
    public async Task<ApiClientResult<CompanyProfile>> GetCompanyProfileAsync(string symbol, CancellationToken token)
    {
        var result = await GetAsync<List<CompanyProfile>>(ProfilePath(symbol), token);
        return FirstProfileResult(symbol, result);
    }

    public Task<ApiClientResult<PriceHistoryResponse>> GetPriceHistoryAsync(string symbol, string assetType, int take, CancellationToken token)
    {
        return GetAsync<PriceHistoryResponse>(PriceHistoryPath(symbol, assetType, take), token);
    }

    public Task<ApiClientResult<PriceCaptureResult>> CapturePriceHistoryAsync(string symbol, string assetType, CancellationToken token)
    {
        return PostAsync<PriceCaptureResult>(PriceCapturePath(symbol, assetType), new { }, token);
    }

    // Hand-entered card prices; the API requires a signed-in user and the bearer
    // token is attached like every other call.
    public Task<ApiClientResult<PriceCaptureResult>> RecordManualPriceAsync(string symbol, ManualPriceRequest request, CancellationToken token)
    {
        return PostAsync<PriceCaptureResult>($"api/price-history/{Esc(symbol)}/manual?assetType=TcgCard", request, token);
    }

    public Task<ApiClientResult<MagicCardSearchResponse>> SearchMagicCardsAsync(string query, int pageSize, CancellationToken token)
    {
        return GetAsync<MagicCardSearchResponse>(MagicSearchPath(query, pageSize), token);
    }

    public Task<ApiClientResult<MagicCardDetail>> GetMagicCardAsync(string id, CancellationToken token)
    {
        return GetAsync<MagicCardDetail>($"api/tcg/magic/cards/{Esc(id)}", token);
    }

    public Task<ApiClientResult<AiOverview>> PredictAsync(IReadOnlyList<PredictAssetRequest> assets, CancellationToken token)
    {
        return PostAsync<AiOverview>("api/predict", new PredictBatchRequest(assets), token);
    }

    public async Task<ApiClientResult<IReadOnlyList<PredictionHistoryItem>>> GetPredictionHistoryAsync(int take, CancellationToken token)
    {
        var result = await GetAsync<List<PredictionHistoryItem>>(PredictionHistoryPath(take), token);
        return result.Ok && result.Data is not null
            ? ApiClientResult<IReadOnlyList<PredictionHistoryItem>>.Success(result.Data)
            : ApiClientResult<IReadOnlyList<PredictionHistoryItem>>.Failure(result.Message);
    }

    public async Task<ApiClientResult<IReadOnlyList<ModelAccuracySummary>>> GetPredictionAccuracySummaryAsync(CancellationToken token)
    {
        var result = await GetAsync<List<ModelAccuracySummary>>("api/predict/accuracy/summary", token);
        return result.Ok && result.Data is not null
            ? ApiClientResult<IReadOnlyList<ModelAccuracySummary>>.Success(result.Data)
            : ApiClientResult<IReadOnlyList<ModelAccuracySummary>>.Failure(result.Message);
    }

    public Task<ApiClientResult<PredictionHealth>> GetPredictionHealthAsync(CancellationToken token)
    {
        return GetAsync<PredictionHealth>("api/predict/health", token);
    }

    public Task<ApiClientResult<StockTradingAgentsResponse>> AnalyzeStockWithTradingAgentsAsync(StockTradingAgentsRequest request, CancellationToken token)
    {
        return PostAsync<StockTradingAgentsResponse>("api/trading-agents/stock/analyze", request, token);
    }

    public Task<ApiClientResult<TradingAgentsHealth>> GetTradingAgentsHealthAsync(CancellationToken token)
    {
        return GetAsync<TradingAgentsHealth>("api/trading-agents/stock/health", token);
    }

    public Task<ApiClientResult<TradingAgentsModelInfo>> GetTradingAgentsModelInfoAsync(CancellationToken token)
    {
        return GetAsync<TradingAgentsModelInfo>("api/trading-agents/stock/model-info", token);
    }

    public Task<ApiClientResult<PortfolioStock>> AddPortfolioStockAsync(CreatePortfolioStockRequest request, CancellationToken token)
    {
        return PostAsync<PortfolioStock>("api/stocks", request, token);
    }

    public Task<ApiClientResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken token)
    {
        return PostAsync<AuthResponse>("api/v1/auth/login", request, token);
    }

    public Task<ApiClientResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken token)
    {
        return PostAsync<AuthResponse>("api/v1/auth/register", request, token);
    }

    public Task<ApiClientResult<CurrentUser>> GetCurrentUserAsync(CancellationToken token)
    {
        return GetAsync<CurrentUser>("api/v1/auth/me", token);
    }

    public Task<ApiClientResult<bool>> LogoutAsync(CancellationToken token)
    {
        return SendNoContentAsync(HttpMethod.Post, "api/v1/auth/logout", token);
    }

    private async Task<ApiClientResult<T>> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        return await SendAsync<T>(HttpMethod.Get, path, null, cancellationToken);
    }

    private static string SearchPath(string query)
    {
        return $"api/market-data/search-name?query={Uri.EscapeDataString(query.Trim())}";
    }

    private static string ProfilePath(string symbol)
    {
        return $"api/market-data/profile?symbol={Uri.EscapeDataString(symbol.Trim().ToUpperInvariant())}";
    }

    private static string PriceHistoryPath(string symbol, string assetType, int take)
    {
        return $"api/price-history/{Esc(symbol)}?assetType={Esc(assetType)}&take={take}";
    }

    private static string PriceCapturePath(string symbol, string assetType)
    {
        return $"api/price-history/{Esc(symbol)}/capture?assetType={Esc(assetType)}";
    }

    private static string MagicSearchPath(string query, int pageSize)
    {
        return $"api/tcg/magic/cards?query={Esc(query)}&pageSize={Math.Clamp(pageSize, 1, 24)}";
    }

    private static string PredictionHistoryPath(int take)
    {
        return $"api/predict/history?take={Math.Clamp(take, 1, 100)}";
    }

    private static string Esc(string value)
    {
        return Uri.EscapeDataString(value.Trim());
    }

    private static ApiClientResult<CompanyProfile> FirstProfileResult(string symbol, ApiClientResult<List<CompanyProfile>> result)
    {
        if (!result.Ok || result.Data is null)
        {
            return ApiClientResult<CompanyProfile>.Failure(result.Message);
        }
        var profile = result.Data.FirstOrDefault();
        return profile is null ? MissingProfile(symbol) : ApiClientResult<CompanyProfile>.Success(profile);
    }

    private static ApiClientResult<CompanyProfile> MissingProfile(string symbol)
    {
        return ApiClientResult<CompanyProfile>.Failure($"No profile data for {symbol.Trim().ToUpperInvariant()}.");
    }

    private async Task<ApiClientResult<T>> PostAsync<T>(string path, object body, CancellationToken token)
    {
        return await SendAsync<T>(HttpMethod.Post, path, body, token);
    }

    private async Task<ApiClientResult<T>> SendAsync<T>(HttpMethod method, string path, object? body, CancellationToken token)
    {
        try
        {
            using var response = await SendRawAsync(method, path, body, token);
            return await ToResult<T>(response, token);
        }
        catch (Exception exception) when (IsConnectionFailure(exception))
        {
            return ApiClientResult<T>.Failure("Unable to reach Regulas.Api.");
        }
    }

    private async Task<ApiClientResult<bool>> SendNoContentAsync(HttpMethod method, string path, CancellationToken token)
    {
        try
        {
            using var response = await SendRawAsync(method, path, null, token);
            return response.IsSuccessStatusCode ? ApiClientResult<bool>.Success(true) : ApiClientResult<bool>.Failure(await ErrorMessage(response));
        }
        catch (Exception exception) when (IsConnectionFailure(exception))
        {
            return ApiClientResult<bool>.Failure("Unable to reach Regulas.Api.");
        }
    }

    private async Task<HttpResponseMessage> SendRawAsync(HttpMethod method, string path, object? body, CancellationToken token)
    {
        RefreshBaseAddress();
        using var request = await RequestAsync(method, path, body);
        return await _httpClient.SendAsync(request, token);
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
        var value = await response.Content.ReadFromJsonAsync<T>(JsonOptions, token);
        return value is null ? ApiClientResult<T>.Failure("Regulas.Api returned no data.") : ApiClientResult<T>.Success(value);
    }

    private async Task<HttpRequestMessage> RequestAsync(HttpMethod method, string path, object? body)
    {
        var request = new HttpRequestMessage(method, path);
        await AddBearerAsync(request);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }
        return request;
    }

    private async Task AddBearerAsync(HttpRequestMessage request)
    {
        var token = await _tokens.GetAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
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
