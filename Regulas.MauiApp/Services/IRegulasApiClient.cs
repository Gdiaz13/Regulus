using Regulas.MauiApp.Models;

namespace Regulas.MauiApp.Services;

public interface IRegulasApiClient
{
    Task<ApiClientResult<ApiHealth>> GetHealthAsync(CancellationToken cancellationToken);

    Task<ApiClientResult<IReadOnlyList<PortfolioStock>>> GetPortfolioStocksAsync(CancellationToken cancellationToken);

    Task<ApiClientResult<IReadOnlyList<CompanySearchResult>>> SearchCompaniesAsync(string query, CancellationToken token);

    Task<ApiClientResult<CompanyProfile>> GetCompanyProfileAsync(string symbol, CancellationToken token);

    Task<ApiClientResult<PriceHistoryResponse>> GetPriceHistoryAsync(string symbol, string assetType, int take, CancellationToken token);

    Task<ApiClientResult<PriceCaptureResult>> CapturePriceHistoryAsync(string symbol, string assetType, CancellationToken token);

    Task<ApiClientResult<PriceCaptureResult>> RecordManualPriceAsync(string symbol, ManualPriceRequest request, CancellationToken token);

    Task<ApiClientResult<PokemonCardSearchResponse>> SearchPokemonCardsAsync(string query, int pageSize, CancellationToken token);

    Task<ApiClientResult<PokemonCardDetail>> GetPokemonCardAsync(string id, CancellationToken token);

    Task<ApiClientResult<MagicCardSearchResponse>> SearchMagicCardsAsync(string query, int pageSize, CancellationToken token);

    Task<ApiClientResult<MagicCardDetail>> GetMagicCardAsync(string id, CancellationToken token);

    Task<ApiClientResult<OnePieceCardSearchResponse>> SearchOnePieceCardsAsync(string query, int pageSize, CancellationToken token);

    Task<ApiClientResult<OnePieceCardDetail>> GetOnePieceCardAsync(string id, CancellationToken token);

    Task<ApiClientResult<AiOverview>> PredictAsync(IReadOnlyList<PredictAssetRequest> assets, CancellationToken token);

    Task<ApiClientResult<IReadOnlyList<PredictionHistoryItem>>> GetPredictionHistoryAsync(int take, CancellationToken token);

    Task<ApiClientResult<IReadOnlyList<ModelAccuracySummary>>> GetPredictionAccuracySummaryAsync(CancellationToken token);

    Task<ApiClientResult<PredictionHealth>> GetPredictionHealthAsync(CancellationToken token);

    Task<ApiClientResult<StockTradingAgentsResponse>> AnalyzeStockWithTradingAgentsAsync(StockTradingAgentsRequest request, CancellationToken token);

    Task<ApiClientResult<TradingAgentsHealth>> GetTradingAgentsHealthAsync(CancellationToken token);

    Task<ApiClientResult<TradingAgentsModelInfo>> GetTradingAgentsModelInfoAsync(CancellationToken token);

    Task<ApiClientResult<PortfolioStock>> AddPortfolioStockAsync(CreatePortfolioStockRequest request, CancellationToken token);

    Task<ApiClientResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    Task<ApiClientResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);

    Task<ApiClientResult<CurrentUser>> GetCurrentUserAsync(CancellationToken cancellationToken);

    Task<ApiClientResult<bool>> LogoutAsync(CancellationToken cancellationToken);
}
