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
        var stock = await SeedStock(factory, TestUsers.AliceId);
        var store = new StockCommentStore(factory);
        var comment = await store.CreateAsync(TestUsers.AliceId, Comment(stock.Id, "Thesis"));
        Assert.True(comment.Id > 0);
        Assert.Equal(stock.Id, comment.StockId);
        Assert.Equal("Thesis", comment.Title);
    }

    [Fact]
    public async Task ListAsync_orders_newest_first()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var stock = await SeedStock(factory, TestUsers.AliceId);
        var store = new StockCommentStore(factory);
        await store.CreateAsync(TestUsers.AliceId, Comment(stock.Id, "Old", -1));
        await store.CreateAsync(TestUsers.AliceId, Comment(stock.Id, "New", 1));
        var comments = await store.ListAsync(TestUsers.AliceId, stock.Id);
        Assert.Equal(["New", "Old"], comments.Select(comment => comment.Title));
    }

    [Fact]
    public async Task UpdateAsync_updates_existing_comment()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var stock = await SeedStock(factory, TestUsers.AliceId);
        var store = new StockCommentStore(factory);
        var comment = await store.CreateAsync(TestUsers.AliceId, Comment(stock.Id, "Old"));
        var updated = await store.UpdateAsync(TestUsers.AliceId, comment.Id, "New", "Updated");
        Assert.Equal("New", updated!.Title);
        Assert.Equal("Updated", updated.Content);
    }

    [Fact]
    public async Task DeleteAsync_removes_comment()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var stock = await SeedStock(factory, TestUsers.AliceId);
        var store = new StockCommentStore(factory);
        var comment = await store.CreateAsync(TestUsers.AliceId, Comment(stock.Id, "Delete"));
        Assert.True(await store.DeleteAsync(TestUsers.AliceId, comment.Id));
        Assert.Empty(await store.ListAsync(TestUsers.AliceId, stock.Id));
    }

    [Fact]
    public async Task Store_methods_are_scoped_to_user()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var stock = await SeedStock(factory, TestUsers.AliceId);
        var store = new StockCommentStore(factory);
        var comment = await store.CreateAsync(TestUsers.AliceId, Comment(stock.Id, "Private"));
        Assert.Empty(await store.ListAsync(TestUsers.BobId, stock.Id));
        Assert.False(await store.StockExistsAsync(TestUsers.BobId, stock.Id));
        Assert.Null(await store.UpdateAsync(TestUsers.BobId, comment.Id, "Nope", "Denied"));
        Assert.False(await store.DeleteAsync(TestUsers.BobId, comment.Id));
    }

    private static async Task<Stock> SeedStock(SqliteDapperConnectionFactory factory, Guid userId)
    {
        var store = new PortfolioStockStore(factory);
        return await store.CreateAsync(userId, new Stock { Symbol = "AMD", CompanyName = "AMD" });
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
