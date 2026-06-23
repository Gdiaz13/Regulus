using api.Contracts;
using api.Data;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

// Turns FMP history into stored PriceHistory rows. It find-or-creates the Asset
// so any captured symbol becomes a first-class asset, then dedupes by day so
// re-capturing only adds the days we do not already have.
public static class PriceHistoryStore
{
    private const string Source = "FMP";

    public static async Task<Asset> EnsureAssetAsync(ApplicationDBContext db, string symbol, AssetType type, string? name)
    {
        var clean = Normalize(symbol);
        var existing = await Find(db, clean, type);
        return existing ?? await CreateAsync(db, clean, type, name);
    }

    public static async Task<int> SaveAsync(ApplicationDBContext db, int assetId, IEnumerable<FmpHistoricalPrice> prices)
    {
        var existing = await ExistingDates(db, assetId);
        var fresh = prices.Where(price => !existing.Contains(price.Date)).Select(price => ToEntity(assetId, price)).ToList();
        db.PriceHistories.AddRange(fresh);
        await db.SaveChangesAsync();
        return fresh.Count;
    }

    private static Task<Asset?> Find(ApplicationDBContext db, string symbol, AssetType type)
    {
        return db.Assets.FirstOrDefaultAsync(asset => asset.AssetType == type && asset.Symbol == symbol);
    }

    private static async Task<Asset> CreateAsync(ApplicationDBContext db, string symbol, AssetType type, string? name)
    {
        var asset = new Asset { Symbol = symbol, AssetType = type, Name = name?.Trim() ?? symbol };
        db.Assets.Add(asset);
        await db.SaveChangesAsync();
        return asset;
    }

    private static async Task<HashSet<DateOnly>> ExistingDates(ApplicationDBContext db, int assetId)
    {
        var dates = await db.PriceHistories.Where(price => price.AssetId == assetId).Select(price => price.Date).ToListAsync();
        return dates.ToHashSet();
    }

    private static PriceHistory ToEntity(int assetId, FmpHistoricalPrice price)
    {
        return new PriceHistory
        {
            AssetId = assetId,
            Date = price.Date,
            Open = price.Open,
            High = price.High,
            Low = price.Low,
            Close = price.Close,
            Volume = price.Volume,
            Source = Source,
        };
    }

    private static string Normalize(string symbol)
    {
        return symbol.Trim().ToUpperInvariant();
    }
}
