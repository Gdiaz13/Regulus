using api.Models;
using api.Services;
using Xunit;

namespace api.Tests;

public class StockCommentStoreTests
{
    [Fact]
    public async Task CreateAsync_round_trips_comment_fields()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var stock = await SeedStock(factory);
        var store = new StockCommentStore(factory);
        var comment = await store.CreateAsync(Comment(stock.Id, "Thesis"));
        Assert.True(comment.Id > 0);
        Assert.Equal(stock.Id, comment.StockId);
        Assert.Equal("Thesis", comment.Title);
    }

    [Fact]
    public async Task ListAsync_orders_newest_first()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var stock = await SeedStock(factory);
        var store = new StockCommentStore(factory);
        await store.CreateAsync(Comment(stock.Id, "Old", -1));
        await store.CreateAsync(Comment(stock.Id, "New", 1));
        var comments = await store.ListAsync(stock.Id);
        Assert.Equal(["New", "Old"], comments.Select(comment => comment.Title));
    }

    [Fact]
    public async Task UpdateAsync_updates_existing_comment()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var stock = await SeedStock(factory);
        var store = new StockCommentStore(factory);
        var comment = await store.CreateAsync(Comment(stock.Id, "Old"));
        var updated = await store.UpdateAsync(comment.Id, "New", "Updated");
        Assert.Equal("New", updated!.Title);
        Assert.Equal("Updated", updated.Content);
    }

    [Fact]
    public async Task DeleteAsync_removes_comment()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var stock = await SeedStock(factory);
        var store = new StockCommentStore(factory);
        var comment = await store.CreateAsync(Comment(stock.Id, "Delete"));
        Assert.True(await store.DeleteAsync(comment.Id));
        Assert.Empty(await store.ListAsync(stock.Id));
    }

    private static async Task<Stock> SeedStock(SqliteDapperConnectionFactory factory)
    {
        var store = new PortfolioStockStore(factory);
        return await store.CreateAsync(new Stock { Symbol = "AMD", CompanyName = "AMD" });
    }

    private static Comment Comment(int stockId, string title, int offsetDays = 0)
    {
        return new Comment
        {
            StockId = stockId,
            Title = title,
            Content = $"{title} content",
            CreatedOn = DateTime.UtcNow.AddDays(offsetDays),
        };
    }
}
