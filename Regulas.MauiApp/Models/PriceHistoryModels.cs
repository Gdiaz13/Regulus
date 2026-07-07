namespace Regulas.MauiApp.Models;

public sealed record PriceHistoryResponse(
    string Symbol,
    string AssetType,
    int Count,
    List<PricePoint> Points
);

// The TCG fields stay null for provider stock rows; defaults keep existing calls working.
public sealed record PricePoint(
    DateOnly Date,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    long Volume,
    string Source,
    string? PriceType = null,
    string? CardCondition = null,
    string? Grade = null,
    string? Currency = null
);

// What the manual capture endpoint accepts (TCG cards first); mirrors
// api/Contracts/PriceHistoryContracts.cs ManualPriceRequest.
public sealed record ManualPriceRequest(
    DateOnly Date,
    decimal Price,
    string? PriceType,
    string? CardCondition,
    string? Grade,
    string? Currency,
    string? Name
);

// One stored card price formatted for the TCG list.
public sealed record TcgPointRow(
    string Date,
    string Price,
    string Meta,
    string Source
);

public sealed record PriceCaptureResult(
    string Symbol,
    string AssetType,
    int AssetId,
    int Captured,
    int Skipped,
    string Source
);

public sealed record PricePointRow(
    string Date,
    string Close,
    string Range,
    string Volume,
    string Source
);
