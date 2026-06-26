using api.Services;

namespace api.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("/api/health", GetHealth);
    }

    private static async Task<IResult> GetHealth(
        PostgresHealthCheck database,
        IConfiguration configuration
    )
    {
        var databaseAvailable = await database.IsAvailableAsync();
        var marketDataConfigured = MarketDataConfiguration.HasApiKey(configuration);
        return Results.Ok(new HealthResponse("ok", databaseAvailable, marketDataConfigured));
    }

    private sealed record HealthResponse(
        string Status,
        bool DatabaseAvailable,
        bool MarketDataConfigured
    );
}
