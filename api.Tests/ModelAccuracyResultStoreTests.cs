using api.Contracts;
using api.Models;
using api.Services;
using Dapper;
using Xunit;

namespace api.Tests;

public class ModelAccuracyResultStoreTests
{
    [Fact]
    public async Task ScorePending_persists_matured_prediction()
    {
        using var factory = new SqliteDapperConnectionFactory();
        await SaveReadyPrediction(factory, "AMD");
        await SavePrice(factory, "AMD", DateOnly.FromDateTime(DateTime.UtcNow), 112m);
        var store = new ModelAccuracyResultStore(factory);
        Assert.Equal(1, await store.ScorePendingAsync());
        var result = Assert.Single(await store.ListResultsAsync(TestUsers.AliceId, null));
        Assert.Equal(12d, result.ActualPercentChange);
        Assert.Equal(2d, result.AbsolutePercentError);
        Assert.Equal(0.6d, result.ConfidenceScore);
        Assert.Equal(0.4d, result.RiskScore);
        Assert.Equal(0.7d, result.BullishScore);
        Assert.Equal(0.3d, result.BearishScore);
        Assert.Equal(1, result.TimeHorizonDays);
        Assert.True(result.DirectionMatched);
    }

    [Fact]
    public async Task ScorePending_scores_each_prediction_once()
    {
        using var factory = new SqliteDapperConnectionFactory();
        await SaveReadyPrediction(factory, "AMD");
        await SavePrice(factory, "AMD", DateOnly.FromDateTime(DateTime.UtcNow), 112m);
        var store = new ModelAccuracyResultStore(factory);
        await store.ScorePendingAsync();
        Assert.Equal(0, await store.ScorePendingAsync());
        Assert.Single(await store.ListResultsAsync(TestUsers.AliceId, null));
    }

    [Fact]
    public async Task RecalculateExisting_refreshes_when_better_target_price_arrives()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var createdOn = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var targetDate = DateOnly.FromDateTime(createdOn.AddDays(1));
        await SaveReadyPrediction(factory, "AMD", createdOn: createdOn);
        await SavePrice(factory, "AMD", targetDate.AddDays(2), 112m);
        var store = new ModelAccuracyResultStore(factory);
        Assert.Equal(1, await store.ScorePendingAsync());
        await SavePrice(factory, "AMD", targetDate, 105m);
        Assert.Equal(1, await store.RecalculateExistingAsync());
        var result = Assert.Single(await store.ListResultsAsync(TestUsers.AliceId, null));
        Assert.Equal(5d, result.ActualPercentChange);
        Assert.Equal(5d, result.AbsolutePercentError);
        Assert.Equal(1, result.TimeHorizonDays);
    }

    [Fact]
    public async Task ScorePending_skips_predictions_that_have_not_matured()
    {
        using var factory = new SqliteDapperConnectionFactory();
        await SaveReadyPrediction(factory, "AMD");
        var store = new ModelAccuracyResultStore(factory);
        Assert.Equal(0, await store.ScorePendingAsync());
        Assert.Empty(await store.ListResultsAsync(TestUsers.AliceId, null));
    }

    [Fact]
    public async Task ListResults_only_returns_current_users_rows()
    {
        using var factory = new SqliteDapperConnectionFactory();
        await SaveReadyPrediction(factory, "AMD", TestUsers.AliceId);
        await SaveReadyPrediction(factory, "NVDA", TestUsers.BobId);
        await SavePrice(factory, "AMD", DateOnly.FromDateTime(DateTime.UtcNow), 112m);
        await SavePrice(factory, "NVDA", DateOnly.FromDateTime(DateTime.UtcNow), 115m);
        var store = new ModelAccuracyResultStore(factory);
        Assert.Equal(2, await store.ScorePendingAsync());
        Assert.Equal(["NVDA"], (await store.ListResultsAsync(TestUsers.BobId, null)).Select(row => row.AssetId));
    }

    private static async Task SaveReadyPrediction(
        SqliteDapperConnectionFactory factory,
        string symbol,
        Guid? userId = null,
        DateTime? createdOn = null,
        int horizonDays = 1
    )
    {
        await new PredictionStore(factory).SaveAsync(userId ?? TestUsers.AliceId, TestData.Overview(TestData.Prediction(symbol)));
        await TouchPrediction(factory, symbol, createdOn ?? DateTime.UtcNow.AddDays(-2), horizonDays);
    }

    private static async Task TouchPrediction(
        SqliteDapperConnectionFactory factory,
        string symbol,
        DateTime createdOn,
        int horizonDays
    )
    {
        await using var connection = await factory.OpenDatabaseConnectionAsync();
        await connection.ExecuteAsync(SqlTouch, new { Symbol = symbol, CreatedOn = createdOn, HorizonDays = horizonDays });
    }

    private static async Task SavePrice(SqliteDapperConnectionFactory factory, string symbol, DateOnly date, decimal close)
    {
        var store = new PriceHistoryStore(factory);
        var asset = await store.EnsureAssetAsync(symbol, AssetType.Stock, symbol);
        await store.SaveAsync(asset.Id, [Price(symbol, date, close)]);
    }

    private static FmpHistoricalPrice Price(string symbol, DateOnly date, decimal close)
    {
        return new FmpHistoricalPrice(symbol, date, close, close, close, close, 1000);
    }

    private const string SqlTouch = """
        update predictions
        set created_on = @CreatedOn, time_horizon_days = @HorizonDays
        where asset_id = @Symbol;
        """;
}
