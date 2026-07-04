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

// One stored price point as the frontend reads it back. The TCG fields stay
// null for provider stock rows; defaults keep existing constructor calls working.
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

// A hand-entered price point, mainly for TCG cards where no provider feed exists
// yet. Tracks what kind of price it was (market/listed/sold), the card's
// condition and grade, and the currency, so different prices never mix silently.
public sealed record ManualPriceRequest(
    DateOnly Date,
    decimal Price,
    string? PriceType,
    string? CardCondition,
    string? Grade,
    string? Currency,
    string? Name
);

// The GET history response for a symbol.
public sealed record PriceHistoryResponse(
    string Symbol,
    string AssetType,
    int Count,
    List<PricePoint> Points
);
