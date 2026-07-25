using System.Net;
using System.Text;
using api.Contracts;
using api.Models;
using api.Services;
using Xunit;

namespace api.Tests;

public class ModelTrainingRunnerTests
{
    [Fact]
    public async Task Stored_technology_series_complete_a_real_training_run()
    {
        using var factory = new SqliteDapperConnectionFactory();
        await SaveData(factory);
        var client = Client(CompletedResponse);
        var runner = new ModelTrainingRunner(new ModelTrainingDataStore(factory), client);

        var outcome = await runner.RunAsync(CancellationToken.None);

        Assert.Equal("completed", outcome.Status);
        Assert.Equal(1, outcome.Count);
        Assert.Contains("StockTechAI 0.2.0", outcome.Detail);
    }

    [Fact]
    public async Task Insufficient_data_response_is_skipped()
    {
        using var factory = new SqliteDapperConnectionFactory();
        await SaveData(factory);
        var runner = new ModelTrainingRunner(new ModelTrainingDataStore(factory), Client(InsufficientResponse));

        var outcome = await runner.RunAsync(CancellationToken.None);

        Assert.Equal("skipped", outcome.Status);
        Assert.Equal(0, outcome.Count);
    }

    private static StockTechAiClient Client(string json)
    {
        var http = new HttpClient(StubHttpMessageHandler.Json(json))
        {
            BaseAddress = new Uri("http://localhost:8101/"),
        };
        return new StockTechAiClient(http);
    }

    private static async Task SaveData(SqliteDapperConnectionFactory factory)
    {
        var prediction = TestData.Prediction(assetId: "AMD", category: "Technology");
        await new PredictionStore(factory).SaveAsync(TestUsers.AliceId, TestData.Overview(prediction));
        var store = new PriceHistoryStore(factory);
        var asset = await store.EnsureAssetAsync("AMD", AssetType.Stock, "AMD");
        var prices = Enumerable.Range(0, 35).Select(index => Price(index));
        await store.SaveAsync(asset.Id, prices);
    }

    private static FmpHistoricalPrice Price(int index)
    {
        var close = 100m + index;
        return new FmpHistoricalPrice("AMD", new DateOnly(2026, 1, 1).AddDays(index), close, close, close, close, 1000);
    }

    private const string CompletedResponse = """
        {"status":"completed","modelName":"StockTechAI","modelVersion":"0.2.0","message":"trained","isMock":false,"contractVersion":"1.0","trained":true,"artifact":{"damping":0.5},"metrics":{"testMae":0.42,"baselineMae":0.5,"improved":true},"warnings":[]}
        """;

    private const string InsufficientResponse = """
        {"status":"insufficient-data","modelName":"StockTechAI","modelVersion":"0.2.0","message":"not enough data","isMock":false,"contractVersion":"1.0","trained":false,"artifact":null,"metrics":null,"warnings":["need more samples"]}
        """;
}
