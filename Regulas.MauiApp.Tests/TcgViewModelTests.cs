using System.Reflection;
using Regulas.MauiApp.Models;
using Regulas.MauiApp.Services;
using Regulas.MauiApp.ViewModels;
using Xunit;

namespace Regulas.MauiApp.Tests;

public class TcgViewModelTests
{
    [Fact]
    public async Task Failed_magic_detail_keeps_existing_search_results()
    {
        var api = new FakeRegulasApiClient
        {
            SearchMagicResult = ApiClientResult<MagicCardSearchResponse>.Success(SearchResponse()),
            MagicDetailResult = ApiClientResult<MagicCardDetail>.Failure("Magic detail unavailable.")
        };
        var viewModel = new TcgViewModel(api, new AuthSession(api, new MemoryTokenStore())) { MagicQuery = "lightning bolt" };
        await InvokePrivateAsync(viewModel, "SearchMagicAsync");
        Assert.Single(viewModel.MagicCards);
        await InvokePrivateAsync(viewModel, "OpenMagicAsync", "missing");
        Assert.Single(viewModel.MagicCards);
        Assert.True(viewModel.HasMagicCards);
        Assert.False(viewModel.HasMagicDetail);
        Assert.Equal("Magic detail unavailable.", viewModel.MagicMessageText);
    }

    private static Task InvokePrivateAsync(TcgViewModel viewModel, string name, params object?[] args)
    {
        var method = typeof(TcgViewModel).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingMethodException(nameof(TcgViewModel), name);
        return (Task)(method.Invoke(viewModel, args) ?? throw new InvalidOperationException($"{name} did not return a task."));
    }

    private static MagicCardSearchResponse SearchResponse()
    {
        return new MagicCardSearchResponse([Summary()], 1, 12, 1, 1);
    }

    private static MagicCardSummary Summary()
    {
        return new MagicCardSummary("card-1", "Lightning Bolt", "Limited Edition Alpha", "lea", "161", "common", null, 399.99m, "usd", "Scryfall", null);
    }

    private sealed class FakeRegulasApiClient : IRegulasApiClient
    {
        public ApiClientResult<MagicCardSearchResponse> SearchMagicResult { get; init; } = ApiClientResult<MagicCardSearchResponse>.Failure("not set");
        public ApiClientResult<MagicCardDetail> MagicDetailResult { get; init; } = ApiClientResult<MagicCardDetail>.Failure("not set");
        public Task<ApiClientResult<MagicCardSearchResponse>> SearchMagicCardsAsync(string query, int pageSize, CancellationToken token) => Task.FromResult(SearchMagicResult);
        public Task<ApiClientResult<MagicCardDetail>> GetMagicCardAsync(string id, CancellationToken token) => Task.FromResult(MagicDetailResult);
        public Task<ApiClientResult<ApiHealth>> GetHealthAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<ApiClientResult<IReadOnlyList<PortfolioStock>>> GetPortfolioStocksAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<ApiClientResult<IReadOnlyList<CompanySearchResult>>> SearchCompaniesAsync(string query, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<CompanyProfile>> GetCompanyProfileAsync(string symbol, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<PriceHistoryResponse>> GetPriceHistoryAsync(string symbol, string assetType, int take, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<PriceCaptureResult>> CapturePriceHistoryAsync(string symbol, string assetType, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<PriceCaptureResult>> RecordManualPriceAsync(string symbol, ManualPriceRequest request, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<AiOverview>> PredictAsync(IReadOnlyList<PredictAssetRequest> assets, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<IReadOnlyList<PredictionHistoryItem>>> GetPredictionHistoryAsync(int take, CancellationToken token) => throw new NotImplementedException();
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
