using System.Net;
using api.Contracts;
using api.Services;
using Xunit;

namespace api.Tests;

// Covers RegulasAiClient: the one service that talks HTTP to RegulasCoreAI.
public class RegulasAiClientTests
{
    private static RegulasAiClient ClientWith(StubHttpMessageHandler handler)
    {
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:8301/") };
        return new RegulasAiClient(http);
    }

    [Fact]
    public async Task PredictAsync_deserializes_camelCase_overview_from_the_wire()
    {
        var client = ClientWith(StubHttpMessageHandler.Json(TestData.OverviewJson));
        var overview = await client.PredictAsync(new List<AiPredictRequest>());
        Assert.Equal("RegulasCoreAI", overview!.ModelName);
        Assert.Equal("AMD", overview.Categories.Single().Predictions.Single().AssetId);
    }

    [Fact]
    public async Task IsHealthyAsync_true_on_success()
    {
        var client = ClientWith(StubHttpMessageHandler.Status(HttpStatusCode.OK));
        Assert.True(await client.IsHealthyAsync());
    }

    [Fact]
    public async Task IsHealthyAsync_false_on_error_status()
    {
        var client = ClientWith(StubHttpMessageHandler.Status(HttpStatusCode.InternalServerError));
        Assert.False(await client.IsHealthyAsync());
    }

    [Fact]
    public async Task IsHealthyAsync_false_when_service_unreachable()
    {
        var client = ClientWith(StubHttpMessageHandler.Throws());
        Assert.False(await client.IsHealthyAsync());
    }

    [Theory]
    [InlineData(typeof(HttpRequestException), true)]
    [InlineData(typeof(TaskCanceledException), true)]
    [InlineData(typeof(InvalidOperationException), false)]
    public void IsAiException_only_matches_connection_failures(Type type, bool expected)
    {
        var exception = (Exception)Activator.CreateInstance(type)!;
        Assert.Equal(expected, RegulasAiClient.IsAiException(exception));
    }
}
