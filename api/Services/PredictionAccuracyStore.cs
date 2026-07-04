using api.Contracts;
using Dapper;
using System.Data.Common;

namespace api.Services;

// Scores saved predictions against stored prices without loading the full price
// history table. Each limited prediction asks for its first matching future price.
public sealed class PredictionAccuracyStore
{
    private readonly IDatabaseConnectionFactory _factory;

    public PredictionAccuracyStore(IDatabaseConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<List<PredictionAccuracyResponse>> ListAsync(Guid userId, string? assetId, int? take)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await ScoredAsync(connection, userId, assetId, take);
    }

    // Rolls the per-prediction scores up per model so Regulas can rank its AIs.
    // Scoped to the current user, like every other prediction read.
    public async Task<List<ModelAccuracySummary>> SummaryAsync(Guid userId, string? assetId, int? take)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return Summarize(await ScoredAsync(connection, userId, assetId, take));
    }

    private static async Task<List<PredictionAccuracyResponse>> ScoredAsync(
        DbConnection connection,
        Guid userId,
        string? assetId,
        int? take
    )
    {
        var predictions = await LoadPredictions(connection, userId, assetId, take);
        var scored = await Task.WhenAll(predictions.Select(prediction => Score(connection, prediction)));
        return scored.OfType<PredictionAccuracyResponse>().ToList();
    }

    private static async Task<List<PredictionRow>> LoadPredictions(
        DbConnection connection,
        Guid userId,
        string? assetId,
        int? take
    )
    {
        var rows = await connection.QueryAsync<PredictionRow>(Sql.Predictions, Params(userId, assetId, take));
        return rows.ToList();
    }

    private static async Task<PredictionAccuracyResponse?> Score(DbConnection connection, PredictionRow prediction)
    {
        var targetDate = TargetDate(prediction);
        var actual = await ActualPrice(connection, prediction, targetDate);
        return actual is null ? null : Response(prediction, actual, targetDate);
    }

    private static Task<ActualPriceRow?> ActualPrice(DbConnection connection, PredictionRow prediction, DateOnly targetDate)
    {
        return connection.QuerySingleOrDefaultAsync<ActualPriceRow>(Sql.ActualPrice, ActualParams(prediction, targetDate));
    }

    private static PredictionAccuracyResponse Response(
        PredictionRow prediction,
        ActualPriceRow actual,
        DateOnly targetDate
    )
    {
        var actualPercent = AccuracyMath.PercentChange(prediction.CurrentPrice, actual.Close);
        var error = AccuracyMath.AbsoluteError(prediction.PredictedPercentChange, actualPercent);
        return BuildResponse(prediction, actual, targetDate, actualPercent, error);
    }

    private static PredictionAccuracyResponse BuildResponse(
        PredictionRow prediction, ActualPriceRow actual, DateOnly targetDate, double actualPercent, double error)
    {
        return new PredictionAccuracyResponse(
            (int)prediction.Id, prediction.AssetId, prediction.AssetName, prediction.AssetType,
            prediction.ModelName, prediction.ModelVersion, prediction.CurrentPrice, prediction.PredictedPrice,
            actual.Close, prediction.PredictedPercentChange, actualPercent, error,
            AccuracyMath.DirectionMatched(prediction.PredictedPercentChange, actualPercent),
            prediction.TimeHorizonDays, prediction.CreatedOn, targetDate, actual.Date,
            prediction.IsMock
        );
    }

    private static object Params(Guid userId, string? assetId, int? take)
    {
        return new { UserId = userId, AssetId = CleanAssetId(assetId), Take = ClampTake(take) };
    }

    private static object ActualParams(PredictionRow prediction, DateOnly targetDate)
    {
        return new { prediction.AssetId, prediction.AssetType, TargetDate = targetDate.ToDateTime(TimeOnly.MinValue) };
    }

    private static DateOnly TargetDate(PredictionRow prediction)
    {
        return AccuracyMath.TargetDate(prediction.CreatedOn, prediction.TimeHorizonDays);
    }

    private static string CleanAssetId(string? assetId)
    {
        return assetId?.Trim().ToUpperInvariant() ?? string.Empty;
    }

    private static int ClampTake(int? take)
    {
        return Math.Clamp(take ?? 25, 1, 100);
    }

    private static List<ModelAccuracySummary> Summarize(List<PredictionAccuracyResponse> scored)
    {
        return scored.GroupBy(result => result.ModelName).Select(ToSummary).OrderByDescending(summary => summary.WinRate).ToList();
    }

    private static ModelAccuracySummary ToSummary(IGrouping<string, PredictionAccuracyResponse> group)
    {
        var scored = group.ToList();
        return new ModelAccuracySummary(
            group.Key, scored.Count, WinRate(scored),
            Average(scored, result => result.AbsolutePercentError),
            Average(scored, result => result.PredictedPercentChange),
            Average(scored, result => result.ActualPercentChange)
        );
    }

    private static double WinRate(List<PredictionAccuracyResponse> scored)
    {
        return Math.Round(scored.Count(result => result.DirectionMatched) * 100.0 / scored.Count, 2);
    }

    private static double Average(List<PredictionAccuracyResponse> scored, Func<PredictionAccuracyResponse, double> select)
    {
        return Math.Round(scored.Average(select), 2);
    }

    private sealed class PredictionRow
    {
        public long Id { get; init; }
        public string AssetId { get; init; } = string.Empty;
        public string AssetName { get; init; } = string.Empty;
        public string AssetType { get; init; } = string.Empty;
        public decimal CurrentPrice { get; init; }
        public decimal PredictedPrice { get; init; }
        public double PredictedPercentChange { get; init; }
        public int TimeHorizonDays { get; init; }
        public string ModelName { get; init; } = string.Empty;
        public string ModelVersion { get; init; } = string.Empty;
        public bool IsMock { get; init; }
        public DateTime CreatedOn { get; init; }
    }

    // Npgsql returns PostgreSQL date columns as DateOnly, so the row matches that.
    private sealed class ActualPriceRow
    {
        public DateOnly Date { get; init; }
        public decimal Close { get; init; }
    }

    private static class Sql
    {
        public const string Predictions = """
            select id as "Id", asset_id as "AssetId", asset_name as "AssetName",
                   asset_type as "AssetType", current_price as "CurrentPrice",
                   predicted_price as "PredictedPrice",
                   predicted_percent_change as "PredictedPercentChange",
                   time_horizon_days as "TimeHorizonDays", model_name as "ModelName",
                   model_version as "ModelVersion", is_mock as "IsMock", created_on as "CreatedOn"
            from predictions
            where user_id = @UserId and (@AssetId = '' or asset_id = @AssetId)
            order by created_on desc, id desc
            limit @Take;
            """;

        public const string ActualPrice = """
            select p.date as "Date", p.close_price as "Close"
            from price_history p
            join assets a on a.id = p.asset_id
            where a.symbol = @AssetId and a.asset_type = @AssetType and p.date >= @TargetDate
            order by p.date
            limit 1;
            """;
    }
}
