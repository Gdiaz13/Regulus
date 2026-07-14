using api.Contracts;
using api.Models;
using api.Services;
using Xunit;

namespace api.Tests;

public class PredictionRequestEnricherTests
{
    [Fact]
    public async Task Requests_gain_stored_closes_oldest_first()
    {
        using var factory = new SqliteDapperConnectionFactory();
        await SavePrices(factory, "AMD", [100m, 105m, 110m]);
        var enriched = await Enrich(factory, Request("AMD"));
        Assert.Equal([100m, 105m, 110m], enriched.RecentCloses);
    }

    [Fact]
    public async Task Requests_without_stored_history_pass_through_unchanged()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var request = Request("NVDA");
        var enriched = await Enrich(factory, request);
        Assert.Null(enriched.RecentCloses);
        Assert.Same(request, enriched);
    }

    private static async Task<AiPredictRequest> Enrich(SqliteDapperConnectionFactory factory, AiPredictRequest request)
    {
        var enricher = new PredictionRequestEnricher(new PriceHistoryStore(factory));
        return (await enricher.EnrichAsync([request])).Single();
    }

    private static AiPredictRequest Request(string symbol)
    {
        return new AiPredictRequest(symbol, symbol, "Stock", "Technology", 110m, 90);
    }

    private static async Task SavePrices(SqliteDapperConnectionFactory factory, string symbol, decimal[] closes)
    {
        var store = new PriceHistoryStore(factory);
        var asset = await store.EnsureAssetAsync(symbol, AssetType.Stock, symbol);
        var start = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-closes.Length);
        var prices = closes.Select((close, index) => new FmpHistoricalPrice(symbol, start.AddDays(index), close, close, close, close, 1000));
        await store.SaveAsync(asset.Id, prices);
    }
}
