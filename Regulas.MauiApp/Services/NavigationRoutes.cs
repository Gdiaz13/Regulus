using System.Globalization;

namespace Regulas.MauiApp.Services;

public static class NavigationRoutes
{
    public static Task OpenAccountAsync()
    {
        return Shell.Current.GoToAsync($"//{nameof(AuthPage)}");
    }

    public static Task OpenAssetDetailAsync(string? symbol)
    {
        var cleanSymbol = CleanSymbol(symbol);
        return string.IsNullOrWhiteSpace(cleanSymbol)
            ? Task.CompletedTask
            : Shell.Current.GoToAsync(AssetDetailRoute(cleanSymbol));
    }

    public static Task OpenStockDetailAsync(string? symbol)
    {
        return OpenAssetDetailAsync(symbol);
    }

    public static Task OpenPortfolioStockAsync(string? symbol)
    {
        var cleanSymbol = CleanSymbol(symbol);
        return string.IsNullOrWhiteSpace(cleanSymbol)
            ? Task.CompletedTask
            : Shell.Current.GoToAsync(PortfolioStockRoute(cleanSymbol));
    }

    public static Task OpenPriceHistoryAsync(string? symbol)
    {
        var cleanSymbol = CleanSymbol(symbol);
        return string.IsNullOrWhiteSpace(cleanSymbol)
            ? Task.CompletedTask
            : Shell.Current.GoToAsync(PriceHistoryRoute(cleanSymbol));
    }

    public static Task OpenPredictionsAsync(string? symbol = null)
    {
        var cleanSymbol = CleanSymbol(symbol);
        return Shell.Current.GoToAsync(PredictionsRoute(cleanSymbol));
    }

    public static Task OpenTradingAgentsAsync(string? symbol = null, string? companyName = null, decimal? currentPrice = null)
    {
        return Shell.Current.GoToAsync(TradingAgentsRoute(CleanSymbol(symbol), companyName, currentPrice));
    }

    private static string AssetDetailRoute(string symbol)
    {
        return $"{nameof(AssetDetailPage)}?symbol={Uri.EscapeDataString(symbol)}";
    }

    private static string PortfolioStockRoute(string symbol)
    {
        return $"{nameof(PortfolioStockPage)}?symbol={Uri.EscapeDataString(symbol)}";
    }

    private static string PriceHistoryRoute(string symbol)
    {
        return $"{nameof(PriceHistoryPage)}?symbol={Uri.EscapeDataString(symbol)}";
    }

    private static string PredictionsRoute(string symbol)
    {
        return string.IsNullOrWhiteSpace(symbol)
            ? $"//{nameof(PredictionsPage)}"
            : $"//{nameof(PredictionsPage)}?symbol={Uri.EscapeDataString(symbol)}";
    }

    private static string TradingAgentsRoute(string symbol, string? companyName, decimal? currentPrice)
    {
        var query = TradingAgentsQuery(symbol, companyName, currentPrice);
        return string.IsNullOrWhiteSpace(query) ? $"//{nameof(TradingAgentsPage)}" : $"//{nameof(TradingAgentsPage)}?{query}";
    }

    private static string TradingAgentsQuery(string symbol, string? companyName, decimal? currentPrice)
    {
        var parts = new List<string>();
        AddQuery(parts, "symbol", symbol);
        AddQuery(parts, "companyName", companyName);
        AddQuery(parts, "currentPrice", PriceText(currentPrice));
        return string.Join("&", parts);
    }

    private static void AddQuery(List<string> parts, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            parts.Add($"{key}={Uri.EscapeDataString(value.Trim())}");
        }
    }

    private static string? PriceText(decimal? value)
    {
        return value?.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static string CleanSymbol(string? symbol)
    {
        return symbol?.Trim().ToUpperInvariant() ?? string.Empty;
    }
}
