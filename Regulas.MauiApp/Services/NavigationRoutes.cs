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

    private static string AssetDetailRoute(string symbol)
    {
        return $"{nameof(AssetDetailPage)}?symbol={Uri.EscapeDataString(symbol)}";
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

    private static string CleanSymbol(string? symbol)
    {
        return symbol?.Trim().ToUpperInvariant() ?? string.Empty;
    }
}
