namespace api.Contracts;

// One row from FMP's stable historical-price-eod feed. Property names line up
// with the JSON (web defaults), so the client deserializes straight into this.
// Extra fields FMP returns (vwap, change, etc.) are ignored.
public sealed record FmpHistoricalPrice(
    string Symbol,
    DateOnly Date,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    long Volume
);

// What the capture endpoint returns after pulling and storing history.
public sealed record CaptureResult(
    string Symbol,
    string AssetType,
    int AssetId,
    int Captured,
    int Skipped,
    string Source
);

// One stored price point as the frontend reads it back.
public sealed record PricePoint(
    DateOnly Date,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    long Volume,
    string Source
);

// The GET history response for a symbol.
public sealed record PriceHistoryResponse(
    string Symbol,
    string AssetType,
    int Count,
    List<PricePoint> Points
);
