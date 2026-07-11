using System.Data.Common;
using System.Globalization;
using api.Contracts;
using api.Models;

namespace api.Services;

// Persists a browsed card's market price so simply viewing cards builds TCG
// price history over time. Storage failures never break the card detail view.
public static class TcgCardPriceCapture
{
    public static async Task TryStoreAsync(PriceHistoryStore store, PokemonCardDetail card, ILogger logger)
    {
        try
        {
            await StoreAsync(store, card);
        }
        catch (Exception exception) when (IsStorageException(exception))
        {
            logger.LogWarning(exception, "Market price for card {Card} was not stored.", card.Id);
        }
    }

    private static async Task StoreAsync(PriceHistoryStore store, PokemonCardDetail card)
    {
        var price = MarketPrice(card);
        if (price is null)
        {
            return;
        }
        var asset = await store.EnsureAssetAsync(card.Id, AssetType.TcgCard, card.Name, "Pokemon");
        await store.SaveProviderCardPriceAsync(asset.Id, PriceDate(card), price.Value, card.Source);
    }

    // First variant with a market price, matching what the search list shows.
    private static decimal? MarketPrice(PokemonCardDetail card)
    {
        return card.Prices.Select(price => price.Market).FirstOrDefault(market => market is not null);
    }

    // The provider stamps prices with its own update date (yyyy/MM/dd); fall
    // back to today so a missing stamp still lands on a valid day.
    private static DateOnly PriceDate(PokemonCardDetail card)
    {
        return DateOnly.TryParseExact(card.UpdatedAt, "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : DateOnly.FromDateTime(DateTime.UtcNow);
    }

    private static bool IsStorageException(Exception exception)
    {
        return exception is DbException or InvalidOperationException;
    }
}
