namespace Regulas.MauiApp.Models;

public sealed record MagicCardSearchResponse(
    List<MagicCardSummary> Cards,
    int Page,
    int PageSize,
    int Count,
    int TotalCount
);

public sealed record MagicCardSummary(
    string Id,
    string Name,
    string? SetName,
    string? SetCode,
    string? CollectorNumber,
    string? Rarity,
    string? SmallImageUrl,
    decimal? MarketPrice,
    string? MarketCurrency,
    string Source,
    string? UpdatedAt
);

public sealed record MagicCardDetail(
    string Id,
    string Name,
    string? TypeLine,
    string? ManaCost,
    string? OracleText,
    List<string> Colors,
    string? SetName,
    string? SetCode,
    string? CollectorNumber,
    string? Artist,
    string? Rarity,
    string? SmallImageUrl,
    string? LargeImageUrl,
    string? SourceUrl,
    string Source,
    string? UpdatedAt,
    List<MagicCardPrice> Prices
);

public sealed record MagicCardPrice(
    string Currency,
    string Finish,
    decimal MarketPrice
);

public sealed record MagicCardSummaryRow(
    string Id,
    string Name,
    string Details,
    string Price
);

public sealed record MagicCardPriceRow(
    string Label,
    string Price
);
