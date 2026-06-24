using api.Contracts;
using api.Models;
using api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace api.Tests;

public class PredictionAccuracyStoreTests
{
    [Fact]
    public async Task ListAsync_scores_prediction_against_later_price()
    {
        using var factory = new SqliteDbContextFactory();
        await SaveReadyPrediction(factory, "AMD");
        await SavePrice(factory, "AMD", DateOnly.FromDateTime(DateTime.UtcNow), 112m);
        var accuracy = await List(factory);
        Assert.Equal(12d, accuracy.Single().ActualPercentChange);
        Assert.Equal(2d, accuracy.Single().AbsolutePercentError);
    }

    [Fact]
    public async Task ListAsync_filters_by_asset_id()
    {
        using var factory = new SqliteDbContextFactory();
        await SaveReadyPrediction(factory, "AMD");
        await SaveReadyPrediction(factory, "NVDA");
        await SavePrice(factory, "AMD", DateOnly.FromDateTime(DateTime.UtcNow), 112m);
        var accuracy = await List(factory, " amd ");
        Assert.Single(accuracy);
        Assert.Equal("AMD", accuracy[0].AssetId);
    }

    [Fact]
    public async Task ListAsync_uses_first_price_on_or_after_target_date()
    {
        using var factory = new SqliteDbContextFactory();
        await SaveReadyPrediction(factory, "AMD");
        await SavePrice(factory, "AMD", DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1), 120m);
        await SavePrice(factory, "AMD", DateOnly.FromDateTime(DateTime.UtcNow), 112m);
        Assert.Equal(112m, (await List(factory)).Single().ActualPrice);
    }

    [Fact]
    public async Task ListAsync_skips_predictions_without_matching_price()
    {
        using var factory = new SqliteDbContextFactory();
        await SaveReadyPrediction(factory, "AMD");
        Assert.Empty(await List(factory));
    }

    private static async Task SaveReadyPrediction(SqliteDbContextFactory factory, string symbol)
    {
        using var db = factory.Create();
        await PredictionStore.SaveAsync(db, TestData.Overview(TestData.Prediction(symbol)));
        var prediction = await db.Predictions.SingleAsync(item => item.AssetId == symbol);
        prediction.CreatedOn = DateTime.UtcNow.AddDays(-2);
        prediction.TimeHorizonDays = 1;
        await db.SaveChangesAsync();
    }

    private static async Task SavePrice(SqliteDbContextFactory factory, string symbol, DateOnly date, decimal close)
    {
        using var db = factory.Create();
        var asset = await PriceHistoryStore.EnsureAssetAsync(db, symbol, AssetType.Stock, symbol);
        await PriceHistoryStore.SaveAsync(db, asset.Id, new[] { Price(symbol, date, close) });
    }

    private static FmpHistoricalPrice Price(string symbol, DateOnly date, decimal close)
    {
        return new FmpHistoricalPrice(symbol, date, close, close, close, close, 1000);
    }

    private static async Task<List<PredictionAccuracyResponse>> List(
        SqliteDbContextFactory factory,
        string? assetId = null
    )
    {
        using var db = factory.Create();
        return await PredictionAccuracyStore.ListAsync(db, assetId, null);
    }
}
