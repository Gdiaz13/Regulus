namespace Regulas.MauiApp.Models;

// Mirrors api/Contracts/TradingAgentsContracts.cs. rawDecision is omitted on
// purpose: the app shows the clean summary, not the provider's raw payload.

public sealed record StockTradingAgentsRequest(
    string Symbol,
    string? CompanyName,
    decimal CurrentPrice,
    DateOnly? AnalysisDate
);

public sealed class StockTradingAgentsResult
{
    public string Symbol { get; init; } = string.Empty;
    public DateOnly AnalysisDate { get; init; }
    public decimal CurrentPrice { get; init; }
    public string Summary { get; init; } = string.Empty;
    public string Recommendation { get; init; } = string.Empty;
    public double ConfidenceScore { get; init; }
    public double RiskScore { get; init; }
    public List<string> BullishArguments { get; init; } = [];
    public List<string> BearishArguments { get; init; } = [];
    public List<string> Warnings { get; init; } = [];
    public string ModelName { get; init; } = string.Empty;
    public string ModelVersion { get; init; } = string.Empty;
    public bool IsMock { get; init; }
    public DateTime CreatedAt { get; init; }

    // Display strings live here so the XAML stays single-binding simple.
    public string RecommendationText => $"The signal suggests: {Recommendation}";
    public string ScoreText => $"Confidence {ConfidenceScore:P0} · Risk {RiskScore:P0}";
    public string ModelText => IsMock ? $"{ModelName} {ModelVersion} · MOCK DATA" : $"{ModelName} {ModelVersion}";
    public string DateText => $"Analysis date {AnalysisDate:yyyy-MM-dd} at {CurrentPrice:N2}";
    public bool HasBullish => BullishArguments.Count > 0;
    public bool HasBearish => BearishArguments.Count > 0;
    public bool HasWarnings => Warnings.Count > 0;
}
