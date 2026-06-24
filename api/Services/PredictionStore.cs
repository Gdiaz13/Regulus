using api.Contracts;
using api.Data;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

// Turns AI responses into request DTOs and into saved Prediction rows. Mapping
// lives here so the endpoint stays thin and the rules are easy to test.
public static class PredictionStore
{
    private const int DefaultHorizonDays = 90;

    public static List<AiPredictRequest> ToAiRequests(IEnumerable<PredictAssetRequest> assets)
    {
        return assets.Select(ToAiRequest).ToList();
    }

    private static AiPredictRequest ToAiRequest(PredictAssetRequest asset)
    {
        var symbol = (asset.Symbol ?? string.Empty).Trim().ToUpperInvariant();
        return new AiPredictRequest(
            symbol,
            asset.Name?.Trim() ?? symbol,
            asset.AssetType?.Trim() ?? nameof(AssetType.Stock),
            asset.Category?.Trim() ?? string.Empty,
            asset.CurrentPrice,
            asset.TimeHorizonDays ?? DefaultHorizonDays
        );
    }

    public static async Task<int> SaveAsync(ApplicationDBContext db, AiOverview overview)
    {
        var predictions = Flatten(overview).Select(ToEntity).ToList();
        db.Predictions.AddRange(predictions);
        await db.SaveChangesAsync();
        return predictions.Count;
    }

    public static async Task<List<SavedPredictionResponse>> ListHistoryAsync(
        ApplicationDBContext db,
        string? assetId,
        int? take
    )
    {
        var query = HistoryQuery(db, CleanAssetId(assetId));
        var rows = await query.Take(ClampTake(take)).ToListAsync();
        return rows.Select(ToHistoryResponse).ToList();
    }

    private static IEnumerable<AiPrediction> Flatten(AiOverview overview)
    {
        return overview.Categories.SelectMany(category => category.Predictions);
    }

    private static IQueryable<Prediction> HistoryQuery(ApplicationDBContext db, string cleanAssetId)
    {
        var query = db.Predictions.Include(prediction => prediction.Reasons).AsQueryable();
        if (!string.IsNullOrWhiteSpace(cleanAssetId))
        {
            query = query.Where(prediction => prediction.AssetId == cleanAssetId);
        }
        return query.OrderByDescending(prediction => prediction.CreatedOn).ThenByDescending(prediction => prediction.Id);
    }

    private static string CleanAssetId(string? assetId)
    {
        return assetId?.Trim().ToUpperInvariant() ?? string.Empty;
    }

    private static int ClampTake(int? take)
    {
        return Math.Clamp(take ?? 25, 1, 100);
    }

    private static SavedPredictionResponse ToHistoryResponse(Prediction prediction)
    {
        return new SavedPredictionResponse(
            prediction.Id, prediction.AssetId, prediction.AssetName,
            prediction.AssetType.ToString(), prediction.Category,
            prediction.CurrentPrice, prediction.PredictedPrice,
            prediction.PredictedPercentChange, prediction.ConfidenceScore,
            prediction.RiskScore, prediction.BullishScore, prediction.BearishScore,
            prediction.TimeHorizonDays, ReasonText(prediction, PredictionReasonKind.Reason),
            ReasonText(prediction, PredictionReasonKind.Warning), prediction.ModelName,
            prediction.ModelVersion, prediction.IsMock, prediction.CreatedOn
        );
    }

    private static List<string> ReasonText(Prediction prediction, PredictionReasonKind kind)
    {
        return prediction.Reasons
            .Where(reason => reason.Kind == kind)
            .OrderBy(reason => reason.Id)
            .Select(reason => reason.Text)
            .ToList();
    }

    private static Prediction ToEntity(AiPrediction prediction)
    {
        var entity = NewPrediction(prediction);
        entity.Reasons = BuildReasons(prediction);
        return entity;
    }

    private static Prediction NewPrediction(AiPrediction prediction)
    {
        var entity = NewPredictionIdentity(prediction);
        ApplyPrices(entity, prediction);
        ApplyScores(entity, prediction);
        return entity;
    }

    private static Prediction NewPredictionIdentity(AiPrediction prediction)
    {
        return new Prediction
        {
            AssetId = prediction.AssetId,
            AssetName = prediction.AssetName,
            AssetType = ParseAssetType(prediction.AssetType),
            Category = prediction.Category,
            TimeHorizonDays = prediction.TimeHorizonDays,
            ModelName = prediction.ModelName,
            ModelVersion = prediction.ModelVersion,
            IsMock = IsMock(prediction.Warnings),
        };
    }

    private static void ApplyPrices(Prediction entity, AiPrediction prediction)
    {
        entity.CurrentPrice = prediction.CurrentPrice;
        entity.PredictedPrice = prediction.PredictedPrice;
        entity.PredictedPercentChange = prediction.PredictedPercentChange;
    }

    private static void ApplyScores(Prediction entity, AiPrediction prediction)
    {
        entity.ConfidenceScore = prediction.ConfidenceScore;
        entity.RiskScore = prediction.RiskScore;
        entity.BullishScore = prediction.BullishScore;
        entity.BearishScore = prediction.BearishScore;
    }

    private static List<PredictionReason> BuildReasons(AiPrediction prediction)
    {
        var reasons = prediction.Reasons.Select(text => NewReason(PredictionReasonKind.Reason, text));
        var warnings = prediction.Warnings.Select(text => NewReason(PredictionReasonKind.Warning, text));
        return reasons.Concat(warnings).ToList();
    }

    private static PredictionReason NewReason(PredictionReasonKind kind, string text)
    {
        return new PredictionReason { Kind = kind, Text = text };
    }

    private static bool IsMock(IEnumerable<string> warnings)
    {
        return warnings.Any(warning => warning.Contains("MOCK", StringComparison.OrdinalIgnoreCase));
    }

    private static AssetType ParseAssetType(string value)
    {
        return Enum.TryParse<AssetType>(value, ignoreCase: true, out var parsed) ? parsed : AssetType.Stock;
    }
}
