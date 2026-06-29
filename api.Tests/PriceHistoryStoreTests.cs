using System.Globalization;
using api.Contracts;
using api.Models;
using api.Services;
using Dapper;
using Xunit;

namespace api.Tests;

// Covers the Dapper price-history path that feeds charts and future model data.
public class PriceHistoryStoreTests
{
    [Fact]
    public async Task EnsureAssetAsync_creates_and_normalizes_when_missing()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new PriceHistoryStore(factory);
        var asset = await store.EnsureAssetAsync(" amd ", AssetType.Stock, "Advanced Micro");
        Assert.True(asset.Id > 0);
        Assert.Equal("AMD", asset.Symbol);
    }

    [Fact]
    public async Task EnsureAssetAsync_reuses_existing_asset()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new PriceHistoryStore(factory);
        var first = await store.EnsureAssetAsync("AMD", AssetType.Stock, null);
        var again = await store.EnsureAssetAsync("amd", AssetType.Stock, null);
        Assert.Equal(first.Id, again.Id);
        Assert.Equal(1, await Count(factory, "assets"));
    }

    [Fact]
    public async Task SaveAsync_stores_all_new_points()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new PriceHistoryStore(factory);
        var asset = await store.EnsureAssetAsync("AMD", AssetType.Stock, null);
        var saved = await store.SaveAsync(asset.Id, [Price("2026-01-02"), Price("2026-01-03")]);
        Assert.Equal(2, saved);
        Assert.Equal(2, await Count(factory, "price_history"));
    }

    [Fact]
    public async Task SaveAsync_dedupes_by_date_on_recapture()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new PriceHistoryStore(factory);
        var asset = await Seed(store);
        var saved = await store.SaveAsync(asset.Id, [Price("2026-01-02"), Price("2026-01-04")]);
        Assert.Equal(1, saved);
        Assert.Equal(3, await Count(factory, "price_history"));
    }

    [Fact]
    public async Task SaveAsync_ignores_duplicate_dates_inside_one_capture()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new PriceHistoryStore(factory);
        var asset = await store.EnsureAssetAsync("AMD", AssetType.Stock, null);
        var saved = await store.SaveAsync(asset.Id, [Price("2026-01-02"), Price("2026-01-02")]);
        Assert.Equal(1, saved);
        Assert.Equal(1, await Count(factory, "price_history"));
    }

    [Fact]
    public async Task ListPointsAsync_maps_fields_and_orders_by_date()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new PriceHistoryStore(factory);
        var asset = await store.EnsureAssetAsync("AMD", AssetType.Stock, null);
        await store.SaveAsync(asset.Id, [Price("2026-01-03", 13m), Price("2026-01-02", 12m)]);
        var points = await store.ListPointsAsync("amd", AssetType.Stock, 365);
        Assert.Equal([12m, 13m], points.Select(point => point.Close));
        Assert.All(points, point => Assert.Equal("FMP", point.Source));
    }

    [Fact]
    public async Task ListPointsAsync_returns_latest_points_with_limit()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new PriceHistoryStore(factory);
        var asset = await store.EnsureAssetAsync("AMD", AssetType.Stock, null);
        await store.SaveAsync(asset.Id, [Price("2026-01-01", 11m), Price("2026-01-02", 12m), Price("2026-01-03", 13m)]);
        var points = await store.ListPointsAsync("amd", AssetType.Stock, 2);
        Assert.Equal([12m, 13m], points.Select(point => point.Close));
    }

    [Fact]
    public async Task SaveAsync_tags_source()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new PriceHistoryStore(factory);
        var asset = await store.EnsureAssetAsync("AMD", AssetType.Stock, null);
        await store.SaveAsync(asset.Id, [Price("2026-01-02", 12m)]);
        Assert.Equal("FMP", await Scalar<string>(factory, "select source from price_history;"));
    }

    private static async Task<PriceHistoryAsset> Seed(PriceHistoryStore store)
    {
        var asset = await store.EnsureAssetAsync("AMD", AssetType.Stock, null);
        await store.SaveAsync(asset.Id, [Price("2026-01-02"), Price("2026-01-03")]);
        return asset;
    }

    private static FmpHistoricalPrice Price(string date, decimal close = 10m)
    {
        var day = DateOnly.Parse(date, CultureInfo.InvariantCulture);
        return new FmpHistoricalPrice("AMD", day, close, close + 1, close - 1, close, 1000);
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
