using api.Contracts;
using api.Models;
using api.Services;
using Xunit;

namespace api.Tests;

public class ModelTrainingDataStoreTests
{
    [Fact]
    public async Task Technology_series_are_bounded_and_oldest_first()
    {
        using var factory = new SqliteDapperConnectionFactory();
        await SavePrediction(factory, "AMD", "Technology");
        await SavePrediction(factory, "XOM", "Energy");
        await SavePrices(factory, "AMD", 100m, 35);
        await SavePrices(factory, "XOM", 200m, 35);

        var amd = Assert.Single(await ListSeries(factory));
        Assert.Equal("AMD", amd.Symbol);
        Assert.Equal(30, amd.Closes.Count);
        Assert.Equal(105m, amd.Closes[0]);
        Assert.Equal(134m, amd.Closes[^1]);
    }

    [Fact]
    public async Task Technology_series_cap_the_number_of_symbols()
    {
        using var factory = new SqliteDapperConnectionFactory();
        await SavePrediction(factory, "AMD", "Technology");
        await SavePrediction(factory, "NVDA", "Technology");
        await SavePrices(factory, "AMD", 100m, 35);
        await SavePrices(factory, "NVDA", 200m, 35, new DateOnly(2026, 2, 1));

        Assert.Equal("NVDA", Assert.Single(await ListSeries(factory, maxSeries: 1)).Symbol);
    }

    [Fact]
    public async Task Latest_non_mock_category_controls_training_eligibility()
    {
        using var factory = new SqliteDapperConnectionFactory();
        await SavePrediction(factory, "AMD", "Technology");
        await SavePrediction(factory, "AMD", "Energy");
        await SavePrices(factory, "AMD", 100m, 35);

        Assert.Empty(await ListSeries(factory));
    }

    [Fact]
    public async Task Mock_only_predictions_do_not_qualify_training_series()
    {
        using var factory = new SqliteDapperConnectionFactory();
        await SavePrediction(factory, "AMD", "Technology", ["MOCK fallback"]);
        await SavePrices(factory, "AMD", 100m, 35);

        Assert.Empty(await ListSeries(factory));
    }

    private static Task<List<AiTrainSeries>> ListSeries(
        SqliteDapperConnectionFactory factory,
        int maxSeries = 10
    )
    {
        return new ModelTrainingDataStore(factory)
            .ListSeriesAsync("Technology", maxSeries, pointsPerSeries: 30);
    }

    private static Task SavePrediction(
        SqliteDapperConnectionFactory factory,
        string symbol,
        string category,
        IEnumerable<string>? warnings = null
    )
    {
        var prediction = TestData.Prediction(assetId: symbol, category: category, warnings: warnings);
        return new PredictionStore(factory).SaveAsync(TestUsers.AliceId, TestData.Overview(prediction));
    }

    private static async Task SavePrices(
        SqliteDapperConnectionFactory factory, string symbol, decimal start, int count,
        DateOnly? first = null
    )
    {
        var store = new PriceHistoryStore(factory);
        var asset = await store.EnsureAssetAsync(symbol, AssetType.Stock, symbol);
        var firstDate = first ?? new DateOnly(2026, 1, 1);
        var prices = Enumerable.Range(0, count)
            .Select(index => Price(symbol, firstDate.AddDays(index), start + index));
        await store.SaveAsync(asset.Id, prices);
    }

    private static FmpHistoricalPrice Price(string symbol, DateOnly date, decimal close)
    {
        return new FmpHistoricalPrice(symbol, date, close, close, close, close, 1000);
    }
}
