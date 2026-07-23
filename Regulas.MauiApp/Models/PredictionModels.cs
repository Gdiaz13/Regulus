namespace Regulas.MauiApp.Models;

public sealed record PredictBatchRequest(IReadOnlyList<PredictAssetRequest> Assets);

public sealed record PredictAssetRequest(
    string Symbol,
    string? Name,
    string? AssetType,
    string? Category,
    decimal CurrentPrice,
    int? TimeHorizonDays
);

public sealed record AiOverview(
    string Summary,
    List<AiCategoryPrediction> Categories,
    string ModelName,
    string ModelVersion,
    DateTime CreatedAt
);

public sealed record AiCategoryPrediction(
    string Category,
    string AssetType,
    string Summary,
    double AverageConfidence,
    double AverageRisk,
    List<AiPrediction> Predictions,
    List<string> Warnings,
    string ModelName,
    string ModelVersion,
    DateTime CreatedAt
);

public sealed record AiPrediction(
    string AssetId,
    string AssetName,
    string AssetType,
    string Category,
    decimal CurrentPrice,
    decimal PredictedPrice,
    double PredictedPercentChange,
    double ConfidenceScore,
    double RiskScore,
    double BullishScore,
    double BearishScore,
    int TimeHorizonDays,
    List<string> Reasons,
    List<string> Warnings,
    string ModelName,
    string ModelVersion,
    DateTime CreatedAt
);

public sealed record PredictionHistoryItem(
    int Id,
    string AssetId,
    string AssetName,
    string AssetType,
    string Category,
    decimal CurrentPrice,
    decimal PredictedPrice,
    double PredictedPercentChange,
    double ConfidenceScore,
    double RiskScore,
    double BullishScore,
    double BearishScore,
    int TimeHorizonDays,
    List<string> Reasons,
    List<string> Warnings,
    string ModelName,
    string ModelVersion,
    bool IsMock,
    DateTime CreatedOn
);

public sealed record PredictionHealth(bool AiAvailable);

public sealed record PredictAssetRow(
    string Symbol,
    string Name,
    string AssetType,
    string Category,
    decimal CurrentPrice,
    int? TimeHorizonDays,
    string DisplayName,
    string PriceText,
    string HorizonText
);

public sealed record PredictionResultRow(
    string AssetId,
    string AssetName,
    string CategoryText,
    string PriceText,
    string ChangeText,
    string ScoreText,
    string ModelText,
    string ReasonText,
    string WarningText,
    bool IsMock,
    bool HasReasons,
    bool HasWarnings
);

public sealed record PredictionHistoryRow(
    string AssetId,
    string AssetName,
    string CreatedText,
    string PriceText,
    string ChangeText,
    string ScoreText,
    string ReasonText,
    string WarningText,
    bool IsMock,
    bool HasReasons,
    bool HasWarnings
);

public sealed record ModelHorizonAccuracySummary(
    string Horizon,
    int ScoredCount,
    double WinRate,
    double AverageAbsolutePercentError
);

public sealed record ModelAccuracySummary(
    string ModelName,
    int ScoredCount,
    double WinRate,
    double AverageAbsolutePercentError,
    double AveragePredictedPercentChange,
    double AverageActualPercentChange,
    double AverageTimeHorizonDays,
    double AverageConfidenceScore,
    double AverageRiskScore,
    double AverageBullishScore,
    double AverageBearishScore,
    double AverageDirectionalBias,
    double BullishBiasRate,
    double BearishBiasRate,
    double ConfidenceCalibrationError,
    double RiskCalibrationError,
    List<ModelHorizonAccuracySummary> Horizons
);

public sealed record ModelAccuracySummaryRow(
    string ModelName,
    string ScoredText,
    string WinRateText,
    string AverageErrorText,
    string ConfidenceGapText,
    string AverageHorizonText
);
