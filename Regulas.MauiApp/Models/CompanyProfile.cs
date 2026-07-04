namespace Regulas.MauiApp.Models;

public sealed record CompanyProfile(
    string? Symbol,
    decimal? Price,
    long? MarketCap,
    decimal? Beta,
    decimal? LastDividend,
    string? Range,
    decimal? Change,
    decimal? ChangePercentage,
    long? Volume,
    long? AverageVolume,
    string? CompanyName,
    string? Currency,
    string? ExchangeFullName,
    string? Exchange,
    string? Industry,
    string? Website,
    string? Description,
    string? Ceo,
    string? Sector,
    string? Country,
    string? FullTimeEmployees,
    string? City,
    string? State,
    string? IpoDate,
    bool? IsActivelyTrading
);

public sealed record ProfileMetric(string Label, string Value);
