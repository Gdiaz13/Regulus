namespace Regulas.MauiApp.Models;

public sealed record PortfolioStock(
    int Id,
    string Symbol,
    string CompanyName,
    decimal PurchasePrice,
    decimal LastDividend,
    string Industry,
    long MarketCap
);
