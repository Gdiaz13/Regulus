using api.Contracts;
using api.Models;
using api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace api.Tests;

// Covers the generic asset store that future stocks, cards, ETFs, and crypto use.
public class AssetStoreTests
{
    [Fact]
    public async Task CreateAsync_normalizes_symbol_and_uses_name()
    {
        using var factory = new SqliteDbContextFactory();
        using var db = factory.Create();
        var result = await AssetStore.CreateAsync(db, Command(" amd ", "Advanced Micro", AssetType.Stock));
        Assert.False(result.Duplicate);
        Assert.Equal("AMD", result.Asset!.Symbol);
        Assert.Equal("Advanced Micro", result.Asset.Name);
    }

    [Fact]
    public async Task CreateAsync_rejects_duplicate_type_and_symbol()
    {
        using var factory = new SqliteDbContextFactory();
        using var db = factory.Create();
        await AssetStore.CreateAsync(db, Command("AMD", "AMD", AssetType.Stock));
        var duplicate = await AssetStore.CreateAsync(db, Command("AMD", "AMD again", AssetType.Stock));
        Assert.True(duplicate.Duplicate);
        Assert.Equal(1, await db.Assets.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_allows_same_symbol_under_different_types()
    {
        using var factory = new SqliteDbContextFactory();
        using var db = factory.Create();
        await AssetStore.CreateAsync(db, Command("ABC", "Stock ABC", AssetType.Stock));
        await AssetStore.CreateAsync(db, Command("ABC", "Card ABC", AssetType.TcgCard));
        Assert.Equal(2, await db.Assets.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_creates_category_when_given()
    {
        using var factory = new SqliteDbContextFactory();
        using var db = factory.Create();
        var result = await AssetStore.CreateAsync(db, Command("PIKA", "Pikachu", AssetType.TcgCard, "Pokemon"));
        Assert.Equal("Pokemon", result.Asset!.Category);
        Assert.Equal("pokemon", await db.AssetCategories.Select(category => category.Slug).SingleAsync());
    }

    [Fact]
    public async Task ListAsync_filters_by_asset_type()
    {
        using var factory = new SqliteDbContextFactory();
        using var db = factory.Create();
        await AssetStore.CreateAsync(db, Command("AMD", "AMD", AssetType.Stock));
        await AssetStore.CreateAsync(db, Command("PIKA", "Pikachu", AssetType.TcgCard));
        var stocks = await AssetStore.ListAsync(db, AssetType.Stock);
        Assert.Single(stocks);
        Assert.Equal("Stock", stocks[0].AssetType);
    }

    private static AssetCommand Command(string symbol, string name, AssetType type, string? category = null)
    {
        return new AssetCommand(symbol, name, type, category);
    }
}
