using System.Reflection;
using Regulas.MauiApp.Models;
using Regulas.MauiApp.Services;
using Regulas.MauiApp.ViewModels;
using Xunit;

namespace Regulas.MauiApp.Tests;

public class PredictionsViewModelTests
{
    [Fact]
    public async Task Load_populates_model_accuracy_summary()
    {
        var api = new FakeRegulasApiClient
        {
            AccuracyResult = ApiClientResult<IReadOnlyList<ModelAccuracySummary>>.Success([Summary()])
        };
        var viewModel = new PredictionsViewModel(api, SignedInSession(api));

        await viewModel.LoadAsync();

        var row = Assert.Single(viewModel.AccuracySummaries);
        Assert.Equal("StockTechAI", row.ModelName);
        Assert.Equal("18 scored predictions", row.ScoredText);
        Assert.Equal("61.11%", row.WinRateText);
        Assert.Equal("7.25%", row.AverageErrorText);
        Assert.Equal("14.50%", row.ConfidenceGapText);
        Assert.Equal("1 day", row.AverageHorizonText);
        Assert.True(viewModel.HasAccuracySummaries);
        Assert.False(viewModel.ShowAccuracyMessage);
    }

    [Fact]
    public async Task Load_shows_message_when_no_accuracy_has_been_scored()
    {
        var api = new FakeRegulasApiClient
        {
            AccuracyResult = ApiClientResult<IReadOnlyList<ModelAccuracySummary>>.Success([])
        };
        var viewModel = new PredictionsViewModel(api, SignedInSession(api));

        await viewModel.LoadAsync();

        Assert.Empty(viewModel.AccuracySummaries);
        Assert.False(viewModel.HasAccuracySummaries);
        Assert.Equal("No scored predictions yet.", viewModel.AccuracyMessageText);
        Assert.True(viewModel.ShowAccuracyMessage);
    }

    [Fact]
    public async Task Failed_accuracy_refresh_keeps_existing_summaries()
    {
        var api = new FakeRegulasApiClient
        {
            AccuracyResult = ApiClientResult<IReadOnlyList<ModelAccuracySummary>>.Success([Summary()])
        };
        var viewModel = new PredictionsViewModel(api, SignedInSession(api));
        await viewModel.LoadAsync();
        api.AccuracyResult = ApiClientResult<IReadOnlyList<ModelAccuracySummary>>.Failure("Accuracy unavailable.");

        await InvokePrivateAsync(viewModel, "LoadAccuracyAsync");

        Assert.Single(viewModel.AccuracySummaries);
        Assert.True(viewModel.HasAccuracySummaries);
        Assert.Equal("Accuracy unavailable.", viewModel.AccuracyMessageText);
        Assert.True(viewModel.ShowAccuracyMessage);
    }

    [Fact]
    public async Task Logout_during_accuracy_refresh_discards_the_previous_session_response()
    {
        var pending = new TaskCompletionSource<ApiClientResult<IReadOnlyList<ModelAccuracySummary>>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var api = new FakeRegulasApiClient
        {
            AccuracyResult = ApiClientResult<IReadOnlyList<ModelAccuracySummary>>.Success([Summary()])
        };
        var session = SignedInSession(api);
        var viewModel = new PredictionsViewModel(api, session);
        await viewModel.LoadAsync();
        api.AccuracyTask = pending.Task;

        var refresh = InvokePrivateAsync(viewModel, "LoadAccuracyAsync");
        await session.LogoutAsync(CancellationToken.None);
        pending.SetResult(ApiClientResult<IReadOnlyList<ModelAccuracySummary>>.Success([Summary()]));
        await refresh;

        Assert.False(viewModel.IsAuthenticated);
        Assert.Empty(viewModel.AccuracySummaries);
        Assert.False(viewModel.ShowAccuracySummaries);
    }

    private static Task InvokePrivateAsync(PredictionsViewModel viewModel, string name)
    {
        var method = typeof(PredictionsViewModel).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingMethodException(nameof(PredictionsViewModel), name);
        return (Task)(method.Invoke(viewModel, null) ?? throw new InvalidOperationException($"{name} did not return a task."));
    }

    private static AuthSession SignedInSession(IRegulasApiClient api)
    {
        return new AuthSession(api, new MemoryTokenStore("test-token"));
    }

    private static ModelAccuracySummary Summary()
    {
        return new ModelAccuracySummary(
            "StockTechAI", 18, 61.11, 7.25, 8.2, 6.4, 1.4,
            0.72, 0.31, 0.67, 0.33, 1.8, 55.56, 44.44, 14.5, 20.1, []
        );
    }

    private sealed class FakeRegulasApiClient : IRegulasApiClient
    {
        public ApiClientResult<IReadOnlyList<ModelAccuracySummary>> AccuracyResult { get; set; } = ApiClientResult<IReadOnlyList<ModelAccuracySummary>>.Failure("not set");
        public Task<ApiClientResult<IReadOnlyList<ModelAccuracySummary>>>? AccuracyTask { get; set; }
        public Task<ApiClientResult<IReadOnlyList<ModelAccuracySummary>>> GetPredictionAccuracySummaryAsync(CancellationToken token) => AccuracyTask ?? Task.FromResult(AccuracyResult);
        public Task<ApiClientResult<IReadOnlyList<PredictionHistoryItem>>> GetPredictionHistoryAsync(int take, CancellationToken token) => Task.FromResult(ApiClientResult<IReadOnlyList<PredictionHistoryItem>>.Success([]));
        public Task<ApiClientResult<CurrentUser>> GetCurrentUserAsync(CancellationToken token) => Task.FromResult(ApiClientResult<CurrentUser>.Success(User()));
        public Task<ApiClientResult<ApiHealth>> GetHealthAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<ApiClientResult<IReadOnlyList<PortfolioStock>>> GetPortfolioStocksAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<ApiClientResult<IReadOnlyList<CompanySearchResult>>> SearchCompaniesAsync(string query, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<CompanyProfile>> GetCompanyProfileAsync(string symbol, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<PriceHistoryResponse>> GetPriceHistoryAsync(string symbol, string assetType, int take, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<PriceCaptureResult>> CapturePriceHistoryAsync(string symbol, string assetType, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<PriceCaptureResult>> RecordManualPriceAsync(string symbol, ManualPriceRequest request, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<PokemonCardSearchResponse>> SearchPokemonCardsAsync(string query, int pageSize, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<PokemonCardDetail>> GetPokemonCardAsync(string id, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<MagicCardSearchResponse>> SearchMagicCardsAsync(string query, int pageSize, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<MagicCardDetail>> GetMagicCardAsync(string id, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<AiOverview>> PredictAsync(IReadOnlyList<PredictAssetRequest> assets, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<PredictionHealth>> GetPredictionHealthAsync(CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<StockTradingAgentsResponse>> AnalyzeStockWithTradingAgentsAsync(StockTradingAgentsRequest request, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<TradingAgentsHealth>> GetTradingAgentsHealthAsync(CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<TradingAgentsModelInfo>> GetTradingAgentsModelInfoAsync(CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<PortfolioStock>> AddPortfolioStockAsync(CreatePortfolioStockRequest request, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<ApiClientResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<ApiClientResult<bool>> LogoutAsync(CancellationToken cancellationToken) => Task.FromResult(ApiClientResult<bool>.Success(true));

        private static CurrentUser User() => new(Guid.NewGuid(), "qa@regulas.test", "QA", DateTime.UtcNow, null);
    }

    private sealed class MemoryTokenStore(string token) : IAuthTokenStore
    {
        public Task<string?> GetAsync() => Task.FromResult<string?>(token);
        public Task SaveAsync(string value) => Task.CompletedTask;
        public Task ClearAsync() => Task.CompletedTask;
    }
}
