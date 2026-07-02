using api.Services;

namespace api.Endpoints;

// Read-only view of background job runs so the app can show what the jobs did.
public static class JobEndpoints
{
    public static void MapJobEndpoints(this WebApplication app)
    {
        app.MapGet("/api/jobs/runs", GetRuns);
    }

    private static Task<IResult> GetRuns(int? take, BackgroundJobRunStore store)
    {
        return DatabaseRequest.Run(async () => Results.Ok(await store.ListRecentAsync(NormalizeTake(take))));
    }

    private static int NormalizeTake(int? take)
    {
        return Math.Clamp(take ?? 20, 1, 100);
    }
}
