namespace api.Services;

// Where the stock TradingAgentsAI wrapper lives. It stays separate from the
// normal backend and can later point at the real fork-backed service.
public static class TradingAgentsConfiguration
{
    private const string DefaultStockUrl = "http://localhost:8261/";

    public static Uri StockUrl(IConfiguration configuration)
    {
        return new Uri(EnsureTrailingSlash(ConfiguredUrl(configuration)));
    }

    private static string ConfiguredUrl(IConfiguration configuration)
    {
        return BlankToNull(configuration["TradingAgents:StockUrl"])
            ?? BlankToNull(configuration["TRADINGAGENTS_STOCK_AI_URL"])
            ?? DefaultStockUrl;
    }

    private static string EnsureTrailingSlash(string url)
    {
        return url.EndsWith('/') ? url : url + "/";
    }

    private static string? BlankToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
