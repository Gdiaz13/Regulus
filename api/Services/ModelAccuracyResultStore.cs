using System.Data.Common;
using api.Models;
using Dapper;

namespace api.Services;

// Persists scored predictions so accuracy history accumulates over time.
// The scoring job writes here; the API reads the user's saved results back.
public sealed class ModelAccuracyResultStore
{
    private const int BatchSize = 100;
    private readonly IDatabaseConnectionFactory _factory;

    public ModelAccuracyResultStore(IDatabaseConnectionFactory factory)
    {
        _factory = factory;
    }

    // Scores every not-yet-scored prediction that has a stored price on or after
    // its target date. Predictions that have not matured yet are simply skipped.
    public async Task<int> ScorePendingAsync(CancellationToken token = default)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync(token);
        var pending = await connection.QueryAsync<PendingPrediction>(Sql.Unscored, new { Take = BatchSize });
        var scored = 0;
        foreach (var prediction in pending)
        {
            scored += await ScoreOneAsync(connection, prediction);
        }
        return scored;
    }

    public async Task<List<ModelAccuracyResult>> ListResultsAsync(Guid userId, int? take, CancellationToken token = default)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync(token);
        var rows = await connection.QueryAsync<ModelAccuracyResult>(Sql.ListResults, ListParams(userId, take));
        return rows.ToList();
    }

    private static async Task<int> ScoreOneAsync(DbConnection connection, PendingPrediction prediction)
    {
        var targetDate = AccuracyMath.TargetDate(prediction.CreatedOn, prediction.TimeHorizonDays);
        var actual = await ActualPrice(connection, prediction, targetDate);
        return actual is null ? 0 : await connection.ExecuteAsync(Sql.Insert, InsertParams(prediction, actual, targetDate));
    }

    private static Task<ActualPriceRow?> ActualPrice(DbConnection connection, PendingPrediction prediction, DateOnly targetDate)
    {
        var parameters = new { prediction.AssetId, prediction.AssetType, TargetDate = targetDate.ToDateTime(TimeOnly.MinValue) };
        return connection.QuerySingleOrDefaultAsync<ActualPriceRow>(Sql.ActualPrice, parameters);
    }

    private static DynamicParameters InsertParams(PendingPrediction prediction, ActualPriceRow actual, DateOnly targetDate)
    {
        var parameters = new DynamicParameters();
        AddPredictionParams(parameters, prediction);
        AddScoreParams(parameters, prediction, actual, targetDate);
        return parameters;
    }

    private static void AddPredictionParams(DynamicParameters parameters, PendingPrediction prediction)
    {
        parameters.Add("PredictionId", prediction.PredictionId);
        parameters.Add("UserId", prediction.UserId);
        parameters.Add("AssetId", prediction.AssetId);
        parameters.Add("AssetType", prediction.AssetType);
        parameters.Add("ModelName", prediction.ModelName);
        parameters.Add("ModelVersion", prediction.ModelVersion);
        parameters.Add("PredictedPercentChange", prediction.PredictedPercentChange);
        parameters.Add("IsMock", prediction.IsMock);
        parameters.Add("PredictedOn", prediction.CreatedOn);
    }

    private static void AddScoreParams(DynamicParameters parameters, PendingPrediction prediction, ActualPriceRow actual, DateOnly targetDate)
    {
        var actualPercent = AccuracyMath.PercentChange(prediction.CurrentPrice, actual.Close);
        parameters.Add("ActualPercentChange", actualPercent);
        parameters.Add("AbsolutePercentError", AccuracyMath.AbsoluteError(prediction.PredictedPercentChange, actualPercent));
        parameters.Add("DirectionMatched", AccuracyMath.DirectionMatched(prediction.PredictedPercentChange, actualPercent));
        parameters.Add("ActualPrice", actual.Close);
        parameters.Add("TargetDate", targetDate.ToDateTime(TimeOnly.MinValue));
        parameters.Add("ActualDate", actual.Date.ToDateTime(TimeOnly.MinValue));
        parameters.Add("ScoredAt", DateTime.UtcNow);
    }

    private static object ListParams(Guid userId, int? take)
    {
        return new { UserId = userId, Take = Math.Clamp(take ?? 25, 1, 100) };
    }

    private sealed class PendingPrediction
    {
        public long PredictionId { get; init; }
        public Guid UserId { get; init; }
        public string AssetId { get; init; } = string.Empty;
        public string AssetType { get; init; } = string.Empty;
        public decimal CurrentPrice { get; init; }
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

    // Timestamps are passed from C# (not now()) so the same SQL runs on
    // PostgreSQL and the SQLite test database.
    private static class Sql
    {
        public const string Unscored = """
            select p.id as "PredictionId", p.user_id as "UserId", p.asset_id as "AssetId",
                   p.asset_type as "AssetType", p.current_price as "CurrentPrice",
                   p.predicted_percent_change as "PredictedPercentChange",
                   p.time_horizon_days as "TimeHorizonDays", p.model_name as "ModelName",
                   p.model_version as "ModelVersion", p.is_mock as "IsMock", p.created_on as "CreatedOn"
            from predictions p
            left join model_accuracy_results r on r.prediction_id = p.id
            where r.id is null
            order by p.created_on, p.id
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

        public const string Insert = """
            insert into model_accuracy_results
                (prediction_id, user_id, asset_id, asset_type, model_name, model_version,
                 predicted_percent_change, actual_percent_change, absolute_percent_error,
                 direction_matched, actual_price, target_date, actual_date, is_mock, predicted_on, scored_at)
            values
                (@PredictionId, @UserId, @AssetId, @AssetType, @ModelName, @ModelVersion,
                 @PredictedPercentChange, @ActualPercentChange, @AbsolutePercentError,
                 @DirectionMatched, @ActualPrice, @TargetDate, @ActualDate, @IsMock, @PredictedOn, @ScoredAt)
            on conflict (prediction_id) do nothing;
            """;

        public const string ListResults = """
            select id as "Id", prediction_id as "PredictionId", user_id as "UserId",
                   asset_id as "AssetId", asset_type as "AssetType", model_name as "ModelName",
                   model_version as "ModelVersion", predicted_percent_change as "PredictedPercentChange",
                   actual_percent_change as "ActualPercentChange", absolute_percent_error as "AbsolutePercentError",
                   direction_matched as "DirectionMatched", actual_price as "ActualPrice",
                   target_date as "TargetDate", actual_date as "ActualDate", is_mock as "IsMock",
                   predicted_on as "PredictedOn", scored_at as "ScoredAt"
            from model_accuracy_results
            where user_id = @UserId
            order by scored_at desc, id desc
            limit @Take;
            """;
    }
}
