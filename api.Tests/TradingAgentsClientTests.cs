using System.Net;
using api.Contracts;
using api.Services;
using Xunit;

namespace api.Tests;

public class TradingAgentsClientTests
{
    private const string AnalysisJson = """
        {"symbol":"AMD","analysisDate":"2026-01-01","summary":"s","recommendation":"research-only hold/watch","confidenceScore":0.58,"riskScore":0.62,"bullishArguments":["b"],"bearishArguments":["r"],"warnings":["MOCK DATA"],"rawDecision":{"source":"mock"},"modelName":"StockTradingAgentsAI","modelVersion":"0.1.0","isMock":true,"createdAt":"2026-01-01T00:00:00Z"}
        """;

    [Fact]
    public async Task AnalyzeStockAsync_deserializes_camelCase_response()
    {
        var client = ClientWith(StubHttpMessageHandler.Json(AnalysisJson));
        var result = await client.AnalyzeStockAsync(new StockTradingAgentsRequest("AMD", null, 100m, null));
        Assert.Equal("StockTradingAgentsAI", result!.ModelName);
        Assert.True(result.IsMock);
    }

    [Fact]
    public async Task IsHealthyAsync_true_on_success()
    {
        var client = ClientWith(StubHttpMessageHandler.Status(HttpStatusCode.OK));
        Assert.True(await client.IsHealthyAsync());
    }

    [Fact]
    public async Task IsHealthyAsync_false_when_service_unreachable()
    {
        var client = ClientWith(StubHttpMessageHandler.Throws());
        Assert.False(await client.IsHealthyAsync());
    }

    private static TradingAgentsClient ClientWith(StubHttpMessageHandler handler)
    {
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:8261/") };
        return new TradingAgentsClient(http);
    }
}
