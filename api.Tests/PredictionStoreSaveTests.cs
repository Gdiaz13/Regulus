using api.Contracts;
using api.Models;
using api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace api.Tests;

// Covers PredictionStore.SaveAsync against a real SQLite schema: the save path
// that lets Regulas check model accuracy later.
public class PredictionStoreSaveTests
{
    [Fact]
    public async Task SaveAsync_returns_count_and_persists_every_prediction()
    {
        using var factory = new SqliteDbContextFactory();
        var overview = TestData.Overview(TestData.Prediction("AMD"), TestData.Prediction("NVDA"));
        var saved = await Save(factory, overview);
        using var read = factory.Create();
        Assert.Equal(2, saved);
        Assert.Equal(2, await read.Predictions.CountAsync());
    }

    [Fact]
    public async Task SaveAsync_splits_reasons_and_warnings_by_kind()
    {
        using var factory = new SqliteDbContextFactory();
        var prediction = TestData.Prediction("AMD", reasons: new[] { "a", "b" }, warnings: new[] { "MOCK DATA" });
        await Save(factory, TestData.Overview(prediction));
        using var read = factory.Create();
        Assert.Equal(2, await read.PredictionReasons.CountAsync(r => r.Kind == PredictionReasonKind.Reason));
        Assert.Equal(1, await read.PredictionReasons.CountAsync(r => r.Kind == PredictionReasonKind.Warning));
    }

    [Fact]
    public async Task SaveAsync_flags_mock_from_warning_text_case_insensitively()
    {
        using var factory = new SqliteDbContextFactory();
        var prediction = TestData.Prediction("AMD", warnings: new[] { "this contains mock data" });
        await Save(factory, TestData.Overview(prediction));
        using var read = factory.Create();
        Assert.True((await read.Predictions.SingleAsync()).IsMock);
    }

    [Fact]
    public async Task SaveAsync_leaves_mock_false_without_a_mock_warning()
    {
        using var factory = new SqliteDbContextFactory();
        var prediction = TestData.Prediction("AMD", warnings: new[] { "High volatility" });
        await Save(factory, TestData.Overview(prediction));
        using var read = factory.Create();
        Assert.False((await read.Predictions.SingleAsync()).IsMock);
    }

    [Fact]
    public async Task SaveAsync_parses_known_asset_type_case_insensitively()
    {
        using var factory = new SqliteDbContextFactory();
        await Save(factory, TestData.Overview(TestData.Prediction("PIKA", assetType: "tcgcard")));
        using var read = factory.Create();
        Assert.Equal(AssetType.TcgCard, (await read.Predictions.SingleAsync()).AssetType);
    }

    [Fact]
    public async Task SaveAsync_falls_back_to_stock_for_unknown_asset_type()
    {
        using var factory = new SqliteDbContextFactory();
        await Save(factory, TestData.Overview(TestData.Prediction("AMD", assetType: "Nonsense")));
        using var read = factory.Create();
        Assert.Equal(AssetType.Stock, (await read.Predictions.SingleAsync()).AssetType);
    }

    [Fact]
    public async Task SaveAsync_copies_prices_and_scores()
    {
        using var factory = new SqliteDbContextFactory();
        await Save(factory, TestData.Overview(TestData.Prediction("AMD")));
        using var read = factory.Create();
        var saved = await read.Predictions.SingleAsync();
        Assert.Equal(110m, saved.PredictedPrice);
        Assert.Equal(0.6d, saved.ConfidenceScore);
    }

    private static async Task<int> Save(SqliteDbContextFactory factory, AiOverview overview)
    {
        using var db = factory.Create();
        return await PredictionStore.SaveAsync(db, overview);
    }
}
