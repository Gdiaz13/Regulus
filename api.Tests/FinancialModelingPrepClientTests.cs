using System.Net;
using api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace api.Tests;

public class FinancialModelingPrepClientTests
{
    [Fact]
    public async Task Missing_api_key_returns_problem_without_calling_provider()
    {
        await WithoutApiKey(async () =>
        {
            var handler = new CapturingHandler(JsonResponse("{}"));
            var response = await Execute(await Client(handler, Config(null)).GetAsync("profile", Query()));
            Assert.Equal(500, response.StatusCode);
            Assert.Equal(0, handler.Calls);
        });
    }

    [Fact]
    public async Task Configured_api_key_is_added_server_side()
    {
        var handler = new CapturingHandler(JsonResponse("""[{"symbol":"AMD"}]"""));
        var response = await Execute(await Client(handler, Config("server-key")).GetAsync("profile", Query()));
        Assert.Equal(200, response.StatusCode);
        Assert.Contains("symbol=AMD", handler.Query);
        Assert.Contains("apikey=server-key", handler.Query);
    }

    [Fact]
    public async Task Provider_connection_failure_returns_unavailable()
    {
        var handler = new FailingHandler();
        var result = await Client(handler, Config("server-key")).GetAsync("profile", Query());
        var response = await Execute(result);
        Assert.Equal(503, response.StatusCode);
    }

    private static FinancialModelingPrepClient Client(HttpMessageHandler handler, IConfiguration config)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        return new FinancialModelingPrepClient(httpClient, config);
    }

    private static IConfiguration Config(string? apiKey)
    {
        var values = new Dictionary<string, string?> { ["FinancialModelingPrep:ApiKey"] = apiKey };
        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }

    private static Dictionary<string, string?> Query()
    {
        return new Dictionary<string, string?> { ["symbol"] = "AMD" };
    }

    private static HttpResponseMessage JsonResponse(string content)
    {
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(content) };
    }

    private static async Task<ResponseSnapshot> Execute(IResult result)
    {
        var context = new DefaultHttpContext { Response = { Body = new MemoryStream() } };
        context.RequestServices = Services();
        await result.ExecuteAsync(context);
        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        return new ResponseSnapshot(context.Response.StatusCode, body);
    }

    private static ServiceProvider Services()
    {
        return new ServiceCollection().AddLogging().AddProblemDetails().BuildServiceProvider();
    }

    private static async Task WithoutApiKey(Func<Task> assert)
    {
        var previous = Environment.GetEnvironmentVariable("FMP_API_KEY");
        try
        {
            Environment.SetEnvironmentVariable("FMP_API_KEY", null);
            await assert();
        }
        finally
        {
            Environment.SetEnvironmentVariable("FMP_API_KEY", previous);
        }
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;
        public int Calls { get; private set; }
        public string Query { get; private set; } = string.Empty;

        public CapturingHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Calls++;
            Query = request.RequestUri?.Query ?? string.Empty;
            return Task.FromResult(_response);
        }
    }

    private sealed class FailingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            return Task.FromException<HttpResponseMessage>(new HttpRequestException("provider offline"));
        }
    }

    private sealed record ResponseSnapshot(int StatusCode, string Body);
}
