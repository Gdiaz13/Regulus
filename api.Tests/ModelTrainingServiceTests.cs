using System.Reflection;
using api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace api.Tests;

public class ModelTrainingServiceTests
{
    [Fact]
    public async Task Service_delegates_to_the_registered_training_runner()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var runner = new ModelTrainingRunner(
            new ModelTrainingDataStore(factory),
            UnusedClient(),
            new TrainedModelVersionStore(factory));
        using var services = new ServiceCollection().AddSingleton(runner).BuildServiceProvider();
        var scopes = services.GetRequiredService<IServiceScopeFactory>();
        var configuration = new ConfigurationBuilder().Build();
        var service = new ModelTrainingService(scopes, configuration, NullLogger<ModelTrainingService>.Instance);

        var outcome = await InvokeRun(service, services);

        Assert.Equal("skipped", outcome.Status);
        Assert.Equal("No stored Technology price series are ready for training.", outcome.Detail);
    }

    private static StockTechAiClient UnusedClient()
    {
        var http = new HttpClient(StubHttpMessageHandler.Throws())
        {
            BaseAddress = new Uri("http://localhost:8101/"),
        };
        return new StockTechAiClient(http);
    }

    private static Task<JobOutcome> InvokeRun(ModelTrainingService service, IServiceProvider services)
    {
        var method = typeof(ModelTrainingService).GetMethod("RunAsync", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingMethodException(nameof(ModelTrainingService), "RunAsync");
        return (Task<JobOutcome>)(method.Invoke(service, [services, CancellationToken.None])
            ?? throw new InvalidOperationException("RunAsync did not return a task."));
    }
}
