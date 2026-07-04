namespace Regulas.MauiApp.Models;

public sealed record PriceHistoryResponse(
    string Symbol,
    string AssetType,
    int Count,
    List<PricePoint> Points
);

public sealed record PricePoint(
    DateOnly Date,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    long Volume,
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
