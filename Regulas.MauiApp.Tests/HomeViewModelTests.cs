using System.Reflection;
using Regulas.MauiApp.Models;
using Regulas.MauiApp.Services;
using Regulas.MauiApp.ViewModels;
using Xunit;

namespace Regulas.MauiApp.Tests;

public class HomeViewModelTests
{
    [Fact]
    public async Task Remove_stock_refreshes_the_portfolio()
    {
        var api = new FakeRegulasApiClient
        {
            StocksResult = Stocks(Apple(), Nvidia()),
            DeleteStockResult = ApiClientResult<bool>.Success(true)
        };
        var viewModel = new HomeViewModel(api, new AuthSession(api, new MemoryTokenStore()));
        await InvokePrivateAsync(viewModel, "LoadStocksAsync");
        api.StocksResult = Stocks(Nvidia());

        await InvokePrivateAsync(viewModel, "RemoveStockAsync", Apple());

        var stock = Assert.Single(viewModel.Stocks);
        Assert.Equal("NVDA", stock.Symbol);
        Assert.False(viewModel.HasError);
    }

    [Fact]
    public async Task Failed_remove_shows_error_and_keeps_the_list()
    {
        var api = new FakeRegulasApiClient
        {
            StocksResult = Stocks(Apple(), Nvidia()),
            DeleteStockResult = ApiClientResult<bool>.Failure("Stock with id 1 was not found.")
        };
        var viewModel = new HomeViewModel(api, new AuthSession(api, new MemoryTokenStore()));
        await InvokePrivateAsync(viewModel, "LoadStocksAsync");

        await InvokePrivateAsync(viewModel, "RemoveStockAsync", Apple());

        Assert.Equal(2, viewModel.Stocks.Count);
        Assert.Equal("Stock with id 1 was not found.", viewModel.ErrorText);
    }

    private static Task InvokePrivateAsync(HomeViewModel viewModel, string name, params object?[] args)
    {
        var method = typeof(HomeViewModel).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingMethodException(nameof(HomeViewModel), name);
        return (Task)(method.Invoke(viewModel, args) ?? throw new InvalidOperationException($"{name} did not return a task."));
    }

    private static PortfolioStock Apple()
    {
        return new PortfolioStock(1, "AAPL", "Apple Inc.", 189.5m, 0.96m, "Consumer Electronics", 2_900_000_000_000);
    }

    private static PortfolioStock Nvidia()
    {
        return new PortfolioStock(2, "NVDA", "NVIDIA Corp.", 120m, 0.04m, "Semiconductors", 3_000_000_000_000);
    }

    private static ApiClientResult<IReadOnlyList<PortfolioStock>> Stocks(params PortfolioStock[] stocks)
    {
        return ApiClientResult<IReadOnlyList<PortfolioStock>>.Success([.. stocks]);
    }

    private sealed class FakeRegulasApiClient : IRegulasApiClient
    {
        public ApiClientResult<IReadOnlyList<PortfolioStock>> StocksResult { get; set; } = ApiClientResult<IReadOnlyList<PortfolioStock>>.Failure("not set");
        public ApiClientResult<bool> DeleteStockResult { get; init; } = ApiClientResult<bool>.Failure("not set");
        public Task<ApiClientResult<IReadOnlyList<PortfolioStock>>> GetPortfolioStocksAsync(CancellationToken cancellationToken) => Task.FromResult(StocksResult);
        public Task<ApiClientResult<bool>> DeletePortfolioStockAsync(int id, CancellationToken token) => Task.FromResult(DeleteStockResult);
        public Task<ApiClientResult<ApiHealth>> GetHealthAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<ApiClientResult<IReadOnlyList<CompanySearchResult>>> SearchCompaniesAsync(string query, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<CompanyProfile>> GetCompanyProfileAsync(string symbol, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<PriceHistoryResponse>> GetPriceHistoryAsync(string symbol, string assetType, int take, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<PriceCaptureResult>> CapturePriceHistoryAsync(string symbol, string assetType, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<PriceCaptureResult>> RecordManualPriceAsync(string symbol, ManualPriceRequest request, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<PokemonCardSearchResponse>> SearchPokemonCardsAsync(string query, int pageSize, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<PokemonCardDetail>> GetPokemonCardAsync(string id, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<MagicCardSearchResponse>> SearchMagicCardsAsync(string query, int pageSize, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<MagicCardDetail>> GetMagicCardAsync(string id, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<OnePieceCardSearchResponse>> SearchOnePieceCardsAsync(string query, int pageSize, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<OnePieceCardDetail>> GetOnePieceCardAsync(string id, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<PortfolioStock>> GetPortfolioStockAsync(string symbol, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<PortfolioStock>> UpdatePortfolioStockAsync(int id, CreatePortfolioStockRequest request, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<IReadOnlyList<StockComment>>> GetStockCommentsAsync(int stockId, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<StockComment>> AddStockCommentAsync(int stockId, CreateStockCommentRequest request, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<StockComment>> UpdateStockCommentAsync(int id, CreateStockCommentRequest request, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<bool>> DeleteStockCommentAsync(int id, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<AiOverview>> PredictAsync(IReadOnlyList<PredictAssetRequest> assets, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<IReadOnlyList<PredictionHistoryItem>>> GetPredictionHistoryAsync(int take, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<IReadOnlyList<ModelAccuracySummary>>> GetPredictionAccuracySummaryAsync(CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<PredictionHealth>> GetPredictionHealthAsync(CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<StockTradingAgentsResponse>> AnalyzeStockWithTradingAgentsAsync(StockTradingAgentsRequest request, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<TradingAgentsHealth>> GetTradingAgentsHealthAsync(CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<TradingAgentsModelInfo>> GetTradingAgentsModelInfoAsync(CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<PortfolioStock>> AddPortfolioStockAsync(CreatePortfolioStockRequest request, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<ApiClientResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<ApiClientResult<CurrentUser>> GetCurrentUserAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<ApiClientResult<bool>> LogoutAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
    }

    private sealed class MemoryTokenStore : IAuthTokenStore
    {
        public Task<string?> GetAsync() => Task.FromResult<string?>(null);
        public Task SaveAsync(string token) => Task.CompletedTask;
        public Task ClearAsync() => Task.CompletedTask;
    }
}
