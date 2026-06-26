using api.Contracts;
using api.Models;
using api.Services;
using Dapper;
using Xunit;

namespace api.Tests;

// Covers the generic asset store that future stocks, cards, ETFs, and crypto use.
public class AssetStoreTests
{
    [Fact]
    public async Task CreateAsync_normalizes_symbol_and_uses_name()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new AssetStore(factory);
        var result = await store.CreateAsync(Command(" amd ", "Advanced Micro", AssetType.Stock));
        Assert.False(result.Duplicate);
        Assert.Equal("AMD", result.Asset!.Symbol);
        Assert.Equal("Advanced Micro", result.Asset.Name);
    }

    [Fact]
    public async Task CreateAsync_rejects_duplicate_type_and_symbol()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new AssetStore(factory);
        await store.CreateAsync(Command("AMD", "AMD", AssetType.Stock));
        var duplicate = await store.CreateAsync(Command("AMD", "AMD again", AssetType.Stock));
        Assert.True(duplicate.Duplicate);
        Assert.Equal(1, await CountAssets(factory));
    }

    [Fact]
    public async Task CreateAsync_allows_same_symbol_under_different_types()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new AssetStore(factory);
        await store.CreateAsync(Command("ABC", "Stock ABC", AssetType.Stock));
        await store.CreateAsync(Command("ABC", "Card ABC", AssetType.TcgCard));
        Assert.Equal(2, await CountAssets(factory));
    }

    [Fact]
    public async Task CreateAsync_creates_category_when_given()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new AssetStore(factory);
        var result = await store.CreateAsync(Command("PIKA", "Pikachu", AssetType.TcgCard, "Pokemon"));
        Assert.Equal("Pokemon", result.Asset!.Category);
        Assert.Equal("pokemon", await CategorySlug(factory));
    }

    [Fact]
    public async Task ListAsync_filters_by_asset_type()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new AssetStore(factory);
        await store.CreateAsync(Command("AMD", "AMD", AssetType.Stock));
        await store.CreateAsync(Command("PIKA", "Pikachu", AssetType.TcgCard));
        var stocks = await store.ListAsync(AssetType.Stock);
        Assert.Single(stocks);
        Assert.Equal("Stock", stocks[0].AssetType);
    }

    [Fact]
    public async Task FindAsync_returns_one_asset_with_category()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new AssetStore(factory);
        var created = await store.CreateAsync(Command("PIKA", "Pikachu", AssetType.TcgCard, "Pokemon"));
        var found = await store.FindAsync(created.Asset!.Id);
        Assert.Equal("PIKA", found!.Symbol);
        Assert.Equal("Pokemon", found.Category);
    }

    private static async Task<int> CountAssets(SqliteDapperConnectionFactory factory)
    {
        await using var connection = await factory.OpenDatabaseConnectionAsync();
        return await connection.ExecuteScalarAsync<int>("select count(*) from assets;");
    }

    private static async Task<string> CategorySlug(SqliteDapperConnectionFactory factory)
    {
        await using var connection = await factory.OpenDatabaseConnectionAsync();
        return (await connection.ExecuteScalarAsync<string>("select slug from asset_categories;"))!;
    }

    private static AssetCommand Command(string symbol, string name, AssetType type, string? category = null)
    {
        return new AssetCommand(symbol, name, type, category);
    }
}
