using System.Text.Json;

namespace Regulas.MauiApp.Models;

public sealed record StockTradingAgentsRequest(
    string Symbol,
    string? CompanyName,
    decimal CurrentPrice,
    DateOnly? AnalysisDate
);

public sealed record StockTradingAgentsResponse(
    string Symbol,
    DateOnly AnalysisDate,
    decimal CurrentPrice,
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

public sealed record TradingAgentsHealth(bool AiAvailable);

public sealed record TradingAgentsModelInfo(
    string ModelName,
    string ModelVersion,
    string AssetType,
    string Category,
    string Purpose,
    bool IsMock
);

public sealed record TradingArgumentRow(string Text);
