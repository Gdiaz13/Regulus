namespace Regulas.MauiApp.Models;

// The slice of the FMP company profile the detail screen shows. The gateway
// proxies the full provider payload; extra fields are simply ignored here.
public sealed record CompanyProfile(
    string Symbol,
    string CompanyName,
    decimal Price,
    string Currency,
    decimal Change,
    double ChangePercentage,
    long MarketCap,
    string ExchangeFullName,
    string Industry,
    string Sector,
    string Ceo,
    string Country,
    string Website,
    string Description
);
