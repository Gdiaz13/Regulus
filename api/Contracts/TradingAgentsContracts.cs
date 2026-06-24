using System.Text.Json;

namespace api.Contracts;

public sealed record StockTradingAgentsRequest(
    string Symbol,
    string? CompanyName,
    decimal CurrentPrice,
    DateOnly? AnalysisDate
);

public sealed record StockTradingAgentsResponse(
    string Symbol,
    DateOnly AnalysisDate,
    string Summary,
    string Recommendation,
    double ConfidenceScore,
    double RiskScore,
    List<string> BullishArguments,
    List<string> BearishArguments,
    List<string> Warnings,
    JsonElement? RawDecision,
    string ModelName,
    string ModelVersion,
    bool IsMock,
    DateTime CreatedAt
);
