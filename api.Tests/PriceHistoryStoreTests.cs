using System.Globalization;
using api.Contracts;
using api.Models;
using api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace api.Tests;

// Covers PriceHistoryStore: the find-or-create-asset + dedup-by-day logic that
// keeps captured history clean and tied to a first-class asset.
public class PriceHistoryStoreTests
{
    private static FmpHistoricalPrice Price(string date, decimal close = 10m)
    {
        var day = DateOnly.Parse(date, CultureInfo.InvariantCulture);
        return new FmpHistoricalPrice("AMD", day, close, close + 1, close - 1, close, 1000);
    }

    [Fact]
    public async Task EnsureAssetAsync_creates_and_normalizes_when_missing()
    {
        using var factory = new SqliteDbContextFactory();
        using var db = factory.Create();
        var asset = await PriceHistoryStore.EnsureAssetAsync(db, " amd ", AssetType.Stock, "Advanced Micro");
        Assert.True(asset.Id > 0);
        Assert.Equal("AMD", asset.Symbol);
    }

    [Fact]
    public async Task EnsureAssetAsync_reuses_existing_asset()
    {
        using var factory = new SqliteDbContextFactory();
        int firstId;
        using (var db = factory.Create()) firstId = (await PriceHistoryStore.EnsureAssetAsync(db, "AMD", AssetType.Stock, null)).Id;
        using var read = factory.Create();
        var again = await PriceHistoryStore.EnsureAssetAsync(read, "amd", AssetType.Stock, null);
        Assert.Equal(firstId, again.Id);
        Assert.Equal(1, await read.Assets.CountAsync());
    }

    [Fact]
    public async Task SaveAsync_stores_all_new_points()
    {
        using var factory = new SqliteDbContextFactory();
        using var db = factory.Create();
        var asset = await PriceHistoryStore.EnsureAssetAsync(db, "AMD", AssetType.Stock, null);
        var saved = await PriceHistoryStore.SaveAsync(db, asset.Id, new[] { Price("2026-01-02"), Price("2026-01-03") });
        Assert.Equal(2, saved);
        Assert.Equal(2, await db.PriceHistories.CountAsync());
    }

    [Fact]
    public async Task SaveAsync_dedupes_by_date_on_recapture()
    {
        using var factory = new SqliteDbContextFactory();
        var assetId = await Seed(factory);
        using var db = factory.Create();
        var saved = await PriceHistoryStore.SaveAsync(db, assetId, new[] { Price("2026-01-02"), Price("2026-01-04") });
        Assert.Equal(1, saved);
        Assert.Equal(3, await db.PriceHistories.CountAsync());
    }

    [Fact]
    public async Task SaveAsync_maps_fields_and_tags_source()
    {
        using var factory = new SqliteDbContextFactory();
        using var db = factory.Create();
        var asset = await PriceHistoryStore.EnsureAssetAsync(db, "AMD", AssetType.Stock, null);
        await PriceHistoryStore.SaveAsync(db, asset.Id, new[] { Price("2026-01-02", 12m) });
        var row = await db.PriceHistories.SingleAsync();
        Assert.Equal(12m, row.Close);
        Assert.Equal("FMP", row.Source);
    }

    private static async Task<int> Seed(SqliteDbContextFactory factory)
    {
        using var db = factory.Create();
        var asset = await PriceHistoryStore.EnsureAssetAsync(db, "AMD", AssetType.Stock, null);
        await PriceHistoryStore.SaveAsync(db, asset.Id, new[] { Price("2026-01-02"), Price("2026-01-03") });
        return asset.Id;
    }
}
