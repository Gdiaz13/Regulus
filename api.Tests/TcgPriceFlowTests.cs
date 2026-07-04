using api.Contracts;
using api.Models;
using api.Services;
using Xunit;

namespace api.Tests;

public class TcgPriceFlowTests
{
    [Fact]
    public async Task Manual_tcg_price_saves_and_reads_back_with_metadata()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new PriceHistoryStore(factory);
        var asset = await store.EnsureAssetAsync("SV3-125", AssetType.TcgCard, "Charizard ex");
        Assert.Equal(1, await store.SaveManualAsync(asset.Id, Request(120.50m)));
        var point = Assert.Single(await store.ListPointsAsync("SV3-125", AssetType.TcgCard, 10));
        Assert.Equal(120.50m, point.Close);
        Assert.Equal("Manual", point.Source);
        AssertTcgMetadata(point);
    }

    [Fact]
    public async Task Manual_price_on_same_date_is_stored_once()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new PriceHistoryStore(factory);
        var asset = await store.EnsureAssetAsync("SV3-125", AssetType.TcgCard, "Charizard ex");
        await store.SaveManualAsync(asset.Id, Request(120.50m));
        Assert.Equal(0, await store.SaveManualAsync(asset.Id, Request(130m)));
        Assert.Single(await store.ListPointsAsync("SV3-125", AssetType.TcgCard, 10));
    }

    [Fact]
    public async Task Provider_stock_points_carry_no_tcg_metadata()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new PriceHistoryStore(factory);
        var asset = await store.EnsureAssetAsync("AMD", AssetType.Stock, "AMD");
        await store.SaveAsync(asset.Id, [new FmpHistoricalPrice("AMD", DateOnly.FromDateTime(DateTime.UtcNow), 100, 101, 99, 100, 1000)]);
        var point = Assert.Single(await store.ListPointsAsync("AMD", AssetType.Stock, 10));
        Assert.Null(point.PriceType);
        Assert.Null(point.CardCondition);
        Assert.Null(point.Grade);
        Assert.Null(point.Currency);
    }

    private static void AssertTcgMetadata(PricePoint point)
    {
        Assert.Equal("Sold", point.PriceType);
        Assert.Equal("Near Mint", point.CardCondition);
        Assert.Equal("PSA 9", point.Grade);
        Assert.Equal("USD", point.Currency);
    }

    // Currency arrives lowercase on purpose: the store should normalize it.
    private static ManualPriceRequest Request(decimal price)
    {
        return new ManualPriceRequest(DateOnly.FromDateTime(DateTime.UtcNow), price, "Sold", "Near Mint", "PSA 9", "usd", null);
    }
}
