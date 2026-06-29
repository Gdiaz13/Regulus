using api.Contracts;
using api.Services;
using Dapper;
using Xunit;

namespace api.Tests;

public class PredictionStoreHistoryTests
{
    [Fact]
    public async Task ListHistoryAsync_returns_newest_predictions_first()
    {
        using var factory = new SqliteDapperConnectionFactory();
        await Save(factory, TestData.Prediction("AMD"));
        await Save(factory, TestData.Prediction("NVDA"));
        await Touch(factory, "AMD", DateTime.UtcNow.AddDays(-1));
        var history = await List(factory);
        Assert.Equal("NVDA", history[0].AssetId);
    }

    [Fact]
    public async Task ListHistoryAsync_filters_by_asset_id()
    {
        using var factory = new SqliteDapperConnectionFactory();
        await Save(factory, TestData.Prediction("AMD"));
        await Save(factory, TestData.Prediction("NVDA"));
        var history = await List(factory, " amd ");
        Assert.Single(history);
        Assert.Equal("AMD", history[0].AssetId);
    }

    [Fact]
    public async Task ListHistoryAsync_includes_reasons_and_warnings()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var prediction = TestData.Prediction("AMD", reasons: ["r"], warnings: ["w"]);
        await Save(factory, prediction);
        var saved = (await List(factory)).Single();
        Assert.Equal(["r"], saved.Reasons);
        Assert.Equal(["w"], saved.Warnings);
    }

    [Fact]
    public async Task ListHistoryAsync_caps_large_take_values()
    {
        using var factory = new SqliteDapperConnectionFactory();
        await SaveMany(factory, 105);
        Assert.Equal(100, (await List(factory, take: 500)).Count);
    }

    private static Task Save(SqliteDapperConnectionFactory factory, AiPrediction prediction)
    {
        return new PredictionStore(factory).SaveAsync(TestData.Overview(prediction));
    }

    private static Task<List<SavedPredictionResponse>> List(
        SqliteDapperConnectionFactory factory,
        string? assetId = null,
        int? take = null
    )
    {
        return new PredictionStore(factory).ListHistoryAsync(assetId, take);
    }

    private static async Task Touch(SqliteDapperConnectionFactory factory, string assetId, DateTime createdOn)
    {
        await using var connection = await factory.OpenDatabaseConnectionAsync();
        await connection.ExecuteAsync(
            "update predictions set created_on = @CreatedOn where asset_id = @AssetId;",
            new { AssetId = assetId, CreatedOn = createdOn }
        );
    }

    private static async Task SaveMany(SqliteDapperConnectionFactory factory, int count)
    {
        foreach (var index in Enumerable.Range(0, count))
        {
            await Save(factory, TestData.Prediction($"A{index}"));
        }
    }
}
