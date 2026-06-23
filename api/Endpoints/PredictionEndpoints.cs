using System.Data.Common;
using api.Contracts;
using api.Data;
using api.Services;
using Microsoft.EntityFrameworkCore;

namespace api.Endpoints;

// The gateway's AI routes. The frontend posts assets here; the backend asks
// RegulasCoreAI, saves every prediction, and returns the clean overview.
public static class PredictionEndpoints
{
    public static void MapPredictionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/predict");
        group.MapPost("", Predict);
        group.MapGet("health", PredictHealth);
    }

    private static async Task<IResult> Predict(
        PredictBatchRequest request,
        RegulasAiClient client,
        ApplicationDBContext db,
        ILoggerFactory loggers
    )
    {
        if (IsEmpty(request))
        {
            return Results.BadRequest("Send at least one asset to predict.");
        }
        return await RunPrediction(request, client, db, loggers);
    }

    private static async Task<IResult> RunPrediction(
        PredictBatchRequest request,
        RegulasAiClient client,
        ApplicationDBContext db,
        ILoggerFactory loggers
    )
    {
        var overview = await CallAi(request, client, loggers);
        if (overview is null)
        {
            return AiUnavailable();
        }
        await TrySave(db, overview, loggers);
        return Results.Ok(overview);
    }

    private static async Task<AiOverview?> CallAi(PredictBatchRequest request, RegulasAiClient client, ILoggerFactory loggers)
    {
        try
        {
            return await client.PredictAsync(PredictionStore.ToAiRequests(request.Assets));
        }
        catch (Exception exception) when (RegulasAiClient.IsAiException(exception))
        {
            Logger(loggers).LogWarning("RegulasCoreAI is unavailable.");
            return null;
        }
    }

    // Saving must never lose the user's prediction response. If the database is
    // down we still return the prediction and just log that it was not stored.
    private static async Task TrySave(ApplicationDBContext db, AiOverview overview, ILoggerFactory loggers)
    {
        try
        {
            await PredictionStore.SaveAsync(db, overview);
        }
        catch (Exception exception) when (IsDatabaseException(exception))
        {
            Logger(loggers).LogWarning("Prediction was not saved because the database is unavailable.");
        }
    }

    private static async Task<IResult> PredictHealth(RegulasAiClient client)
    {
        var healthy = await client.IsHealthyAsync();
        return Results.Ok(new AiHealthResponse(healthy));
    }

    private static bool IsEmpty(PredictBatchRequest request)
    {
        return request.Assets is null || request.Assets.Count == 0;
    }

    private static IResult AiUnavailable()
    {
        return Results.Problem("The Regulas AI service is unavailable.", statusCode: 503);
    }

    private static bool IsDatabaseException(Exception exception)
    {
        return exception is DbException or DbUpdateException or InvalidOperationException;
    }

    private static ILogger Logger(ILoggerFactory loggers)
    {
        return loggers.CreateLogger("PredictionEndpoints");
    }

    private sealed record AiHealthResponse(bool AiAvailable);
}

public sealed record PredictBatchRequest(List<PredictAssetRequest> Assets);
