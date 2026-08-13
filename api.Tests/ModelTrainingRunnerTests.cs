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
        var runner = Runner(factory, client);

        var outcome = await runner.RunAsync(CancellationToken.None);

        Assert.Equal("completed", outcome.Status);
        Assert.Equal(1, outcome.Count);
        Assert.Contains("StockTechAI 0.2.0", outcome.Detail);
    }

    [Fact]
    public async Task Improved_training_persists_a_promotion_eligible_model_version()
    {
        using var factory = new SqliteDapperConnectionFactory();
        await SaveData(factory);
        var versions = new TrainedModelVersionStore(factory);
        var runner = Runner(factory, Client(CompletedResponse), versions);

        await runner.RunAsync(CancellationToken.None);

        var version = Assert.Single(await versions.ListRecentAsync(10));
        AssertImprovedVersion(version);
    }

    [Fact]
    public async Task Non_improved_training_is_persisted_but_not_promotion_eligible()
    {
        using var factory = new SqliteDapperConnectionFactory();
        await SaveData(factory);
        var versions = new TrainedModelVersionStore(factory);

        await Runner(factory, Client(NotImprovedResponse), versions).RunAsync(CancellationToken.None);

        Assert.False(Assert.Single(await versions.ListRecentAsync(10)).PromotionEligible);
    }

    [Fact]
    public async Task Reported_improvement_without_better_mae_is_not_eligible()
    {
        using var factory = new SqliteDapperConnectionFactory();
        await SaveData(factory);
        var versions = new TrainedModelVersionStore(factory);

        await Runner(factory, Client(InconsistentResponse), versions).RunAsync(CancellationToken.None);

        Assert.False(Assert.Single(await versions.ListRecentAsync(10)).PromotionEligible);
    }

    [Fact]
    public async Task Insufficient_data_response_is_skipped()
    {
        using var factory = new SqliteDapperConnectionFactory();
        await SaveData(factory);
        var versions = new TrainedModelVersionStore(factory);
        var runner = Runner(factory, Client(InsufficientResponse), versions);

        var outcome = await runner.RunAsync(CancellationToken.None);

        Assert.Equal("skipped", outcome.Status);
        Assert.Equal(0, outcome.Count);
        Assert.Empty(await versions.ListRecentAsync(10));
    }

    [Fact]
    public async Task Trained_response_without_an_artifact_fails_without_persisting()
    {
        using var factory = new SqliteDapperConnectionFactory();
        await SaveData(factory);
        var versions = new TrainedModelVersionStore(factory);
        var runner = Runner(factory, Client(MissingArtifactResponse), versions);

        await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync(CancellationToken.None));

        Assert.Empty(await versions.ListRecentAsync(10));
    }

    [Theory]
    [MemberData(nameof(MalformedResponses))]
    public async Task Malformed_trained_response_fails_without_persisting(string response)
    {
        using var factory = new SqliteDapperConnectionFactory();
        await SaveData(factory);
        var versions = new TrainedModelVersionStore(factory);
        var runner = Runner(factory, Client(response), versions);

        await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync(CancellationToken.None));

        Assert.Empty(await versions.ListRecentAsync(10));
    }

    private static StockTechAiClient Client(string json)
    {
        var http = new HttpClient(StubHttpMessageHandler.Json(json))
        {
            BaseAddress = new Uri("http://localhost:8101/"),
        };
        return new StockTechAiClient(http);
    }

    private static ModelTrainingRunner Runner(
        SqliteDapperConnectionFactory factory,
        StockTechAiClient client,
        TrainedModelVersionStore? versions = null)
    {
        return new ModelTrainingRunner(
            new ModelTrainingDataStore(factory),
            client,
            versions ?? new TrainedModelVersionStore(factory));
    }

    private static void AssertImprovedVersion(TrainedModelVersion version)
    {
        Assert.True(version.PromotionEligible);
        Assert.Equal("StockTechAI", version.ModelName);
        Assert.Equal("Technology", version.Category);
        Assert.Equal(1, version.SeriesCount);
        Assert.Contains("\"damping\":0.5", version.ArtifactJson);
        Assert.Contains("\"improved\":true", version.MetricsJson);
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

    private const string NotImprovedResponse = """
        {"status":"completed","modelName":"StockTechAI","modelVersion":"0.2.0","message":"trained","isMock":false,"contractVersion":"1.0","trained":true,"artifact":{"damping":1.0},"metrics":{"testMae":0.6,"baselineMae":0.5,"improved":false},"warnings":[]}
        """;

    private const string MissingArtifactResponse = """
        {"status":"completed","modelName":"StockTechAI","modelVersion":"0.2.0","message":"trained","isMock":false,"contractVersion":"1.0","trained":true,"artifact":null,"metrics":{"testMae":0.42,"baselineMae":0.5,"improved":true},"warnings":[]}
        """;

    private const string InconsistentResponse = """
        {"status":"completed","modelName":"StockTechAI","modelVersion":"0.2.0","message":"trained","isMock":false,"contractVersion":"1.0","trained":true,"artifact":{"damping":1.0},"metrics":{"testMae":0.6,"baselineMae":0.5,"improved":true},"warnings":[]}
        """;

    public static TheoryData<string> MalformedResponses => new()
    {
        "{}",
        CompletedResponse.Replace("\"status\":\"completed\"", "\"status\":\"failed\""),
        CompletedResponse.Replace("\"modelName\":\"StockTechAI\"", "\"modelName\":\"OtherAI\""),
        CompletedResponse.Replace("\"modelVersion\":\"0.2.0\"", "\"modelVersion\":\"\""),
        CompletedResponse.Replace("\"contractVersion\":\"1.0\"", "\"contractVersion\":\"2.0\""),
        CompletedResponse.Replace("{\"damping\":0.5}", "[]"),
        CompletedResponse.Replace("{\"testMae\":0.42,\"baselineMae\":0.5,\"improved\":true}", "[]"),
        CompletedResponse.Replace("\"baselineMae\":0.5,", string.Empty),
        InsufficientResponse.Replace("\"modelName\":\"StockTechAI\"", "\"modelName\":\"OtherAI\""),
        InsufficientResponse.Replace("\"contractVersion\":\"1.0\"", "\"contractVersion\":\"2.0\""),
        InsufficientResponse.Replace("\"modelVersion\":\"0.2.0\"", "\"modelVersion\":\"\""),
        InsufficientResponse.Replace("\"status\":\"insufficient-data\"", "\"status\":\"failed\""),
        InsufficientResponse.Replace("\"isMock\":false", "\"isMock\":true"),
    };
}
