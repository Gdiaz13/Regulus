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
        var stock = await store.CreateAsync(Stock("AMD"));
        Assert.True(stock.Id > 0);
        Assert.Equal("AMD", stock.Symbol);
        Assert.Equal(100m, stock.PurchasePrice);
    }

    [Fact]
    public async Task ListAsync_orders_by_symbol()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new PortfolioStockStore(factory);
        await store.CreateAsync(Stock("MSFT"));
        await store.CreateAsync(Stock("AMD"));
        var stocks = await store.ListAsync();
        Assert.Equal(["AMD", "MSFT"], stocks.Select(stock => stock.Symbol));
    }

    [Fact]
    public async Task UpdateAsync_updates_existing_stock()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new PortfolioStockStore(factory);
        var stock = await store.CreateAsync(Stock("AMD"));
        var updated = await store.UpdateAsync(stock.Id, Stock("NVDA"));
        Assert.Equal("NVDA", updated!.Symbol);
        Assert.True(await store.SymbolBelongsToAnotherAsync(stock.Id + 1, "NVDA"));
    }

    [Fact]
    public async Task DeleteAsync_removes_existing_stock()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new PortfolioStockStore(factory);
        var stock = await store.CreateAsync(Stock("AMD"));
        Assert.True(await store.DeleteAsync(stock.Id));
        Assert.False(await store.ExistsAsync("AMD"));
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
