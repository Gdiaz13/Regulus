using System.Data.Common;
using api.Contracts;
using api.Services;

namespace api.Endpoints;

// The gateway's AI routes. The frontend posts assets here; the backend asks
// RegulasCoreAI, saves every prediction, and returns the clean overview.
public static class PredictionEndpoints
{
    public static void MapPredictionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/predict").RequireAuthorization();
        group.MapPost("", Predict);
        group.MapGet("history", History);
        group.MapGet("accuracy", Accuracy);
        group.MapGet("health", PredictHealth).AllowAnonymous();
    }

    private static async Task<IResult> Predict(
        PredictBatchRequest request,
        HttpContext context,
        RegulasAiClient client,
        PredictionStore store,
        ILoggerFactory loggers
    )
    {
        if (IsEmpty(request))
        {
            return Results.BadRequest("Send at least one asset to predict.");
        }
        return await RunPrediction(request, UserId(context), client, store, loggers);
    }

    private static async Task<IResult> RunPrediction(PredictBatchRequest request, Guid userId, RegulasAiClient client, PredictionStore store, ILoggerFactory loggers)
    {
        var overview = await CallAi(request, client, loggers);
        if (overview is null)
        {
            return AiUnavailable();
        }
        await TrySave(store, userId, overview, loggers);
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
    private static async Task TrySave(PredictionStore store, Guid userId, AiOverview overview, ILoggerFactory loggers)
    {
        try
        {
            await store.SaveAsync(userId, overview);
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

    private static Task<IResult> History(string? assetId, int? take, HttpContext context, PredictionStore store)
    {
        return DatabaseRequest.Run(async () =>
        {
            var history = await store.ListHistoryAsync(UserId(context), assetId, take);
            return Results.Ok(history);
        });
    }

    private static Task<IResult> Accuracy(
        string? assetId,
        int? take,
        HttpContext context,
        PredictionAccuracyStore store
    )
    {
        return DatabaseRequest.Run(async () =>
        {
            var accuracy = await store.ListAsync(UserId(context), assetId, take);
            return Results.Ok(accuracy);
        });
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
        return exception is DbException or InvalidOperationException;
    }

    private static ILogger Logger(ILoggerFactory loggers)
    {
        return loggers.CreateLogger("PredictionEndpoints");
    }

    private static Guid UserId(HttpContext context)
    {
        return CurrentUser.Id(context.User);
    }

    private sealed record AiHealthResponse(bool AiAvailable);
}

public sealed record PredictBatchRequest(List<PredictAssetRequest> Assets);
