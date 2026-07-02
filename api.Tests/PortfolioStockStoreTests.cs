using api.Models;
using api.Services;
using Xunit;

namespace api.Tests;

public class PortfolioStockStoreTests
{
    [Fact]
    public async Task CreateAsync_round_trips_stock_fields()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new PortfolioStockStore(factory);
        var stock = await store.CreateAsync(TestUsers.AliceId, Stock("AMD"));
        Assert.True(stock.Id > 0);
        Assert.Equal("AMD", stock.Symbol);
        Assert.Equal(100m, stock.PurchasePrice);
    }

    [Fact]
    public async Task ListAsync_orders_by_symbol()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new PortfolioStockStore(factory);
        await store.CreateAsync(TestUsers.AliceId, Stock("MSFT"));
        await store.CreateAsync(TestUsers.AliceId, Stock("AMD"));
        var stocks = await store.ListAsync(TestUsers.AliceId);
        Assert.Equal(["AMD", "MSFT"], stocks.Select(stock => stock.Symbol));
    }

    [Fact]
    public async Task UpdateAsync_updates_existing_stock()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new PortfolioStockStore(factory);
        var stock = await store.CreateAsync(TestUsers.AliceId, Stock("AMD"));
        var updated = await store.UpdateAsync(TestUsers.AliceId, stock.Id, Stock("NVDA"));
        Assert.Equal("NVDA", updated!.Symbol);
        Assert.True(await store.SymbolBelongsToAnotherAsync(TestUsers.AliceId, stock.Id + 1, "NVDA"));
    }

    [Fact]
    public async Task DeleteAsync_removes_existing_stock()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new PortfolioStockStore(factory);
        var stock = await store.CreateAsync(TestUsers.AliceId, Stock("AMD"));
        Assert.True(await store.DeleteAsync(TestUsers.AliceId, stock.Id));
        Assert.False(await store.ExistsAsync(TestUsers.AliceId, "AMD"));
    }

    [Fact]
    public async Task Store_methods_are_scoped_to_user()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new PortfolioStockStore(factory);
        var alice = await store.CreateAsync(TestUsers.AliceId, Stock("AMD"));
        var bob = await store.CreateAsync(TestUsers.BobId, Stock("AMD"));
        Assert.Equal([alice.Id], (await store.ListAsync(TestUsers.AliceId)).Select(stock => stock.Id));
        Assert.Null(await store.UpdateAsync(TestUsers.BobId, alice.Id, Stock("NVDA")));
        Assert.False(await store.DeleteAsync(TestUsers.BobId, alice.Id));
        Assert.True(await store.ExistsAsync(TestUsers.BobId, bob.Symbol));
    }

    private static Stock Stock(string symbol)
    {
        return new Stock
        {
            Symbol = symbol,
            CompanyName = $"{symbol} Inc.",
            PurchasePrice = 100m,
            LastDividend = 1m,
            Industry = "Technology",
            MarketCap = 123,
        };
    }
}
