using api.Services;
using Xunit;

namespace api.Tests;

public class BackgroundJobRunStoreTests
{
    [Fact]
    public async Task Start_records_a_running_job()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new BackgroundJobRunStore(factory);
        var id = await store.StartAsync("price-snapshot");
        var run = Assert.Single(await store.ListRecentAsync(10));
        Assert.Equal(id, run.Id);
        Assert.Equal("running", run.Status);
        Assert.Null(run.FinishedAt);
    }

    [Fact]
    public async Task Finish_closes_the_run_with_status_and_count()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new BackgroundJobRunStore(factory);
        var id = await store.StartAsync("price-snapshot");
        await store.FinishAsync(id, "completed", "done", 7);
        var run = Assert.Single(await store.ListRecentAsync(10));
        Assert.Equal("completed", run.Status);
        Assert.Equal(7, run.ItemsProcessed);
        Assert.NotNull(run.FinishedAt);
    }

    [Fact]
    public async Task ListRecent_returns_newest_first_within_limit()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new BackgroundJobRunStore(factory);
        await store.StartAsync("first");
        var second = await store.StartAsync("second");
        var runs = await store.ListRecentAsync(1);
        Assert.Equal(second, Assert.Single(runs).Id);
    }
}
