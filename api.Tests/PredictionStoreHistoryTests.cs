using api.Contracts;
using api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace api.Tests;

public class PredictionStoreHistoryTests
{
    [Fact]
    public async Task ListHistoryAsync_returns_newest_predictions_first()
    {
        using var factory = new SqliteDbContextFactory();
        await Save(factory, TestData.Prediction("AMD"));
        await Save(factory, TestData.Prediction("NVDA"));
        await Touch(factory, "AMD", DateTime.UtcNow.AddDays(-1));
        var history = await List(factory);
        Assert.Equal("NVDA", history[0].AssetId);
    }

    [Fact]
    public async Task ListHistoryAsync_filters_by_asset_id()
    {
        using var factory = new SqliteDbContextFactory();
        await Save(factory, TestData.Prediction("AMD"));
        await Save(factory, TestData.Prediction("NVDA"));
        var history = await List(factory, " amd ");
        Assert.Single(history);
        Assert.Equal("AMD", history[0].AssetId);
    }

    [Fact]
    public async Task ListHistoryAsync_includes_reasons_and_warnings()
    {
        using var factory = new SqliteDbContextFactory();
        var prediction = TestData.Prediction("AMD", reasons: new[] { "r" }, warnings: new[] { "w" });
        await Save(factory, prediction);
        var saved = (await List(factory)).Single();
        Assert.Equal(new[] { "r" }, saved.Reasons);
        Assert.Equal(new[] { "w" }, saved.Warnings);
    }

    [Fact]
    public async Task ListHistoryAsync_caps_large_take_values()
    {
        using var factory = new SqliteDbContextFactory();
        await SaveMany(factory, 105);
        Assert.Equal(100, (await List(factory, take: 500)).Count);
    }

    private static async Task Save(SqliteDbContextFactory factory, AiPrediction prediction)
    {
        using var db = factory.Create();
        await PredictionStore.SaveAsync(db, TestData.Overview(prediction));
    }

    private static async Task<List<SavedPredictionResponse>> List(
        SqliteDbContextFactory factory,
        string? assetId = null,
        int? take = null
    )
    {
        using var db = factory.Create();
        return await PredictionStore.ListHistoryAsync(db, assetId, take);
    }

    private static async Task Touch(SqliteDbContextFactory factory, string assetId, DateTime createdOn)
    {
        using var db = factory.Create();
        var prediction = await db.Predictions.SingleAsync(item => item.AssetId == assetId);
        prediction.CreatedOn = createdOn;
        await db.SaveChangesAsync();
    }

    private static async Task SaveMany(SqliteDbContextFactory factory, int count)
    {
        foreach (var index in Enumerable.Range(0, count))
        {
            await Save(factory, TestData.Prediction($"A{index}"));
        }
    }
}
