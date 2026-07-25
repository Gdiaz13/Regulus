namespace api.Services;

// What one job run reports back so BackgroundJobRunStore can record it.
public sealed record JobOutcome(string Status, string Detail, int Count)
{
    public static JobOutcome Skipped(string detail) => new("skipped", detail, 0);

    public static JobOutcome Completed(string detail, int count) => new("completed", detail, count);
}

// Base for scheduled jobs: waits, runs on an interval, records every run, and
// never lets one broken run crash the host. Derived jobs only implement RunAsync.
public abstract class RecurringJobService : BackgroundService
{
    private const string FailureDetail = "Job failed. See server logs for details.";
    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    protected RecurringJobService(IServiceScopeFactory scopeFactory, ILogger logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected abstract string JobName { get; }
    protected abstract bool Enabled { get; }
    protected abstract TimeSpan StartupDelay { get; }
    protected abstract TimeSpan Interval { get; }

    protected abstract Task<JobOutcome> RunAsync(IServiceProvider services, CancellationToken token);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!Enabled)
        {
            return;
        }
        await Loop(stoppingToken);
    }

    private async Task Loop(CancellationToken stoppingToken)
    {
        await SafeDelay(StartupDelay, stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunSafelyAsync(stoppingToken);
            await SafeDelay(Interval, stoppingToken);
        }
    }

    // One broken run must not crash the host, so failures are logged and swallowed.
    private async Task RunSafelyAsync(CancellationToken token)
    {
        try
        {
            await RunOnceAsync(token);
        }
        catch (Exception exception) when (ShouldHandle(exception, token))
        {
            _logger.LogWarning(exception, "{Job} run failed.", JobName);
        }
    }

    private async Task RunOnceAsync(CancellationToken token)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var runs = scope.ServiceProvider.GetRequiredService<BackgroundJobRunStore>();
        var id = await runs.StartAsync(JobName, token);
        try
        {
            await FinishRunAsync(scope.ServiceProvider, runs, id, token);
        }
        catch (Exception exception) when (ShouldHandle(exception, token))
        {
            await TryFinishFailedRunAsync(runs, id);
            throw;
        }
    }

    private async Task FinishRunAsync(
        IServiceProvider services,
        BackgroundJobRunStore runs,
        long id,
        CancellationToken token
    )
    {
        var outcome = await RunAsync(services, token);
        await runs.FinishAsync(id, outcome.Status, outcome.Detail, outcome.Count, token);
    }

    private static bool ShouldHandle(Exception exception, CancellationToken token)
    {
        return exception is not OperationCanceledException || !token.IsCancellationRequested;
    }

    private async Task TryFinishFailedRunAsync(BackgroundJobRunStore runs, long id)
    {
        try
        {
            await runs.FinishAsync(id, "failed", FailureDetail, 0, CancellationToken.None);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "{Job} failure status could not be persisted.", JobName);
        }
    }

    private static async Task SafeDelay(TimeSpan delay, CancellationToken token)
    {
        try
        {
            await Task.Delay(delay, token);
        }
        catch (OperationCanceledException)
        {
        }
    }
}
