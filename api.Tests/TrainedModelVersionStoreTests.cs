using System.Text.Json;
using api.Contracts;
using api.Services;
using Xunit;

namespace api.Tests;

public class TrainedModelVersionStoreTests
{
    [Fact]
    public async Task Oversized_artifact_is_rejected_without_persisting()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new TrainedModelVersionStore(factory);
        var padding = new string('x', TrainedModelVersionStore.MaxArtifactBytes + 1);
        var response = Response(CompletedResponse.Replace("\"damping\":0.5", $"\"payload\":\"{padding}\""));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            store.SaveAsync("Technology", 1, response));

        Assert.Empty(await store.ListRecentAsync(10));
    }

    [Fact]
    public async Task Save_retains_only_the_latest_versions_per_model_and_category()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new TrainedModelVersionStore(factory);
        for (var index = 0; index <= TrainedModelVersionStore.VersionsRetainedPerModel; index++)
        {
            var json = CompletedResponse.Replace("0.2.0", $"0.2.{index}");
            await store.SaveAsync("Technology", 1, Response(json));
        }

        var saved = await store.ListRecentAsync(TrainedModelVersionStore.VersionsRetainedPerModel);
        Assert.Equal(TrainedModelVersionStore.VersionsRetainedPerModel, saved.Count);
        Assert.DoesNotContain(saved, version => version.ModelVersion == "0.2.0");
    }

    [Fact]
    public async Task Concurrent_saves_still_respect_the_retention_limit()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new TrainedModelVersionStore(factory);
        for (var index = 0; index < TrainedModelVersionStore.VersionsRetainedPerModel - 1; index++)
        {
            await store.SaveAsync("Technology", 1, Version(index));
        }

        await Task.WhenAll(store.SaveAsync("Technology", 1, Version(99)),
            store.SaveAsync("Technology", 1, Version(100)));

        Assert.Equal(TrainedModelVersionStore.VersionsRetainedPerModel,
            (await store.ListRecentAsync(TrainedModelVersionStore.VersionsRetainedPerModel)).Count);
    }

    private static AiTrainResponse Version(int index)
    {
        return Response(CompletedResponse.Replace("0.2.0", $"0.2.{index}"));
    }

    private static AiTrainResponse Response(string json)
    {
        return JsonSerializer.Deserialize<AiTrainResponse>(json, JsonOptions)
            ?? throw new InvalidOperationException("Test response did not deserialize.");
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private const string CompletedResponse = """
        {"status":"completed","modelName":"StockTechAI","modelVersion":"0.2.0","message":"trained","isMock":false,"contractVersion":"1.0","trained":true,"artifact":{"damping":0.5},"metrics":{"testMae":0.42,"baselineMae":0.5,"improved":true},"warnings":[]}
        """;
}
