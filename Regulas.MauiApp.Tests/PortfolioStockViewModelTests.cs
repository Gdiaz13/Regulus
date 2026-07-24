using System.Reflection;
using Regulas.MauiApp.Models;
using Regulas.MauiApp.Services;
using Regulas.MauiApp.ViewModels;
using Xunit;

namespace Regulas.MauiApp.Tests;

public class PortfolioStockViewModelTests
{
    [Fact]
    public async Task Load_populates_position_and_notes()
    {
        var api = new FakeRegulasApiClient
        {
            StockResult = ApiClientResult<PortfolioStock>.Success(Stock()),
            CommentsResult = Comments(Note())
        };
        var viewModel = new PortfolioStockViewModel(api);
        viewModel.ApplySymbol("aapl");

        await viewModel.LoadAsync();

        Assert.Equal("Manage AAPL", viewModel.TitleText);
        Assert.Equal("Apple Inc.", viewModel.CompanyName);
        Assert.Equal("189.5", viewModel.PurchasePrice);
        Assert.True(viewModel.CanSaveStock);
        var note = Assert.Single(viewModel.Notes);
        Assert.Equal("Thesis", note.Title);
        Assert.Equal("2026-07-01", note.Created);
    }

    [Fact]
    public async Task Failed_load_shows_message_and_keeps_save_disabled()
    {
        var api = new FakeRegulasApiClient
        {
            StockResult = ApiClientResult<PortfolioStock>.Failure("Stock AAPL was not found.")
        };
        var viewModel = new PortfolioStockViewModel(api);
        viewModel.ApplySymbol("AAPL");

        await viewModel.LoadAsync();

        Assert.Equal("Stock AAPL was not found.", viewModel.MessageText);
        Assert.False(viewModel.CanSaveStock);
        Assert.False(viewModel.CanSaveNote);
    }

    [Fact]
    public async Task Save_sends_update_and_confirms()
    {
        var api = LoadedApi();
        api.UpdateStockResult = ApiClientResult<PortfolioStock>.Success(Stock() with { PurchasePrice = 200m });
        var viewModel = await LoadedViewModelAsync(api);
        viewModel.PurchasePrice = "200";

        await InvokePrivateAsync(viewModel, "SaveStockAsync");

        Assert.Equal("AAPL updated.", viewModel.MessageText);
        Assert.Equal("200", viewModel.PurchasePrice);
    }

    [Fact]
    public async Task Number_typo_disables_save_instead_of_saving_zero()
    {
        var viewModel = await LoadedViewModelAsync(LoadedApi());

        viewModel.PurchasePrice = "abc";

        Assert.False(viewModel.CanSaveStock);
    }

    [Fact]
    public async Task Add_note_clears_form_and_reloads()
    {
        var api = LoadedApi();
        var viewModel = await LoadedViewModelAsync(api);
        viewModel.NoteTitle = "Risk";
        viewModel.NoteContent = "China exposure.";
        api.AddCommentResult = ApiClientResult<StockComment>.Success(Note(id: 4, title: "Risk"));
        api.CommentsResult = Comments(Note(), Note(id: 4, title: "Risk"));

        await InvokePrivateAsync(viewModel, "SaveNoteAsync");

        Assert.Equal(string.Empty, viewModel.NoteTitle);
        Assert.Equal(2, viewModel.Notes.Count);
        Assert.Equal("Save note", viewModel.NoteFormLabel);
    }

    [Fact]
    public async Task Edit_note_updates_through_the_note_form()
    {
        var api = LoadedApi();
        api.UpdateCommentResult = ApiClientResult<StockComment>.Success(Note(title: "Thesis v2"));
        var viewModel = await LoadedViewModelAsync(api);

        viewModel.EditNoteCommand.Execute(viewModel.Notes[0]);
        Assert.Equal("Update note", viewModel.NoteFormLabel);
        Assert.Equal("Thesis", viewModel.NoteTitle);
        await InvokePrivateAsync(viewModel, "SaveNoteAsync");

        Assert.True(api.UpdateCommentCalled);
        Assert.Equal("Save note", viewModel.NoteFormLabel);
    }

    [Fact]
    public async Task Delete_note_reloads_and_shows_empty_message()
    {
        var api = LoadedApi();
        api.DeleteCommentResult = ApiClientResult<bool>.Success(true);
        var viewModel = await LoadedViewModelAsync(api);
        api.CommentsResult = Comments();

        await InvokePrivateAsync(viewModel, "DeleteNoteAsync", viewModel.Notes[0]);

        Assert.Empty(viewModel.Notes);
        Assert.Equal("No notes yet.", viewModel.NotesMessageText);
    }

    private static FakeRegulasApiClient LoadedApi()
    {
        return new FakeRegulasApiClient
        {
            StockResult = ApiClientResult<PortfolioStock>.Success(Stock()),
            CommentsResult = Comments(Note())
        };
    }

    private static async Task<PortfolioStockViewModel> LoadedViewModelAsync(FakeRegulasApiClient api)
    {
        var viewModel = new PortfolioStockViewModel(api);
        viewModel.ApplySymbol("AAPL");
        await viewModel.LoadAsync();
        return viewModel;
    }

    private static Task InvokePrivateAsync(PortfolioStockViewModel viewModel, string name, params object?[] args)
    {
        var method = typeof(PortfolioStockViewModel).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingMethodException(nameof(PortfolioStockViewModel), name);
        return (Task)(method.Invoke(viewModel, args) ?? throw new InvalidOperationException($"{name} did not return a task."));
    }

    private static PortfolioStock Stock()
    {
        return new PortfolioStock(7, "AAPL", "Apple Inc.", 189.5m, 0.96m, "Consumer Electronics", 2_900_000_000_000);
    }

    private static StockComment Note(int id = 3, string title = "Thesis")
    {
        return new StockComment(id, title, "Services growth holds up.", new DateTime(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc), 7);
    }

    private static ApiClientResult<IReadOnlyList<StockComment>> Comments(params StockComment[] notes)
    {
        return ApiClientResult<IReadOnlyList<StockComment>>.Success([.. notes]);
    }

    private sealed class FakeRegulasApiClient : IRegulasApiClient
    {
        public ApiClientResult<PortfolioStock> StockResult { get; init; } = ApiClientResult<PortfolioStock>.Failure("not set");
        public ApiClientResult<PortfolioStock> UpdateStockResult { get; set; } = ApiClientResult<PortfolioStock>.Failure("not set");
        public ApiClientResult<IReadOnlyList<StockComment>> CommentsResult { get; set; } = ApiClientResult<IReadOnlyList<StockComment>>.Failure("not set");
        public ApiClientResult<StockComment> AddCommentResult { get; set; } = ApiClientResult<StockComment>.Failure("not set");
        public ApiClientResult<StockComment> UpdateCommentResult { get; set; } = ApiClientResult<StockComment>.Failure("not set");
        public ApiClientResult<bool> DeleteCommentResult { get; set; } = ApiClientResult<bool>.Failure("not set");
        public bool UpdateCommentCalled { get; private set; }
        public Task<ApiClientResult<PortfolioStock>> GetPortfolioStockAsync(string symbol, CancellationToken token) => Task.FromResult(StockResult);
        public Task<ApiClientResult<PortfolioStock>> UpdatePortfolioStockAsync(int id, CreatePortfolioStockRequest request, CancellationToken token) => Task.FromResult(UpdateStockResult);
        public Task<ApiClientResult<IReadOnlyList<StockComment>>> GetStockCommentsAsync(int stockId, CancellationToken token) => Task.FromResult(CommentsResult);
        public Task<ApiClientResult<StockComment>> AddStockCommentAsync(int stockId, CreateStockCommentRequest request, CancellationToken token) => Task.FromResult(AddCommentResult);
        public Task<ApiClientResult<StockComment>> UpdateStockCommentAsync(int id, CreateStockCommentRequest request, CancellationToken token)
        {
            UpdateCommentCalled = true;
            return Task.FromResult(UpdateCommentResult);
        }
        public Task<ApiClientResult<bool>> DeleteStockCommentAsync(int id, CancellationToken token) => Task.FromResult(DeleteCommentResult);
        public Task<ApiClientResult<bool>> DeletePortfolioStockAsync(int id, CancellationToken token) => throw new NotImplementedException();
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
}
