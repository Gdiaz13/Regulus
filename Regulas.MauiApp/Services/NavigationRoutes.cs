namespace Regulas.MauiApp.Services;

public static class NavigationRoutes
{
    public static Task OpenAccountAsync()
    {
        return Shell.Current.GoToAsync($"//{nameof(AuthPage)}");
    }

    public static Task OpenStockDetailAsync(string? symbol)
    {
        var cleanSymbol = CleanSymbol(symbol);
        return string.IsNullOrWhiteSpace(cleanSymbol)
            ? Task.CompletedTask
            : Shell.Current.GoToAsync(StockDetailRoute(cleanSymbol));
    }

    private static string StockDetailRoute(string symbol)
    {
        return $"{nameof(StockDetailPage)}?symbol={Uri.EscapeDataString(symbol)}";
    }

    private static string CleanSymbol(string? symbol)
    {
        return symbol?.Trim().ToUpperInvariant() ?? string.Empty;
    }
}
