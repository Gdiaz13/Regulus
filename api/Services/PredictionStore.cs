using System.Data.Common;
using api.Contracts;
using api.Models;
using Dapper;

namespace api.Services;

// Turns AI responses into request DTOs and saved prediction rows. Every saved
// row keeps reasons and warnings so Regulas can audit the model later.
public sealed class PredictionStore
{
    private const int DefaultHorizonDays = 90;
    private readonly IDatabaseConnectionFactory _factory;

    public PredictionStore(IDatabaseConnectionFactory factory)
    {
        _factory = factory;
    }

    public static List<AiPredictRequest> ToAiRequests(IEnumerable<PredictAssetRequest> assets)
    {
        return assets.Select(ToAiRequest).ToList();
    }

    public async Task<int> SaveAsync(Guid userId, AiOverview overview)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        var predictions = Flatten(overview).ToList();
        foreach (var prediction in predictions)
        {
            await SaveOneAsync(connection, userId, prediction);
        }
        return predictions.Count;
    }

    public async Task<List<SavedPredictionResponse>> ListHistoryAsync(Guid userId, string? assetId, int? take)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        var rows = (await connection.QueryAsync<PredictionRow>(Sql.History, HistoryParams(userId, assetId, take))).ToList();
        var reasons = await LoadReasons(connection, rows);
        return rows.Select(row => Response(row, reasons)).ToList();
    }

    private static async Task SaveOneAsync(DbConnection connection, Guid userId, AiPrediction prediction)
    {
        var id = await connection.ExecuteScalarAsync<long>(Sql.InsertPrediction, Params(userId, prediction));
        await SaveReasonsAsync(connection, id, prediction);
    }

    private static Task SaveReasonsAsync(DbConnection connection, long id, AiPrediction prediction)
    {
        var rows = ReasonRows(id, prediction).ToList();
        return rows.Count == 0 ? Task.CompletedTask : connection.ExecuteAsync(Sql.InsertReason, rows);
    }

    private static async Task<Dictionary<long, List<ReasonRow>>> LoadReasons(DbConnection connection, List<PredictionRow> rows)
    {
        if (rows.Count == 0)
        {
            return [];
        }
        var reasons = await connection.QueryAsync<ReasonRow>(Sql.Reasons, new { Ids = rows.Select(row => row.Id) });
        return reasons.GroupBy(reason => reason.PredictionId).ToDictionary(group => group.Key, group => group.ToList());
    }

    private static AiPredictRequest ToAiRequest(PredictAssetRequest asset)
    {
        var symbol = (asset.Symbol ?? string.Empty).Trim().ToUpperInvariant();
        return new AiPredictRequest(
            symbol, asset.Name?.Trim() ?? symbol, asset.AssetType?.Trim() ?? nameof(AssetType.Stock),
            asset.Category?.Trim() ?? string.Empty, asset.CurrentPrice, CleanHorizon(asset.TimeHorizonDays)
        );
    }

    private static int CleanHorizon(int? horizon)
    {
        return horizon is > 0 ? horizon.Value : DefaultHorizonDays;
    }

    private static IEnumerable<AiPrediction> Flatten(AiOverview overview)
    {
        return overview.Categories.SelectMany(category => category.Predictions);
    }

    private static DynamicParameters Params(Guid userId, AiPrediction prediction)
    {
        var parameters = new DynamicParameters();
        parameters.Add("UserId", userId);
        AddAssetParams(parameters, prediction);
        AddScoreParams(parameters, prediction);
        AddModelParams(parameters, prediction);
        return parameters;
    }

    private static void AddAssetParams(DynamicParameters parameters, AiPrediction prediction)
    {
        parameters.Add("AssetId", prediction.AssetId);
        parameters.Add("AssetName", prediction.AssetName);
        parameters.Add("AssetType", ParseAssetType(prediction.AssetType).ToString());
        parameters.Add("Category", prediction.Category);
        parameters.Add("CurrentPrice", prediction.CurrentPrice);
        parameters.Add("PredictedPrice", prediction.PredictedPrice);
        parameters.Add("TimeHorizonDays", prediction.TimeHorizonDays);
    }

    private static void AddScoreParams(DynamicParameters parameters, AiPrediction prediction)
    {
        parameters.Add("PredictedPercentChange", prediction.PredictedPercentChange);
        parameters.Add("ConfidenceScore", prediction.ConfidenceScore);
        parameters.Add("RiskScore", prediction.RiskScore);
        parameters.Add("BullishScore", prediction.BullishScore);
        parameters.Add("BearishScore", prediction.BearishScore);
    }

    private static void AddModelParams(DynamicParameters parameters, AiPrediction prediction)
    {
        parameters.Add("ModelName", prediction.ModelName);
        parameters.Add("ModelVersion", prediction.ModelVersion);
        parameters.Add("IsMock", IsMock(prediction.Warnings));
        parameters.Add("CreatedOn", DateTime.UtcNow);
    }

    private static object HistoryParams(Guid userId, string? assetId, int? take)
    {
        return new { UserId = userId, AssetId = CleanAssetId(assetId), Take = ClampTake(take) };
    }

    private static IEnumerable<object> ReasonRows(long id, AiPrediction prediction)
    {
        var reasons = prediction.Reasons.Select(text => NewReason(id, PredictionReasonKind.Reason, text));
        var warnings = prediction.Warnings.Select(text => NewReason(id, PredictionReasonKind.Warning, text));
        return reasons.Concat(warnings);
    }

    private static object NewReason(long predictionId, PredictionReasonKind kind, string text)
    {
        return new { PredictionId = predictionId, Kind = kind.ToString(), Text = text };
    }

    private static SavedPredictionResponse Response(PredictionRow row, Dictionary<long, List<ReasonRow>> reasons)
    {
        return new SavedPredictionResponse(
            (int)row.Id, row.AssetId, row.AssetName, row.AssetType, row.Category,
            row.CurrentPrice, row.PredictedPrice, row.PredictedPercentChange, row.ConfidenceScore,
            row.RiskScore, row.BullishScore, row.BearishScore, row.TimeHorizonDays,
            Text(reasons, row.Id, PredictionReasonKind.Reason), Text(reasons, row.Id, PredictionReasonKind.Warning),
            row.ModelName, row.ModelVersion, row.IsMock, row.CreatedOn
        );
    }

    private static List<string> Text(Dictionary<long, List<ReasonRow>> reasons, long id, PredictionReasonKind kind)
    {
        return reasons.GetValueOrDefault(id, [])
            .Where(reason => reason.Kind == kind.ToString())
            .Select(reason => reason.Text)
            .ToList();
    }

    private static bool IsMock(IEnumerable<string> warnings)
    {
        return warnings.Any(warning => warning.Contains("MOCK", StringComparison.OrdinalIgnoreCase));
    }

    private static AssetType ParseAssetType(string value)
    {
        return Enum.TryParse<AssetType>(value, ignoreCase: true, out var parsed) ? parsed : AssetType.Stock;
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
        public string Category { get; init; } = string.Empty;
        public decimal CurrentPrice { get; init; }
        public decimal PredictedPrice { get; init; }
        public double PredictedPercentChange { get; init; }
        public double ConfidenceScore { get; init; }
        public double RiskScore { get; init; }
        public double BullishScore { get; init; }
        public double BearishScore { get; init; }
        public int TimeHorizonDays { get; init; }
        public string ModelName { get; init; } = string.Empty;
        public string ModelVersion { get; init; } = string.Empty;
        public bool IsMock { get; init; }
        public DateTime CreatedOn { get; init; }
    }

    private sealed class ReasonRow
    {
        public long PredictionId { get; init; }
        public string Kind { get; init; } = string.Empty;
        public string Text { get; init; } = string.Empty;
    }

    private static class Sql
    {
        private const string Columns = """
            id as "Id", asset_id as "AssetId", asset_name as "AssetName",
            asset_type as "AssetType", category as "Category", current_price as "CurrentPrice",
            predicted_price as "PredictedPrice", predicted_percent_change as "PredictedPercentChange",
            confidence_score as "ConfidenceScore", risk_score as "RiskScore", bullish_score as "BullishScore",
            bearish_score as "BearishScore", time_horizon_days as "TimeHorizonDays",
            model_name as "ModelName", model_version as "ModelVersion",
            is_mock as "IsMock", created_on as "CreatedOn"
            """;

        public const string InsertPrediction = """
            insert into predictions
                (user_id, asset_id, asset_name, asset_type, category, current_price, predicted_price,
                 predicted_percent_change, confidence_score, risk_score, bullish_score, bearish_score,
                 time_horizon_days, model_name, model_version, is_mock, created_on)
            values
                (@UserId, @AssetId, @AssetName, @AssetType, @Category, @CurrentPrice, @PredictedPrice,
                 @PredictedPercentChange, @ConfidenceScore, @RiskScore, @BullishScore, @BearishScore,
                 @TimeHorizonDays, @ModelName, @ModelVersion, @IsMock, @CreatedOn)
            returning id;
            """;

        public const string InsertReason = """
            insert into prediction_reasons (prediction_id, kind, text)
            values (@PredictionId, @Kind, @Text);
            """;

        public const string History = $"""
            select {Columns}
            from predictions
            where user_id = @UserId and (@AssetId = '' or asset_id = @AssetId)
            order by created_on desc, id desc
            limit @Take;
            """;

        public const string Reasons = """
            select prediction_id as "PredictionId", kind as "Kind", text as "Text"
            from prediction_reasons
            where prediction_id in @Ids
            order by id;
            """;
    }
}
