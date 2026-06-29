using api.Contracts;
using api.Services;
using Dapper;
using Xunit;

namespace api.Tests;

// Covers the save path that lets Regulas check model accuracy later.
public class PredictionStoreSaveTests
{
    [Fact]
    public async Task SaveAsync_returns_count_and_persists_every_prediction()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var overview = TestData.Overview(TestData.Prediction("AMD"), TestData.Prediction("NVDA"));
        var saved = await Save(factory, overview);
        Assert.Equal(2, saved);
        Assert.Equal(2, await Count(factory, "predictions"));
    }

    [Fact]
    public async Task SaveAsync_splits_reasons_and_warnings_by_kind()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var prediction = TestData.Prediction("AMD", reasons: ["a", "b"], warnings: ["MOCK DATA"]);
        await Save(factory, TestData.Overview(prediction));
        Assert.Equal(2, await ReasonCount(factory, "Reason"));
        Assert.Equal(1, await ReasonCount(factory, "Warning"));
    }

    [Fact]
    public async Task SaveAsync_flags_mock_from_warning_text_case_insensitively()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var prediction = TestData.Prediction("AMD", warnings: ["this contains mock data"]);
        await Save(factory, TestData.Overview(prediction));
        Assert.True(await Scalar<bool>(factory, "select is_mock from predictions;"));
    }

    [Fact]
    public async Task SaveAsync_leaves_mock_false_without_a_mock_warning()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var prediction = TestData.Prediction("AMD", warnings: ["High volatility"]);
        await Save(factory, TestData.Overview(prediction));
        Assert.False(await Scalar<bool>(factory, "select is_mock from predictions;"));
    }

    [Fact]
    public async Task SaveAsync_parses_known_asset_type_case_insensitively()
    {
        using var factory = new SqliteDapperConnectionFactory();
        await Save(factory, TestData.Overview(TestData.Prediction("PIKA", assetType: "tcgcard")));
        Assert.Equal("TcgCard", await Scalar<string>(factory, "select asset_type from predictions;"));
    }

    [Fact]
    public async Task SaveAsync_falls_back_to_stock_for_unknown_asset_type()
    {
        using var factory = new SqliteDapperConnectionFactory();
        await Save(factory, TestData.Overview(TestData.Prediction("AMD", assetType: "Nonsense")));
        Assert.Equal("Stock", await Scalar<string>(factory, "select asset_type from predictions;"));
    }

    [Fact]
    public async Task SaveAsync_copies_prices_and_scores()
    {
        using var factory = new SqliteDapperConnectionFactory();
        await Save(factory, TestData.Overview(TestData.Prediction("AMD")));
        Assert.Equal(110m, await Scalar<decimal>(factory, "select predicted_price from predictions;"));
        Assert.Equal(0.6d, await Scalar<double>(factory, "select confidence_score from predictions;"));
    }

    private static Task<int> Save(SqliteDapperConnectionFactory factory, AiOverview overview)
    {
        return new PredictionStore(factory).SaveAsync(overview);
    }

    private static Task<int> ReasonCount(SqliteDapperConnectionFactory factory, string kind)
    {
        return Scalar<int>(factory, $"select count(*) from prediction_reasons where kind = '{kind}';");
    }

    private static Task<int> Count(SqliteDapperConnectionFactory factory, string table)
    {
        return Scalar<int>(factory, $"select count(*) from {table};");
    }

    private static async Task<T> Scalar<T>(SqliteDapperConnectionFactory factory, string sql)
    {
        await using var connection = await factory.OpenDatabaseConnectionAsync();
        return (await connection.ExecuteScalarAsync<T>(sql))!;
    }
}
