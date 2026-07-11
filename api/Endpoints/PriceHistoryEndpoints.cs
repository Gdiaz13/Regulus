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
        // Manual entry is auth-only so anonymous callers cannot write price data.
        group.MapPost("{symbol}/manual", ManualCapture).RequireAuthorization();
        group.MapGet("{symbol}", GetHistory);
    }

    // Hand-entered prices, mainly for TCG cards: no provider feed exists yet, so
    // signed-in users record card prices with source metadata (type/condition/grade).
    private static async Task<IResult> ManualCapture(
        string symbol,
        string? assetType,
        ManualPriceRequest request,
        PriceHistoryStore store
    )
    {
        var validation = ValidateManual(symbol, request);
        if (validation is not null)
        {
            return validation;
        }
        return await StoreManual(store, symbol, ParseManualType(assetType), request);
    }

    private static async Task<IResult> StoreManual(PriceHistoryStore store, string symbol, AssetType type, ManualPriceRequest request)
    {
        try
        {
            return await SaveManual(store, symbol, type, request);
        }
        catch (Exception exception) when (IsManualCategoryException(exception))
        {
            return ManualCategoryError(exception);
        }
    }

    private static bool IsManualCategoryException(Exception exception)
    {
        return exception is InvalidTcgAssetCategoryException or TcgAssetCategoryConflictException;
    }

    private static IResult ManualCategoryError(Exception exception)
    {
        return exception is InvalidTcgAssetCategoryException
            ? Results.BadRequest(exception.Message)
            : Results.Conflict(exception.Message);
    }

    private static async Task<IResult> SaveManual(
        PriceHistoryStore store,
        string symbol,
        AssetType type,
        ManualPriceRequest request
    )
    {
        var asset = await store.EnsureAssetAsync(symbol, type, request.Name, request.Category);
        var captured = await store.SaveManualAsync(asset.Id, request);
        return Results.Ok(new CaptureResult(asset.Symbol, type.ToString(), (int)asset.Id, captured, 1 - captured, PriceHistoryStore.ManualSource));
    }

    private static IResult? ValidateManual(string symbol, ManualPriceRequest request)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            return Results.BadRequest("A symbol is required.");
        }
        return request.Price <= 0 ? Results.BadRequest("A positive price is required.") : null;
    }

    // Manual entry defaults to TCG cards because that is the market without a feed.
    private static AssetType ParseManualType(string? value)
    {
        return Enum.TryParse<AssetType>(value, ignoreCase: true, out var parsed) ? parsed : AssetType.TcgCard;
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
        return Results.Ok(BuildCaptureResult(asset, type, captured, prices.Count));
    }

    private static CaptureResult BuildCaptureResult(PriceHistoryAsset asset, AssetType type, int captured, int total)
    {
        var skipped = total - captured;
        return new CaptureResult(asset.Symbol, type.ToString(), (int)asset.Id, captured, skipped, PriceHistoryStore.ProviderSource);
    }

    private static async Task<IResult> GetHistory(string symbol, string? assetType, int? take, PriceHistoryStore store)
    {
        var type = ParseType(assetType);
        var clean = symbol.Trim().ToUpperInvariant();
        var points = await store.ListPointsAsync(clean, type, PriceHistoryQueryOptions.NormalizeTake(take));
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
