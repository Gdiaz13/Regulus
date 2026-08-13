using System.Reflection;
using api.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace api.Tests;

public class RecurringJobServiceTests
{
    [Fact]
    public async Task Failed_runs_are_finished_in_job_history()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var (store, provider, service) = Context(factory, new HttpRequestException("trainer offline"));
        using (provider)
        {
            await Assert.ThrowsAsync<HttpRequestException>(service.RunOnceForTest);
        }

        var run = Assert.Single(await store.ListRecentAsync(10));
        Assert.Equal("failed", run.Status);
        Assert.Equal("Job failed. See server logs for details.", run.Detail);
        Assert.NotNull(run.FinishedAt);
    }

    [Fact]
    public async Task Unrequested_timeout_is_recorded_and_does_not_escape_safe_run()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var (store, provider, service) = Context(factory, new TaskCanceledException("trainer timed out"));
        using (provider)
        {
            await service.RunSafelyForTest();
        }

        Assert.Equal("failed", Assert.Single(await store.ListRecentAsync(10)).Status);
    }

    [Fact]
    public async Task Requested_cancellation_propagates_without_failed_finalization()
    {
        using var factory = new SqliteDapperConnectionFactory();
        using var stopping = new CancellationTokenSource();
        var (store, provider, service) = Context(factory, new OperationCanceledException(stopping.Token), stopping);
        using (provider)
            await Assert.ThrowsAsync<OperationCanceledException>(() => service.RunSafelyForTest(stopping.Token));

        var run = Assert.Single(await store.ListRecentAsync(10));
        Assert.Equal("running", run.Status);
        Assert.Null(run.FinishedAt);
    }

    private static (BackgroundJobRunStore, ServiceProvider, FailingJobService) Context(
        SqliteDapperConnectionFactory factory,
        Exception failure,
        CancellationTokenSource? cancellation = null
    )
    {
        var store = new BackgroundJobRunStore(factory);
        var provider = new ServiceCollection().AddSingleton(store).BuildServiceProvider();
        var service = new FailingJobService(provider.GetRequiredService<IServiceScopeFactory>(), failure, cancellation);
        return (store, provider, service);
    }

    private sealed class FailingJobService(
        IServiceScopeFactory scopes,
        Exception failure,
        CancellationTokenSource? cancellation
    )
        : RecurringJobService(scopes, NullLogger.Instance)
    {
        protected override string JobName => "failing-job";
        protected override bool Enabled => true;
        protected override TimeSpan StartupDelay => TimeSpan.Zero;
        protected override TimeSpan Interval => TimeSpan.FromDays(1);

        public Task RunOnceForTest() => Invoke("RunOnceAsync");

        public Task RunSafelyForTest(CancellationToken token = default) => Invoke("RunSafelyAsync", token);

        private Task Invoke(string methodName, CancellationToken token = default)
        {
            var method = typeof(RecurringJobService).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new MissingMethodException(nameof(RecurringJobService), methodName);
            return (Task)(method.Invoke(this, [token])
                ?? throw new InvalidOperationException($"{methodName} did not return a task."));
        }

        protected override Task<JobOutcome> RunAsync(IServiceProvider services, CancellationToken token)
        {
            cancellation?.Cancel();
            return Task.FromException<JobOutcome>(failure);
        }
    }
}
