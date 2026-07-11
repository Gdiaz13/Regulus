using api.Contracts;
using api.Models;
using api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace api.Tests;

public class TcgCardPriceCaptureTests
{
    [Fact]
    public async Task Browsed_card_market_price_is_stored_with_provider_metadata()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new PriceHistoryStore(factory);
        await TcgCardPriceCapture.TryStoreAsync(store, Card("sv3-125", "2026/07/05", Variant(120.5m)), NullLogger.Instance);
        var point = Assert.Single(await store.ListPointsAsync("SV3-125", AssetType.TcgCard, 10));
        Assert.Equal(new DateOnly(2026, 7, 5), point.Date);
        Assert.Equal(120.5m, point.Close);
        Assert.Equal(PokemonTcgClient.SourceName, point.Source);
        Assert.Equal("Market", point.PriceType);
        Assert.Equal("USD", point.Currency);
    }

    [Fact]
    public async Task Browsed_magic_card_market_price_is_stored_with_provider_currency()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new PriceHistoryStore(factory);
        await TcgCardPriceCapture.TryStoreAsync(store, MagicCard("lea-161", Price("eur", 1.23m)), NullLogger.Instance);
        var point = Assert.Single(await store.ListPointsAsync("LEA-161", AssetType.TcgCard, 10));
        Assert.Equal(1.23m, point.Close);
        Assert.Equal(MagicTcgClient.SourceName, point.Source);
        Assert.Equal("Market", point.PriceType);
        Assert.Equal("EUR", point.Currency);
    }

    [Fact]
    public async Task Card_without_market_price_stores_nothing()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new PriceHistoryStore(factory);
        await TcgCardPriceCapture.TryStoreAsync(store, Card("sv3-125", "2026/07/05", Variant(null)), NullLogger.Instance);
        Assert.Empty(await store.ListPointsAsync("SV3-125", AssetType.TcgCard, 10));
    }

    [Fact]
    public async Task Repeat_views_on_the_same_provider_date_store_once()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new PriceHistoryStore(factory);
        await TcgCardPriceCapture.TryStoreAsync(store, Card("sv3-125", "2026/07/05", Variant(120.5m)), NullLogger.Instance);
        await TcgCardPriceCapture.TryStoreAsync(store, Card("sv3-125", "2026/07/05", Variant(130m)), NullLogger.Instance);
        var point = Assert.Single(await store.ListPointsAsync("SV3-125", AssetType.TcgCard, 10));
        Assert.Equal(120.5m, point.Close);
    }

    [Fact]
    public async Task Missing_provider_date_falls_back_to_today()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new PriceHistoryStore(factory);
        await TcgCardPriceCapture.TryStoreAsync(store, Card("sv3-125", null, Variant(120.5m)), NullLogger.Instance);
        var point = Assert.Single(await store.ListPointsAsync("SV3-125", AssetType.TcgCard, 10));
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), point.Date);
    }

    private static PokemonCardDetail Card(string id, string? updatedAt, params PokemonCardPrice[] prices)
    {
        return new PokemonCardDetail(
            id, "Charizard ex", "Pokemon", [], "180", [], "Obsidian Flames", "Scarlet & Violet",
            "125", null, "Rare", null, null, null, PokemonTcgClient.SourceName, updatedAt, [.. prices]
        );
    }

    private static MagicCardDetail MagicCard(string id, params MagicCardPrice[] prices)
    {
        return new MagicCardDetail(
            id, "Lightning Bolt", "Instant", "{R}", "Deal 3 damage.", ["R"], "Limited Edition Alpha",
            "lea", "161", null, "common", null, null, null, MagicTcgClient.SourceName, null, [.. prices]
        );
    }

    private static PokemonCardPrice Variant(decimal? market)
    {
        return new PokemonCardPrice("holofoil", 100m, 110m, 130m, market, null);
    }

    private static MagicCardPrice Price(string currency, decimal market)
    {
        return new MagicCardPrice(currency, "normal", market);
    }
}
