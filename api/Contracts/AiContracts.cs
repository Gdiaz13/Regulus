namespace api.Contracts;

// These records mirror the shared Python AI contract in ai/regulas_ai_core.
// They are the wire format between the C# gateway and the AI services. If a
// field changes in the Python contract, change it here too.

// What the frontend posts to the gateway for one asset.
public sealed record PredictAssetRequest(
    string Symbol,
    string? Name,
    string? AssetType,
    string? Category,
    decimal CurrentPrice,
    int? TimeHorizonDays
);

// What the gateway sends down to the AI services for one asset.
public sealed record AiPredictRequest(
    string AssetId,
    string AssetName,
    string AssetType,
    string Category,
    decimal CurrentPrice,
    int TimeHorizonDays
);

// One specialist prediction coming back up the hierarchy.
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

// A category AI's summary of its specialists.
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

// RegulasCoreAI's final overview across every category AI.
public sealed record AiOverview(
    string Summary,
    List<AiCategoryPrediction> Categories,
    string ModelName,
    string ModelVersion,
    DateTime CreatedAt
);
