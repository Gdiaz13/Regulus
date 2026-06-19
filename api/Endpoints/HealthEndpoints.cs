using api.Data;
using api.Services;

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
        return await db.Database.CanConnectAsync(timeout.Token).WaitAsync(DatabaseProbeTimeout);
    }

    private sealed record HealthResponse(
        string Status,
        bool DatabaseAvailable,
        bool MarketDataConfigured
    );
}
