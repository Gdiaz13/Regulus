using api.Contracts;

namespace api.Tests;

// Builders and canned wire data shared by the tests. Keeping them here keeps the
// individual test methods short and focused on the behaviour under test.
internal static class TestData
{
    public static AiPrediction Prediction(
        string assetId = "AMD",
        string assetType = "Stock",
        string category = "Technology",
        IEnumerable<string>? reasons = null,
        IEnumerable<string>? warnings = null)
    {
        return new AiPrediction(
            assetId, assetId + " Inc", assetType, category,
            100m, 110m, 10d, 0.6d, 0.4d, 0.7d, 0.3d, 90,
            (reasons ?? new[] { "Momentum improving" }).ToList(),
            (warnings ?? new[] { "High volatility" }).ToList(),
            "StockTechAI", "0.1.0", DateTime.UtcNow);
    }

    public static AiOverview Overview(params AiPrediction[] predictions)
    {
        var category = new AiCategoryPrediction(
            "Technology", "Stock", "summary", 0.6d, 0.4d,
            predictions.ToList(), new List<string>(),
            "StockAI", "0.1.0", DateTime.UtcNow);
        return new AiOverview(
            "overview", new List<AiCategoryPrediction> { category },
            "RegulasCoreAI", "0.1.0", DateTime.UtcNow);
    }

    // A camelCase overview exactly as the Python services send it on the wire.
    public const string OverviewJson = """
        {"summary":"ov","categories":[{"category":"Technology","assetType":"Stock","summary":"s","averageConfidence":0.6,"averageRisk":0.4,"predictions":[{"assetId":"AMD","assetName":"AMD","assetType":"Stock","category":"Technology","currentPrice":100,"predictedPrice":110,"predictedPercentChange":10,"confidenceScore":0.6,"riskScore":0.4,"bullishScore":0.7,"bearishScore":0.3,"timeHorizonDays":90,"reasons":["r"],"warnings":["w"],"modelName":"m","modelVersion":"0.1.0","createdAt":"2026-01-01T00:00:00Z"}],"warnings":[],"modelName":"StockAI","modelVersion":"0.1.0","createdAt":"2026-01-01T00:00:00Z"}],"modelName":"RegulasCoreAI","modelVersion":"0.1.0","createdAt":"2026-01-01T00:00:00Z"}
        """;
}
