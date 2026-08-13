using System.ComponentModel;
using Regulas.MauiApp.Models;
using Regulas.MauiApp.Services;
using Xunit;

namespace Regulas.MauiApp.Tests;

// AppShell swaps between the sign-in gate and the tab bar purely off
// AuthSession raising PropertyChanged for IsAuthenticated. If that contract
// breaks the app silently strands users on one side of the gate, so it is
// pinned here rather than left to manual clicking.
public class AuthSessionTests
{
    [Fact]
    public async Task Signing_in_announces_that_the_session_is_authenticated()
    {
        var api = new FakeRegulasApiClient { LoginResult = ApiClientResult<AuthResponse>.Success(Response()) };
        var session = new AuthSession(api, new MemoryTokenStore());
        var announced = Watch(session);

        var result = await session.LoginAsync(new LoginRequest("star@regulas.local", "Sky-check-1!"), CancellationToken.None);

        Assert.True(result.Ok);
        Assert.True(session.IsAuthenticated);
        Assert.Contains(nameof(AuthSession.IsAuthenticated), announced);
    }

    [Fact]
    public async Task Signing_out_announces_that_the_gate_should_return()
    {
        var api = new FakeRegulasApiClient { LoginResult = ApiClientResult<AuthResponse>.Success(Response()) };
        var session = new AuthSession(api, new MemoryTokenStore());
        await session.LoginAsync(new LoginRequest("star@regulas.local", "Sky-check-1!"), CancellationToken.None);
        var announced = Watch(session);

        await session.LogoutAsync(CancellationToken.None);

        Assert.False(session.IsAuthenticated);
        Assert.Contains(nameof(AuthSession.IsAuthenticated), announced);
    }

    [Fact]
    public async Task A_stored_token_that_the_api_rejects_leaves_the_gate_up()
    {
        var api = new FakeRegulasApiClient { CurrentUserResult = ApiClientResult<CurrentUser>.Failure("expired") };
        var session = new AuthSession(api, new MemoryTokenStore("stale-token"));

        await session.RefreshAsync(CancellationToken.None);

        Assert.False(session.IsAuthenticated);
    }

    [Fact]
    public async Task A_stored_token_the_api_accepts_opens_the_app_without_a_prompt()
    {
        var api = new FakeRegulasApiClient { CurrentUserResult = ApiClientResult<CurrentUser>.Success(User()) };
        var session = new AuthSession(api, new MemoryTokenStore("good-token"));

        await session.RefreshAsync(CancellationToken.None);

        Assert.True(session.IsAuthenticated);
    }

    private static List<string> Watch(AuthSession session)
    {
        var announced = new List<string>();
        session.PropertyChanged += (_, args) => announced.Add(args.PropertyName ?? string.Empty);
        return announced;
    }

    private static CurrentUser User()
    {
        return new CurrentUser(Guid.NewGuid(), "star@regulas.local", "Sky Watcher", DateTime.UtcNow, null);
    }

    private static AuthResponse Response()
    {
        return new AuthResponse("token-value", DateTime.UtcNow.AddHours(8), User());
    }

    private sealed class MemoryTokenStore : IAuthTokenStore
    {
        private string? _token;

        public MemoryTokenStore(string? token = null)
        {
            _token = token;
        }

        public Task<string?> GetAsync() => Task.FromResult(_token);
        public Task SaveAsync(string token) { _token = token; return Task.CompletedTask; }
        public Task ClearAsync() { _token = null; return Task.CompletedTask; }
    }

    private sealed class FakeRegulasApiClient : IRegulasApiClient
    {
        public ApiClientResult<AuthResponse> LoginResult { get; init; } = ApiClientResult<AuthResponse>.Failure("not set");
        public ApiClientResult<CurrentUser> CurrentUserResult { get; init; } = ApiClientResult<CurrentUser>.Failure("not set");
        public Task<ApiClientResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken token) => Task.FromResult(LoginResult);
        public Task<ApiClientResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken token) => Task.FromResult(LoginResult);
        public Task<ApiClientResult<CurrentUser>> GetCurrentUserAsync(CancellationToken token) => Task.FromResult(CurrentUserResult);
        public Task<ApiClientResult<bool>> LogoutAsync(CancellationToken token) => Task.FromResult(ApiClientResult<bool>.Success(true));
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
        public Task<ApiClientResult<OnePieceCardSearchResponse>> SearchOnePieceCardsAsync(string query, int pageSize, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<OnePieceCardDetail>> GetOnePieceCardAsync(string id, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<PortfolioStock>> GetPortfolioStockAsync(string symbol, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<PortfolioStock>> UpdatePortfolioStockAsync(int id, CreatePortfolioStockRequest request, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<bool>> DeletePortfolioStockAsync(int id, CancellationToken token) => throw new NotImplementedException();
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
    }
}
