namespace Regulas.MauiApp.Models;

public sealed record CompanySearchResult(
    string Symbol,
    string Name,
    string Currency,
    string ExchangeFullName,
    string Exchange
);

public sealed record CreatePortfolioStockRequest(
    string Symbol,
    string CompanyName,
    decimal? PurchasePrice,
    decimal? LastDividend,
    string Industry,
    long? MarketCap
);
