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

    private static Task<IResult> SearchByName(string query, FinancialModelingPrepClient client)
    {
        return client.GetAsync("search-name", Query("query", query));
    }

    private static Task<IResult> GetProfile(string symbol, FinancialModelingPrepClient client)
    {
        return client.GetAsync("profile", Symbol(symbol));
    }

    private static Task<IResult> GetKeyMetrics(string symbol, FinancialModelingPrepClient client)
    {
        return client.GetAsync("key-metrics-ttm", Symbol(symbol));
    }

    private static Task<IResult> GetIncomeStatement(string symbol, FinancialModelingPrepClient client)
    {
        return client.GetAsync("income-statement", Symbol(symbol));
    }

    private static Task<IResult> GetBalanceSheet(string symbol, FinancialModelingPrepClient client)
    {
        return client.GetAsync("balance-sheet-statement", Symbol(symbol));
    }

    private static Task<IResult> GetCashFlow(string symbol, FinancialModelingPrepClient client)
    {
        return client.GetAsync("cash-flow-statement-growth", Symbol(symbol));
    }

    private static IReadOnlyDictionary<string, string?> Symbol(string symbol)
    {
        return Query("symbol", symbol);
    }

    private static IReadOnlyDictionary<string, string?> Query(string key, string value)
    {
        return new Dictionary<string, string?> { [key] = value };
    }
}
