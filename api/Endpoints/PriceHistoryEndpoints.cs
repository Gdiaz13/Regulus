using api.Contracts;
using api.Models;
using api.Services;

namespace api.Endpoints;

// Capture and read stored price history. The gateway pulls EOD data from FMP,
// stores it, and serves it back; the browser never calls FMP for history.
public static class PriceHistoryEndpoints
{
    private const string FmpHistoryPath = "historical-price-eod/full";

    public static void MapPriceHistoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/price-history");
        group.MapPost("{symbol}/capture", Capture);
        group.MapGet("{symbol}", GetHistory);
    }

    private static async Task<IResult> Capture(
        string symbol,
        string? assetType,
        string? name,
        FinancialModelingPrepClient client,
        PriceHistoryStore store
    )
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            return Results.BadRequest("A symbol is required.");
        }
        return await RunCapture(symbol, assetType, name, client, store);
    }

    private static async Task<IResult> RunCapture(
        string symbol,
        string? assetType,
        string? name,
        FinancialModelingPrepClient client,
        PriceHistoryStore store
    )
    {
        var prices = await TryFetch(client, symbol);
        if (prices is null)
        {
            return MarketDataUnavailable();
        }
        return await Store(store, symbol, ParseType(assetType), name, prices);
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

    private static async Task<IResult> Store(
        PriceHistoryStore store,
        string symbol,
        AssetType type,
        string? name,
        List<FmpHistoricalPrice> prices
    )
    {
        var asset = await store.EnsureAssetAsync(symbol, type, name);
        var captured = await store.SaveAsync(asset.Id, prices);
        var result = new CaptureResult(asset.Symbol, type.ToString(), (int)asset.Id, captured, prices.Count - captured);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetHistory(string symbol, string? assetType, PriceHistoryStore store)
    {
        var type = ParseType(assetType);
        var clean = symbol.Trim().ToUpperInvariant();
        var points = await store.ListPointsAsync(clean, type);
        return Results.Ok(new PriceHistoryResponse(clean, type.ToString(), points.Count, points));
    }

    private static AssetType ParseType(string? value)
    {
        return Enum.TryParse<AssetType>(value, ignoreCase: true, out var parsed) ? parsed : AssetType.Stock;
    }

    private static IReadOnlyDictionary<string, string?> Query(string symbol)
    {
        return new Dictionary<string, string?> { ["symbol"] = symbol.Trim().ToUpperInvariant() };
    }

    private static bool IsFetchException(Exception exception)
    {
        return exception is HttpRequestException or TaskCanceledException or InvalidOperationException;
    }

    private static IResult MarketDataUnavailable()
    {
        return Results.Problem("Market data is unavailable (check the FMP API key).", statusCode: 503);
    }
}
