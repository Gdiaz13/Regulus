using System.Reflection;
using Regulas.MauiApp.Models;
using Regulas.MauiApp.Services;
using Regulas.MauiApp.ViewModels;
using Xunit;

namespace Regulas.MauiApp.Tests;

public class TcgViewModelTests
{
    [Fact]
    public async Task Pokemon_search_populates_provider_cards()
    {
        var api = new FakeRegulasApiClient
        {
            SearchPokemonResult = ApiClientResult<PokemonCardSearchResponse>.Success(PokemonSearchResponse())
        };
        var viewModel = new TcgViewModel(api, new AuthSession(api, new MemoryTokenStore()))
        {
            BrowseGame = "Pokemon",
            BrowseQuery = "charizard"
        };

        await InvokePrivateAsync(viewModel, "SearchCardsAsync");

        var card = Assert.Single(viewModel.ProviderCards);
        Assert.Equal("Pokemon", card.Game);
        Assert.Equal("Charizard ex", card.Name);
        Assert.Equal("Scarlet & Violet · 125 · Double Rare", card.Details);
        Assert.Equal("$120.50", card.Price);
        Assert.Equal("/charizard-small.png", card.ImageUrl);
        Assert.Equal("Found 1 Pokemon card.", viewModel.BrowseMessageText);
    }

    [Fact]
    public async Task Pokemon_detail_prefills_manual_market_entry()
    {
        var api = new FakeRegulasApiClient
        {
            SearchPokemonResult = ApiClientResult<PokemonCardSearchResponse>.Success(PokemonSearchResponse()),
            PokemonDetailResult = ApiClientResult<PokemonCardDetail>.Success(PokemonDetail())
        };
        var viewModel = new TcgViewModel(api, new AuthSession(api, new MemoryTokenStore())) { BrowseQuery = "charizard" };
        await InvokePrivateAsync(viewModel, "SearchCardsAsync");

        await InvokePrivateAsync(viewModel, "OpenCardAsync", Assert.Single(viewModel.ProviderCards));

        Assert.Equal("Charizard ex", viewModel.BrowseDetailName);
        Assert.Equal("/charizard-large.png", viewModel.BrowseDetailImageUrl);
        Assert.Equal("SV3-125", viewModel.Symbol);
        Assert.Equal("Pokemon", viewModel.Category);
        Assert.Equal("Market", viewModel.PriceType);
        Assert.Equal("115.25", viewModel.Price);
        Assert.Equal("USD", viewModel.Currency);
        Assert.Single(viewModel.ProviderPrices);
    }

    [Fact]
    public async Task Failed_magic_detail_keeps_existing_provider_results()
    {
        var api = new FakeRegulasApiClient
        {
            SearchMagicResult = ApiClientResult<MagicCardSearchResponse>.Success(SearchResponse()),
            MagicDetailResult = ApiClientResult<MagicCardDetail>.Failure("Magic detail unavailable.")
        };
        var viewModel = new TcgViewModel(api, new AuthSession(api, new MemoryTokenStore()))
        {
            BrowseGame = "Magic",
            BrowseQuery = "lightning bolt"
        };
        await InvokePrivateAsync(viewModel, "SearchCardsAsync");
        var card = Assert.Single(viewModel.ProviderCards);

        await InvokePrivateAsync(viewModel, "OpenCardAsync", card);

        Assert.Single(viewModel.ProviderCards);
        Assert.True(viewModel.HasProviderCards);
        Assert.False(viewModel.HasProviderDetail);
        Assert.Equal("Magic detail unavailable.", viewModel.BrowseMessageText);
    }

    [Fact]
    public async Task Empty_pokemon_search_shows_empty_message()
    {
        var api = new FakeRegulasApiClient
        {
            SearchPokemonResult = ApiClientResult<PokemonCardSearchResponse>.Success(new PokemonCardSearchResponse([], 1, 12, 0, 0))
        };
        var viewModel = new TcgViewModel(api, new AuthSession(api, new MemoryTokenStore())) { BrowseQuery = "missing" };

        await InvokePrivateAsync(viewModel, "SearchCardsAsync");

        Assert.Empty(viewModel.ProviderCards);
        Assert.Equal("No Pokemon cards matched that search.", viewModel.BrowseMessageText);
    }

    [Fact]
    public async Task Switching_browse_game_clears_previous_results()
    {
        var api = new FakeRegulasApiClient
        {
            SearchPokemonResult = ApiClientResult<PokemonCardSearchResponse>.Success(PokemonSearchResponse())
        };
        var viewModel = new TcgViewModel(api, new AuthSession(api, new MemoryTokenStore())) { BrowseQuery = "charizard" };
        await InvokePrivateAsync(viewModel, "SearchCardsAsync");

        viewModel.BrowseGame = "Magic";

        Assert.Empty(viewModel.ProviderCards);
        Assert.False(viewModel.HasProviderCards);
        Assert.Equal("Search Magic cards through Regulas.Api.", viewModel.BrowseMessageText);
    }

    [Fact]
    public async Task Pokemon_prefill_prefers_any_market_price_before_mid_fallbacks()
    {
        var detail = PokemonDetail(
            new PokemonCardPrice("normal", 90m, 110m, 130m, null, null),
            new PokemonCardPrice("holofoil", 100m, 115m, 140m, 120m, null));
        var api = new FakeRegulasApiClient
        {
            PokemonDetailResult = ApiClientResult<PokemonCardDetail>.Success(detail)
        };
        var viewModel = new TcgViewModel(api, new AuthSession(api, new MemoryTokenStore()));

        await InvokePrivateAsync(viewModel, "OpenCardAsync", PokemonRow());

        Assert.Equal("120", viewModel.Price);
    }

    [Fact]
    public async Task Unpriced_magic_detail_clears_previous_manual_price_and_currency()
    {
        var api = new FakeRegulasApiClient
        {
            PokemonDetailResult = ApiClientResult<PokemonCardDetail>.Success(PokemonDetail()),
            SearchMagicResult = ApiClientResult<MagicCardSearchResponse>.Success(SearchResponse())
        };
        var viewModel = new TcgViewModel(api, new AuthSession(api, new MemoryTokenStore()));
        await InvokePrivateAsync(viewModel, "OpenCardAsync", PokemonRow());
        viewModel.BrowseGame = "Magic";
        viewModel.BrowseQuery = "lightning bolt";
        await InvokePrivateAsync(viewModel, "SearchCardsAsync");
        api.MagicDetailResult = ApiClientResult<MagicCardDetail>.Success(UnpricedMagicDetail());

        await InvokePrivateAsync(viewModel, "OpenCardAsync", Assert.Single(viewModel.ProviderCards));

        Assert.Equal(string.Empty, viewModel.Price);
        Assert.Equal(string.Empty, viewModel.Currency);
    }

    [Fact]
    public async Task Provider_switch_discards_pending_search_response()
    {
        var pending = new TaskCompletionSource<ApiClientResult<PokemonCardSearchResponse>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var api = new FakeRegulasApiClient { SearchPokemonTask = pending.Task };
        var viewModel = new TcgViewModel(api, new AuthSession(api, new MemoryTokenStore())) { BrowseQuery = "charizard" };
        var search = InvokePrivateAsync(viewModel, "SearchCardsAsync");

        viewModel.BrowseGame = "Magic";
        pending.SetResult(ApiClientResult<PokemonCardSearchResponse>.Success(PokemonSearchResponse()));
        await search;

        Assert.Empty(viewModel.ProviderCards);
        Assert.Equal("Search Magic cards through Regulas.Api.", viewModel.BrowseMessageText);
    }

    [Fact]
    public async Task Provider_switch_discards_pending_detail_response()
    {
        var pending = new TaskCompletionSource<ApiClientResult<PokemonCardDetail>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var api = new FakeRegulasApiClient { PokemonDetailTask = pending.Task };
        var viewModel = new TcgViewModel(api, new AuthSession(api, new MemoryTokenStore()));
        var detail = InvokePrivateAsync(viewModel, "OpenCardAsync", PokemonRow());

        viewModel.BrowseGame = "Magic";
        pending.SetResult(ApiClientResult<PokemonCardDetail>.Success(PokemonDetail()));
        await detail;

        Assert.False(viewModel.HasProviderDetail);
        Assert.Equal(string.Empty, viewModel.Symbol);
    }

    [Fact]
    public async Task One_piece_search_populates_provider_cards()
    {
        var api = new FakeRegulasApiClient
        {
            SearchOnePieceResult = ApiClientResult<OnePieceCardSearchResponse>.Success(OnePieceSearchResponse())
        };
        var viewModel = new TcgViewModel(api, new AuthSession(api, new MemoryTokenStore()))
        {
            BrowseGame = "One Piece",
            BrowseQuery = "luffy"
        };

        await InvokePrivateAsync(viewModel, "SearchCardsAsync");

        var card = Assert.Single(viewModel.ProviderCards);
        Assert.Equal("One Piece", card.Game);
        Assert.Equal("Monkey.D.Luffy", card.Name);
        Assert.Equal("Pillars of Strength · OP03-070 · SR", card.Details);
        Assert.Equal("$0.31", card.Price);
        Assert.Equal("/luffy-small.png", card.ImageUrl);
        Assert.Equal("Found 1 One Piece card.", viewModel.BrowseMessageText);
    }

    [Fact]
    public async Task Empty_one_piece_search_shows_empty_message()
    {
        var api = new FakeRegulasApiClient
        {
            SearchOnePieceResult = ApiClientResult<OnePieceCardSearchResponse>.Success(new OnePieceCardSearchResponse([], 1, 12, 0, 0))
        };
        var viewModel = new TcgViewModel(api, new AuthSession(api, new MemoryTokenStore()))
        {
            BrowseGame = "One Piece",
            BrowseQuery = "missing"
        };

        await InvokePrivateAsync(viewModel, "SearchCardsAsync");

        Assert.Empty(viewModel.ProviderCards);
        Assert.Equal("No One Piece cards matched that search.", viewModel.BrowseMessageText);
    }

    [Fact]
    public async Task One_piece_detail_prefills_entry_with_card_code()
    {
        var api = new FakeRegulasApiClient
        {
            OnePieceDetailResult = ApiClientResult<OnePieceCardDetail>.Success(OnePieceDetail())
        };
        var viewModel = new TcgViewModel(api, new AuthSession(api, new MemoryTokenStore()));

        await InvokePrivateAsync(viewModel, "OpenCardAsync", OnePieceRow());

        Assert.Equal("Monkey.D.Luffy", viewModel.BrowseDetailName);
        Assert.Equal("/luffy-large.png", viewModel.BrowseDetailImageUrl);
        Assert.Contains("7000 power", viewModel.BrowseDetailText);
        Assert.Equal("OP03-070", viewModel.Symbol);
        Assert.Equal("One Piece", viewModel.Category);
        Assert.Equal("Market", viewModel.PriceType);
        Assert.Equal("0.31", viewModel.Price);
        Assert.Equal("USD", viewModel.Currency);
    }

    [Fact]
    public async Task One_piece_detail_without_code_falls_back_to_provider_id()
    {
        var api = new FakeRegulasApiClient
        {
            OnePieceDetailResult = ApiClientResult<OnePieceCardDetail>.Success(OnePieceDetail(code: "  "))
        };
        var viewModel = new TcgViewModel(api, new AuthSession(api, new MemoryTokenStore()));

        await InvokePrivateAsync(viewModel, "OpenCardAsync", OnePieceRow());

        Assert.Equal("1024", viewModel.Symbol);
    }

    [Fact]
    public async Task One_piece_detail_renders_a_price_row_per_market()
    {
        var api = new FakeRegulasApiClient
        {
            OnePieceDetailResult = ApiClientResult<OnePieceCardDetail>.Success(OnePieceDetail())
        };
        var viewModel = new TcgViewModel(api, new AuthSession(api, new MemoryTokenStore()));

        await InvokePrivateAsync(viewModel, "OpenCardAsync", OnePieceRow());

        Assert.Equal(2, viewModel.ProviderPrices.Count);
        Assert.Equal("Tcgplayer USD", viewModel.ProviderPrices[0].Label);
        Assert.Equal("Market $0.31 · Low $0.15 · High $2.50", viewModel.ProviderPrices[0].Price);
        Assert.Equal("Tcgmatch USD", viewModel.ProviderPrices[1].Label);
        Assert.Equal("Market $0.28", viewModel.ProviderPrices[1].Price);
    }

    [Fact]
    public async Task Provider_switch_discards_pending_one_piece_search_response()
    {
        var pending = new TaskCompletionSource<ApiClientResult<OnePieceCardSearchResponse>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var api = new FakeRegulasApiClient { SearchOnePieceTask = pending.Task };
        var viewModel = new TcgViewModel(api, new AuthSession(api, new MemoryTokenStore()))
        {
            BrowseGame = "One Piece",
            BrowseQuery = "luffy"
        };
        var search = InvokePrivateAsync(viewModel, "SearchCardsAsync");

        viewModel.BrowseGame = "Magic";
        pending.SetResult(ApiClientResult<OnePieceCardSearchResponse>.Success(OnePieceSearchResponse()));
        await search;

        Assert.Empty(viewModel.ProviderCards);
        Assert.Equal("Search Magic cards through Regulas.Api.", viewModel.BrowseMessageText);
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

    private static PokemonCardSearchResponse PokemonSearchResponse()
    {
        return new PokemonCardSearchResponse(
            [new PokemonCardSummary("sv3-125", "Charizard ex", "Scarlet & Violet", "Scarlet & Violet", "125", "Double Rare", "/charizard-small.png", 120.50m, "Pokemon TCG API", null)],
            1, 12, 1, 1
        );
    }

    private static PokemonCardDetail PokemonDetail(params PokemonCardPrice[] prices)
    {
        List<PokemonCardPrice> detailPrices = prices.Length == 0
            ? [new PokemonCardPrice("holofoil", 100m, 110m, 140m, 115.25m, null)]
            : [.. prices];
        return new PokemonCardDetail(
            "sv3-125", "Charizard ex", "Pokemon", ["Stage 2"], "330", ["Fire"],
            "Scarlet & Violet", "Scarlet & Violet", "125", "5ban Graphics", "Double Rare",
            "/charizard-small.png", "/charizard-large.png", null, "Pokemon TCG API", null,
            detailPrices
        );
    }

    private static TcgProviderCardRow PokemonRow()
    {
        return new TcgProviderCardRow("sv3-125", "Pokemon", "Charizard ex", "Scarlet & Violet", null, "$120.50");
    }

    private static MagicCardDetail UnpricedMagicDetail()
    {
        return new MagicCardDetail(
            "card-1", "Lightning Bolt", "Instant", "R", "Lightning Bolt deals 3 damage.", [],
            "Limited Edition Alpha", "lea", "161", null, "common", null, null, null, "Scryfall", null, []);
    }

    private static MagicCardSummary Summary()
    {
        return new MagicCardSummary("card-1", "Lightning Bolt", "Limited Edition Alpha", "lea", "161", "common", null, 399.99m, "usd", "Scryfall", null);
    }

    private static OnePieceCardSearchResponse OnePieceSearchResponse()
    {
        return new OnePieceCardSearchResponse(
            [new OnePieceCardSummary("1024", "Monkey.D.Luffy", "Pillars of Strength", "OP03-070", "SR", "Red", "/luffy-small.png", 0.31m, "APITCG", null)],
            1, 12, 1, 1
        );
    }

    private static OnePieceCardDetail OnePieceDetail(string? code = "OP03-070")
    {
        return new OnePieceCardDetail(
            "1024", "Monkey.D.Luffy", "The captain of the Straw Hat Pirates.", "Pillars of Strength", code,
            "OP03-070", "SR", "Red", "7000", "/luffy-small.png", "/luffy-large.png", null, "APITCG", null,
            [new OnePieceCardPrice("tcgplayer", "USD", 0.15m, 0.24m, 2.50m, 0.31m), new OnePieceCardPrice("tcgmatch", "USD", null, null, null, 0.28m)]
        );
    }

    private static TcgProviderCardRow OnePieceRow()
    {
        return new TcgProviderCardRow("1024", "One Piece", "Monkey.D.Luffy", "Pillars of Strength", null, "$0.31");
    }

    private sealed class FakeRegulasApiClient : IRegulasApiClient
    {
        public ApiClientResult<PokemonCardSearchResponse> SearchPokemonResult { get; init; } = ApiClientResult<PokemonCardSearchResponse>.Failure("not set");
        public ApiClientResult<PokemonCardDetail> PokemonDetailResult { get; set; } = ApiClientResult<PokemonCardDetail>.Failure("not set");
        public ApiClientResult<MagicCardSearchResponse> SearchMagicResult { get; init; } = ApiClientResult<MagicCardSearchResponse>.Failure("not set");
        public ApiClientResult<MagicCardDetail> MagicDetailResult { get; set; } = ApiClientResult<MagicCardDetail>.Failure("not set");
        public ApiClientResult<OnePieceCardSearchResponse> SearchOnePieceResult { get; init; } = ApiClientResult<OnePieceCardSearchResponse>.Failure("not set");
        public ApiClientResult<OnePieceCardDetail> OnePieceDetailResult { get; set; } = ApiClientResult<OnePieceCardDetail>.Failure("not set");
        public Task<ApiClientResult<PokemonCardSearchResponse>>? SearchPokemonTask { get; init; }
        public Task<ApiClientResult<PokemonCardDetail>>? PokemonDetailTask { get; init; }
        public Task<ApiClientResult<OnePieceCardSearchResponse>>? SearchOnePieceTask { get; init; }
        public Task<ApiClientResult<PokemonCardSearchResponse>> SearchPokemonCardsAsync(string query, int pageSize, CancellationToken token) => SearchPokemonTask ?? Task.FromResult(SearchPokemonResult);
        public Task<ApiClientResult<PokemonCardDetail>> GetPokemonCardAsync(string id, CancellationToken token) => PokemonDetailTask ?? Task.FromResult(PokemonDetailResult);
        public Task<ApiClientResult<MagicCardSearchResponse>> SearchMagicCardsAsync(string query, int pageSize, CancellationToken token) => Task.FromResult(SearchMagicResult);
        public Task<ApiClientResult<MagicCardDetail>> GetMagicCardAsync(string id, CancellationToken token) => Task.FromResult(MagicDetailResult);
        public Task<ApiClientResult<OnePieceCardSearchResponse>> SearchOnePieceCardsAsync(string query, int pageSize, CancellationToken token) => SearchOnePieceTask ?? Task.FromResult(SearchOnePieceResult);
        public Task<ApiClientResult<OnePieceCardDetail>> GetOnePieceCardAsync(string id, CancellationToken token) => Task.FromResult(OnePieceDetailResult);
        public Task<ApiClientResult<ApiHealth>> GetHealthAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<ApiClientResult<IReadOnlyList<PortfolioStock>>> GetPortfolioStocksAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<ApiClientResult<IReadOnlyList<CompanySearchResult>>> SearchCompaniesAsync(string query, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<CompanyProfile>> GetCompanyProfileAsync(string symbol, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<PriceHistoryResponse>> GetPriceHistoryAsync(string symbol, string assetType, int take, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<PriceCaptureResult>> CapturePriceHistoryAsync(string symbol, string assetType, CancellationToken token) => throw new NotImplementedException();
        public Task<ApiClientResult<PriceCaptureResult>> RecordManualPriceAsync(string symbol, ManualPriceRequest request, CancellationToken token) => throw new NotImplementedException();
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
