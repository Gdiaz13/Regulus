using api.Contracts;
using api.Models;
using api.Services;
using Dapper;
using Xunit;

namespace api.Tests;

public class PredictionAccuracyStoreTests
{
    [Fact]
    public async Task ListAsync_scores_prediction_against_later_price()
    {
        using var factory = new SqliteDapperConnectionFactory();
        await SaveReadyPrediction(factory, "AMD");
        await SavePrice(factory, "AMD", DateOnly.FromDateTime(DateTime.UtcNow), 112m);
        var accuracy = await List(factory);
        Assert.Equal(12d, accuracy.Single().ActualPercentChange);
        Assert.Equal(2d, accuracy.Single().AbsolutePercentError);
        Assert.Equal(0.6d, accuracy.Single().ConfidenceScore);
        Assert.Equal(0.4d, accuracy.Single().RiskScore);
    }

    [Fact]
    public async Task ListAsync_filters_by_asset_id()
    {
        using var factory = new SqliteDapperConnectionFactory();
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
        using var factory = new SqliteDapperConnectionFactory();
        await SaveReadyPrediction(factory, "AMD");
        await SavePrice(factory, "AMD", DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1), 120m);
        await SavePrice(factory, "AMD", DateOnly.FromDateTime(DateTime.UtcNow), 112m);
        Assert.Equal(112m, (await List(factory)).Single().ActualPrice);
    }

    [Fact]
    public async Task ListAsync_skips_predictions_without_matching_price()
    {
        using var factory = new SqliteDapperConnectionFactory();
        await SaveReadyPrediction(factory, "AMD");
        Assert.Empty(await List(factory));
    }

    [Fact]
    public async Task ListAsync_only_scores_current_user_predictions()
    {
        using var factory = new SqliteDapperConnectionFactory();
        await SaveReadyPrediction(factory, "AMD", TestUsers.AliceId);
        await SaveReadyPrediction(factory, "NVDA", TestUsers.BobId);
        await SavePrice(factory, "AMD", DateOnly.FromDateTime(DateTime.UtcNow), 112m);
        await SavePrice(factory, "NVDA", DateOnly.FromDateTime(DateTime.UtcNow), 115m);
        var accuracy = await List(factory, userId: TestUsers.BobId);
        Assert.Equal(["NVDA"], accuracy.Select(item => item.AssetId));
    }

    [Fact]
    public async Task SummaryAsync_rolls_up_scored_predictions_per_model()
    {
        using var factory = new SqliteDapperConnectionFactory();
        await SaveReadyPrediction(factory, "AMD");
        await SavePrice(factory, "AMD", DateOnly.FromDateTime(DateTime.UtcNow), 112m);
        var summary = (await Summary(factory)).Single();
        Assert.Equal(1, summary.ScoredCount);
        Assert.Equal(100d, summary.WinRate);
        Assert.Equal(2d, summary.AverageAbsolutePercentError);
        Assert.Equal(12d, summary.AverageActualPercentChange);
        Assert.Equal(0.6d, summary.AverageConfidenceScore);
        Assert.Equal(0.4d, summary.AverageRiskScore);
        Assert.Equal(40d, summary.ConfidenceCalibrationError);
    }

    [Fact]
    public async Task SummaryAsync_win_rate_drops_when_direction_is_wrong()
    {
        using var factory = new SqliteDapperConnectionFactory();
        await SaveReadyPrediction(factory, "AMD");
        await SavePrice(factory, "AMD", DateOnly.FromDateTime(DateTime.UtcNow), 90m);
        var summary = (await Summary(factory)).Single();
        Assert.Equal(0d, summary.WinRate);
        Assert.Equal(-10d, summary.AverageActualPercentChange);
        Assert.Equal(60d, summary.ConfidenceCalibrationError);
    }

    [Fact]
    public async Task SummaryAsync_only_rolls_up_current_user_predictions()
    {
        using var factory = new SqliteDapperConnectionFactory();
        await SaveReadyPrediction(factory, "AMD", TestUsers.AliceId);
        await SaveReadyPrediction(factory, "NVDA", TestUsers.BobId);
        await SavePrice(factory, "AMD", DateOnly.FromDateTime(DateTime.UtcNow), 112m);
        await SavePrice(factory, "NVDA", DateOnly.FromDateTime(DateTime.UtcNow), 115m);
        var summary = (await Summary(factory)).Single();
        Assert.Equal(1, summary.ScoredCount);
    }

    // Summaries read through the same user-scoped scoring as ListAsync.
    private static Task<List<ModelAccuracySummary>> Summary(SqliteDapperConnectionFactory factory)
    {
        return new PredictionAccuracyStore(factory).SummaryAsync(TestUsers.AliceId, null, null);
    }

    private static async Task SaveReadyPrediction(
        SqliteDapperConnectionFactory factory,
        string symbol,
        Guid? userId = null
    )
    {
        await new PredictionStore(factory).SaveAsync(userId ?? TestUsers.AliceId, TestData.Overview(TestData.Prediction(symbol)));
        await TouchPrediction(factory, symbol);
    }

    private static async Task TouchPrediction(SqliteDapperConnectionFactory factory, string symbol)
    {
        await using var connection = await factory.OpenDatabaseConnectionAsync();
        await connection.ExecuteAsync(SqlTouch, new { Symbol = symbol, CreatedOn = DateTime.UtcNow.AddDays(-2) });
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

    private static Task<List<PredictionAccuracyResponse>> List(
        SqliteDapperConnectionFactory factory,
        string? assetId = null,
        Guid? userId = null
    )
    {
        return new PredictionAccuracyStore(factory).ListAsync(userId ?? TestUsers.AliceId, assetId, null);
    }

    private const string SqlTouch = """
        update predictions
        set created_on = @CreatedOn, time_horizon_days = 1
        where asset_id = @Symbol;
        """;
}
