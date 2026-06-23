using api.Data;
using api.Services;
using Microsoft.EntityFrameworkCore;

namespace api.Endpoints;

public static class HealthEndpoints
{
    private static readonly TimeSpan DatabaseProbeTimeout = TimeSpan.FromSeconds(2);

    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("/api/health", GetHealth);
    }

    private static async Task<IResult> GetHealth(
        ApplicationDBContext db,
        IConfiguration configuration
    )
    {
        var databaseAvailable = await CanConnect(db);
        var marketDataConfigured = MarketDataConfiguration.HasApiKey(configuration);
        return Results.Ok(new HealthResponse("ok", databaseAvailable, marketDataConfigured));
    }

    private static async Task<bool> CanConnect(ApplicationDBContext db)
    {
        try
        {
            return await ProbeDatabase(db);
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static async Task<bool> ProbeDatabase(ApplicationDBContext db)
    {
        using var timeout = new CancellationTokenSource(DatabaseProbeTimeout);
        if (!await db.Database.CanConnectAsync(timeout.Token).WaitAsync(DatabaseProbeTimeout))
        {
            return false;
        }
        return await CanQueryPortfolioTable(db, timeout.Token);
    }

    // Health checks the same table the portfolio screens need.
    private static async Task<bool> CanQueryPortfolioTable(
        ApplicationDBContext db,
        CancellationToken token
    )
    {
        await db.Stocks.AsNoTracking().Select(stock => stock.Id).FirstOrDefaultAsync(token);
        return true;
    }

    private sealed record HealthResponse(
        string Status,
        bool DatabaseAvailable,
        bool MarketDataConfigured
    );
}
