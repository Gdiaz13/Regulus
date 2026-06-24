using api.Contracts;
using api.Services;

namespace api.Endpoints;

// Gateway routes for the separate TradingAgents branch. The browser calls the
// C# API; the C# API calls StockTradingAgentsAI.
public static class TradingAgentsEndpoints
{
    public static void MapTradingAgentsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/trading-agents");
        group.MapPost("stock/analyze", AnalyzeStock);
        group.MapGet("stock/health", StockHealth);
    }

    private static async Task<IResult> AnalyzeStock(
        StockTradingAgentsRequest request,
        TradingAgentsClient client
    )
    {
        if (string.IsNullOrWhiteSpace(request.Symbol))
        {
            return Results.BadRequest("A stock symbol is required.");
        }
        return await TryAnalyze(request, client);
    }

    private static async Task<IResult> TryAnalyze(
        StockTradingAgentsRequest request,
        TradingAgentsClient client
    )
    {
        try
        {
            return Results.Ok(await client.AnalyzeStockAsync(Normalize(request)));
        }
        catch (Exception exception) when (TradingAgentsClient.IsAiException(exception))
        {
            return Unavailable();
        }
    }

    private static StockTradingAgentsRequest Normalize(StockTradingAgentsRequest request)
    {
        return request with { Symbol = request.Symbol.Trim().ToUpperInvariant() };
    }

    private static async Task<IResult> StockHealth(TradingAgentsClient client)
    {
        return Results.Ok(new TradingAgentsHealthResponse(await client.IsHealthyAsync()));
    }

    private static IResult Unavailable()
    {
        return Results.Problem("StockTradingAgentsAI is unavailable.", statusCode: 503);
    }

    private sealed record TradingAgentsHealthResponse(bool AiAvailable);
}
