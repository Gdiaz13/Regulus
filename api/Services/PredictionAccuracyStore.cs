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

    public async Task<List<PredictionAccuracyResponse>> ListAsync(string? assetId, int? take)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        var predictions = await LoadPredictions(connection, assetId, take);
        var scored = await Task.WhenAll(predictions.Select(prediction => Score(connection, prediction)));
        return scored.OfType<PredictionAccuracyResponse>().ToList();
    }

    private static async Task<List<PredictionRow>> LoadPredictions(DbConnection connection, string? assetId, int? take)
    {
        var rows = await connection.QueryAsync<PredictionRow>(Sql.Predictions, Params(assetId, take));
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
        var actualPercent = PercentChange(prediction.CurrentPrice, actual.Close);
        var error = Math.Abs(actualPercent - prediction.PredictedPercentChange);
        return BuildResponse(prediction, actual, targetDate, actualPercent, error);
    }

    private static PredictionAccuracyResponse BuildResponse(
        PredictionRow prediction, ActualPriceRow actual, DateOnly targetDate, double actualPercent, double error)
    {
        return new PredictionAccuracyResponse(
            (int)prediction.Id, prediction.AssetId, prediction.AssetName, prediction.AssetType,
            prediction.ModelName, prediction.ModelVersion, prediction.CurrentPrice, prediction.PredictedPrice,
            actual.Close, prediction.PredictedPercentChange, actualPercent, error,
            Direction(prediction.PredictedPercentChange) == Direction(actualPercent),
            prediction.TimeHorizonDays, prediction.CreatedOn, targetDate, DateOnly.FromDateTime(actual.Date),
            prediction.IsMock
        );
    }

    private static object Params(string? assetId, int? take)
    {
        return new { AssetId = CleanAssetId(assetId), Take = ClampTake(take) };
    }

    private static object ActualParams(PredictionRow prediction, DateOnly targetDate)
    {
        return new { prediction.AssetId, prediction.AssetType, TargetDate = targetDate.ToDateTime(TimeOnly.MinValue) };
    }

    private static DateOnly TargetDate(PredictionRow prediction)
    {
        return DateOnly.FromDateTime(prediction.CreatedOn.Date).AddDays(prediction.TimeHorizonDays);
    }

    private static double PercentChange(decimal start, decimal finish)
    {
        return start == 0 ? 0 : Math.Round((double)((finish - start) / start * 100), 2);
    }

    private static int Direction(double percent)
    {
        return percent.CompareTo(0);
    }

    private static string CleanAssetId(string? assetId)
    {
        return assetId?.Trim().ToUpperInvariant() ?? string.Empty;
    }

    private static int ClampTake(int? take)
    {
        return Math.Clamp(take ?? 25, 1, 100);
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

    private sealed class ActualPriceRow
    {
        public DateTime Date { get; init; }
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
            where @AssetId = '' or asset_id = @AssetId
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
