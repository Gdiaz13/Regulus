using api.Contracts;
using api.Models;

namespace api.Services;

// Periodically snapshots portfolio price history into PostgreSQL so user requests
// read stored data instead of calling the provider. Every run is recorded.
public sealed class PriceSnapshotService : BackgroundService
{
    private const string JobName = "price-snapshot";
    private const string FmpHistoryPath = "historical-price-eod/full";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PriceSnapshotService> _logger;

    public PriceSnapshotService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<PriceSnapshotService> logger
    )
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!BackgroundJobsConfiguration.PriceSnapshotEnabled(_configuration))
        {
            return;
        }
        await Loop(stoppingToken);
    }

    private async Task Loop(CancellationToken stoppingToken)
    {
        await SafeDelay(BackgroundJobsConfiguration.StartupDelay(_configuration), stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunSafelyAsync(stoppingToken);
            await SafeDelay(BackgroundJobsConfiguration.PriceSnapshotInterval(_configuration), stoppingToken);
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

    // One broken run must not crash the host, so failures are logged and swallowed.
    private async Task RunSafelyAsync(CancellationToken token)
    {
        try
        {
            await RunOnceAsync(token);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogWarning(exception, "Price snapshot run failed.");
        }
    }

    private async Task RunOnceAsync(CancellationToken token)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var runs = scope.ServiceProvider.GetRequiredService<BackgroundJobRunStore>();
        var id = await runs.StartAsync(JobName, token);
        var outcome = await CaptureAsync(scope.ServiceProvider, token);
        await runs.FinishAsync(id, outcome.Status, outcome.Detail, outcome.Count, token);
    }

    private async Task<Outcome> CaptureAsync(IServiceProvider services, CancellationToken token)
    {
        if (!MarketDataConfiguration.HasApiKey(_configuration))
        {
            return Outcome.Skipped("Market data is not configured (no FMP key).");
        }
        var stocks = await services.GetRequiredService<PortfolioStockStore>().ListAsync();
        return stocks.Count == 0 ? Outcome.Skipped("No portfolio stocks to snapshot.") : await SnapshotAsync(services, stocks, token);
    }

    private async Task<Outcome> SnapshotAsync(IServiceProvider services, List<Stock> stocks, CancellationToken token)
    {
        var client = services.GetRequiredService<FinancialModelingPrepClient>();
        var store = services.GetRequiredService<PriceHistoryStore>();
        var captured = 0;
        foreach (var stock in stocks)
        {
            captured += await SnapshotOneAsync(client, store, stock.Symbol, token);
        }
        return Outcome.Completed($"Snapshotted {stocks.Count} symbol(s).", captured);
    }

    private static async Task<int> SnapshotOneAsync(
        FinancialModelingPrepClient client,
        PriceHistoryStore store,
        string symbol,
        CancellationToken token
    )
    {
        var prices = await TryFetch(client, symbol);
        if (prices is null || prices.Count == 0)
        {
            return 0;
        }
        var asset = await store.EnsureAssetAsync(symbol, AssetType.Stock, symbol);
        return await store.SaveAsync(asset.Id, prices);
    }

    private static async Task<List<FmpHistoricalPrice>?> TryFetch(FinancialModelingPrepClient client, string symbol)
    {
        try
        {
            return await client.GetJsonAsync<List<FmpHistoricalPrice>>(FmpHistoryPath, Query(symbol));
        }
        catch (Exception exception) when (IsFetchException(exception))
        {
            return null;
        }
    }

    private static IReadOnlyDictionary<string, string?> Query(string symbol)
    {
        return new Dictionary<string, string?> { ["symbol"] = symbol.Trim().ToUpperInvariant() };
    }

    private static bool IsFetchException(Exception exception)
    {
        return exception is HttpRequestException or TaskCanceledException or InvalidOperationException;
    }

    private sealed record Outcome(string Status, string Detail, int Count)
    {
        public static Outcome Skipped(string detail) => new("skipped", detail, 0);

        public static Outcome Completed(string detail, int count) => new("completed", detail, count);
    }
}
