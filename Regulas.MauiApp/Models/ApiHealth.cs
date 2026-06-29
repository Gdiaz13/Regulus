namespace Regulas.MauiApp.Models;

public sealed record ApiHealth(
    string Status,
    bool DatabaseAvailable,
    bool MarketDataConfigured
);
