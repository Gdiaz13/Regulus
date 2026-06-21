using api.Services;

namespace api.Endpoints;

public static class MarketDataEndpoints
{
    public static void MapMarketDataEndpoints(this WebApplication app)
    {
        var marketData = app.MapGroup("/api/market-data");
        marketData.MapGet("search-name", SearchByName);
        marketData.MapGet("profile", GetProfile);
        marketData.MapGet("key-metrics-ttm", GetKeyMetrics);
        marketData.MapGet("income-statement", GetIncomeStatement);
        marketData.MapGet("balance-sheet-statement", GetBalanceSheet);
        marketData.MapGet("cash-flow-statement-growth", GetCashFlow);
    }

    private static Task<IResult> SearchByName(string? query, FinancialModelingPrepClient client)
    {
        return ForwardValue("search-name", "query", query, "Search query is required.", client);
    }

    private static Task<IResult> GetProfile(string? symbol, FinancialModelingPrepClient client)
    {
        return ForwardSymbol("profile", symbol, client);
    }

    private static Task<IResult> GetKeyMetrics(string? symbol, FinancialModelingPrepClient client)
    {
        return ForwardSymbol("key-metrics-ttm", symbol, client);
    }

    private static Task<IResult> GetIncomeStatement(string? symbol, FinancialModelingPrepClient client)
    {
        return ForwardSymbol("income-statement", symbol, client);
    }

    private static Task<IResult> GetBalanceSheet(string? symbol, FinancialModelingPrepClient client)
    {
        return ForwardSymbol("balance-sheet-statement", symbol, client);
    }

    private static Task<IResult> GetCashFlow(string? symbol, FinancialModelingPrepClient client)
    {
        return ForwardSymbol("cash-flow-statement-growth", symbol, client);
    }

    private static Task<IResult> ForwardSymbol(
        string path,
        string? symbol,
        FinancialModelingPrepClient client
    )
    {
        return ForwardValue(path, "symbol", symbol, "A stock symbol is required.", client);
    }

    // Keep validation here so bad requests never leave our API proxy.
    private static Task<IResult> ForwardValue(
        string path,
        string key,
        string? value,
        string message,
        FinancialModelingPrepClient client
    )
    {
        var cleanValue = Clean(value);
        return string.IsNullOrWhiteSpace(cleanValue)
            ? BadRequest(message)
            : client.GetAsync(path, Query(key, cleanValue));
    }

    private static string Clean(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }

    private static Task<IResult> BadRequest(string message)
    {
        return Task.FromResult<IResult>(Results.BadRequest(message));
    }

    private static IReadOnlyDictionary<string, string?> Query(string key, string value)
    {
        return new Dictionary<string, string?> { [key] = value };
    }
}
