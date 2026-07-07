namespace api.Contracts;

public sealed record PokemonCardSearchResponse(
    List<PokemonCardSummary> Cards,
    int Page,
    int PageSize,
    int Count,
    int TotalCount
);

public sealed record PokemonCardSummary(
    string Id,
    string Name,
    string? SetName,
    string? SetSeries,
    string? Number,
    string? Rarity,
    string? SmallImageUrl,
    decimal? MarketPrice,
    string Source,
    string? UpdatedAt
);

public sealed record PokemonCardDetail(
    string Id,
    string Name,
    string? Supertype,
    List<string> Subtypes,
    string? Hp,
    List<string> Types,
    string? SetName,
    string? SetSeries,
    string? Number,
    string? Artist,
    string? Rarity,
    string? SmallImageUrl,
    string? LargeImageUrl,
    string? TcgPlayerUrl,
    string Source,
    string? UpdatedAt,
    List<PokemonCardPrice> Prices
);

public sealed record PokemonCardPrice(
    string Variant,
    decimal? Low,
    decimal? Mid,
    decimal? High,
    decimal? Market,
    decimal? DirectLow
);

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
    string? ScryfallUrl,
    string Source,
    string? UpdatedAt,
    List<MagicCardPrice> Prices
);

public sealed record MagicCardPrice(
    string Currency,
    string Finish,
    decimal MarketPrice
);
