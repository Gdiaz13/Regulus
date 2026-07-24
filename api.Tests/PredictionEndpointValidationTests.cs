using System.Data.Common;
using System.Reflection;
using api.Contracts;
using api.Endpoints;
using api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace api.Tests;

public class PredictionEndpointValidationTests
{
    [Fact]
    public async Task Predict_rejects_blank_asset_symbol()
    {
        var request = new PredictBatchRequest([new PredictAssetRequest("   ", null, "Stock", "Technology", 100m, 90)]);
        var result = await InvokePredict(request);
        var response = await Execute(result);
        Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
        Assert.Contains("symbol", response.Body, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<IResult> InvokePredict(PredictBatchRequest request)
    {
        var method = typeof(PredictionEndpoints).GetMethod("Predict", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new MissingMethodException(nameof(PredictionEndpoints), "Predict");
        using var loggers = LoggerFactory.Create(builder => builder.AddDebug());
        var task = (Task<IResult>?)method.Invoke(null, [request, new DefaultHttpContext(), Client(), Store(), Enricher(), loggers])
            ?? throw new InvalidOperationException("Predict did not return a result task.");
        return await task;
    }

    // Validation fails before enrichment runs, so a failing factory proves that.
    private static PredictionRequestEnricher Enricher()
    {
        return new PredictionRequestEnricher(new PriceHistoryStore(new FailingConnectionFactory()));
    }

    private static RegulasAiClient Client()
    {
        var http = new HttpClient(StubHttpMessageHandler.Json(TestData.OverviewJson)) { BaseAddress = new Uri("http://localhost:8301/") };
        return new RegulasAiClient(http);
    }

    private static PredictionStore Store()
    {
        return new PredictionStore(new FailingConnectionFactory());
    }

    private static async Task<ResponseSnapshot> Execute(IResult result)
    {
        var context = new DefaultHttpContext { Response = { Body = new MemoryStream() } };
        context.RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider();
        await result.ExecuteAsync(context);
        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        return new ResponseSnapshot(context.Response.StatusCode, body);
    }

    private sealed class FailingConnectionFactory : IDatabaseConnectionFactory
    {
        public Task<DbConnection> OpenDatabaseConnectionAsync(CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("storage should not be needed for validation failures");
        }
    }

    private sealed record ResponseSnapshot(int StatusCode, string Body);
}
