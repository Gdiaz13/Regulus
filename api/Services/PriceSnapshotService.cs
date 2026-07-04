using api.Contracts;
using api.Models;

namespace api.Services;

// Periodically snapshots portfolio price history into PostgreSQL so user requests
// read stored data instead of calling the provider. Every run is recorded.
public sealed class PriceSnapshotService : RecurringJobService
{
    private const string FmpHistoryPath = "historical-price-eod/full";
    private readonly IConfiguration _configuration;

    public PriceSnapshotService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<PriceSnapshotService> logger
    )
        : base(scopeFactory, logger)
    {
        _configuration = configuration;
    }

    protected override string JobName => "price-snapshot";

    protected override bool Enabled => BackgroundJobsConfiguration.PriceSnapshotEnabled(_configuration);

    protected override TimeSpan StartupDelay => BackgroundJobsConfiguration.StartupDelay(_configuration);

    protected override TimeSpan Interval => BackgroundJobsConfiguration.PriceSnapshotInterval(_configuration);

    protected override async Task<JobOutcome> RunAsync(IServiceProvider services, CancellationToken token)
    {
        if (!MarketDataConfiguration.HasApiKey(_configuration))
        {
            return JobOutcome.Skipped("Market data is not configured (no FMP key).");
        }
        var symbols = await services.GetRequiredService<PortfolioStockStore>().ListTrackedSymbolsAsync(token);
        return symbols.Count == 0 ? JobOutcome.Skipped("No portfolio stocks to snapshot.") : await SnapshotAsync(services, symbols, token);
    }

    // Snapshots every distinct symbol across all portfolios: price history is
    // global market data, so the job runs once per symbol, not per user.
    private static async Task<JobOutcome> SnapshotAsync(IServiceProvider services, List<string> symbols, CancellationToken token)
    {
        var client = services.GetRequiredService<FinancialModelingPrepClient>();
        var store = services.GetRequiredService<PriceHistoryStore>();
        var captured = 0;
        foreach (var symbol in symbols)
        {
            captured += await SnapshotOneAsync(client, store, symbol, token);
        }
        return JobOutcome.Completed($"Snapshotted {symbols.Count} symbol(s).", captured);
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
}
