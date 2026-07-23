using System.Data.Common;
using System.Net;
using System.Reflection;
using api.Endpoints;
using api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace api.Tests;

public class OnePieceTcgClientTests
{
    [Fact]
    public async Task Search_maps_cards_and_keeps_provider_key_server_side()
    {
        var handler = new CapturingHandler(JsonResponse(SearchJson));
        var result = await Client(handler, Config("server-key")).SearchAsync("Monkey D. Luffy", 12, CancellationToken.None);

        Assert.Equal("server-key", handler.ApiKey);
        Assert.Contains("tcg=one-piece", handler.Query);
        Assert.Contains("type=card", handler.Query);
        Assert.Contains("name=Monkey+D.+Luffy", handler.Query);
        var card = Assert.Single(result!.Cards);
        Assert.Equal("1024", card.Id);
        Assert.Equal("Pillars of Strength", card.SetName);
        Assert.Equal(0.31m, card.MarketPrice);
    }

    [Fact]
    public async Task Search_uses_apitcg_environment_key_when_config_is_empty()
    {
        var previous = Environment.GetEnvironmentVariable("APITCG_API_KEY");
        Environment.SetEnvironmentVariable("APITCG_API_KEY", "environment-key");
        try
        {
            var handler = new CapturingHandler(JsonResponse(SearchJson));
            await Client(handler, Config(string.Empty)).SearchAsync("luffy", 12, CancellationToken.None);
            Assert.Equal("environment-key", handler.ApiKey);
        }
        finally
        {
            Environment.SetEnvironmentVariable("APITCG_API_KEY", previous);
        }
    }

    [Fact]
    public async Task Search_endpoint_returns_mapped_provider_cards()
    {
        var method = typeof(TcgEndpoints).GetMethod("SearchOnePieceCards", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new MissingMethodException(nameof(TcgEndpoints), "SearchOnePieceCards");
        var client = Client(new CapturingHandler(JsonResponse(SearchJson)), Config("server-key"));
        var task = (Task<IResult>?)method.Invoke(null, ["luffy", 12, client, CancellationToken.None])
            ?? throw new InvalidOperationException("One Piece search endpoint did not return a result task.");

        var response = await Execute(await task);

        Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        Assert.Contains("Monkey.D.Luffy", response.Body);
    }

    [Fact]
    public async Task Detail_maps_card_metadata_and_market_prices()
    {
        var handler = new CapturingHandler(JsonResponse(DetailJson));

        var result = await Client(handler, Config("server-key")).GetCardAsync("1024", CancellationToken.None);

        Assert.Equal("/api/products/1024?populate=set", handler.PathAndQuery);
        Assert.Equal("OP03-070", result!.Code);
        Assert.Equal("7000", result.Power);
        Assert.Equal("large.png", result.LargeImageUrl);
        Assert.Equal(2, result.Prices.Count);
        Assert.Equal(0.31m, result.Prices.Single(price => price.Market == "tcgplayer").MarketPrice);
    }

    [Fact]
    public async Task Detail_returns_null_when_provider_card_is_missing()
    {
        var handler = new CapturingHandler(new HttpResponseMessage(HttpStatusCode.NotFound));

        var result = await Client(handler, Config("server-key")).GetCardAsync("999999", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Detail_endpoint_returns_card_when_price_storage_fails()
    {
        var method = typeof(TcgEndpoints).GetMethod("GetOnePieceCard", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new MissingMethodException(nameof(TcgEndpoints), "GetOnePieceCard");
        using var loggers = LoggerFactory.Create(builder => builder.AddDebug());
        var client = Client(new CapturingHandler(JsonResponse(DetailJson)), Config("server-key"));
        var store = new PriceHistoryStore(new FailingConnectionFactory());
        var task = (Task<IResult>?)method.Invoke(null, ["1024", client, store, loggers, CancellationToken.None])
            ?? throw new InvalidOperationException("One Piece detail endpoint did not return a result task.");

        var response = await Execute(await task);

        Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        Assert.Contains("Monkey.D.Luffy", response.Body);
    }

    [Theory]
    [InlineData("OP03-070")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("+1")]
    [InlineData(" 1 ")]
    [InlineData("999999999999999999999999")]
    public async Task Detail_endpoint_rejects_invalid_provider_id(string id)
    {
        var method = typeof(TcgEndpoints).GetMethod("GetOnePieceCard", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new MissingMethodException(nameof(TcgEndpoints), "GetOnePieceCard");
        using var loggers = LoggerFactory.Create(builder => builder.AddDebug());
        var client = Client(new CapturingHandler(JsonResponse(DetailJson)), Config("server-key"));
        var store = new PriceHistoryStore(new FailingConnectionFactory());
        var task = (Task<IResult>?)method.Invoke(null, [id, client, store, loggers, CancellationToken.None])
            ?? throw new InvalidOperationException("One Piece detail endpoint did not return a result task.");

        var response = await Execute(await task);

        Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
    }

    private static OnePieceTcgClient Client(HttpMessageHandler handler, IConfiguration configuration)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        return new OnePieceTcgClient(httpClient, configuration);
    }

    private static IConfiguration Config(string? apiKey)
    {
        return new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["ApiTcg:ApiKey"] = apiKey }
        ).Build();
    }

    private static HttpResponseMessage JsonResponse(string content)
    {
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(content) };
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

    private sealed class CapturingHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;
        public string Query { get; private set; } = string.Empty;
        public string PathAndQuery { get; private set; } = string.Empty;
        public string? ApiKey { get; private set; }

        public CapturingHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Query = request.RequestUri?.Query ?? string.Empty;
            PathAndQuery = request.RequestUri?.PathAndQuery ?? string.Empty;
            ApiKey = request.Headers.TryGetValues("x-api-key", out var values) ? values.Single() : null;
            return Task.FromResult(_response);
        }
    }

    private sealed class FailingConnectionFactory : IDatabaseConnectionFactory
    {
        public Task<DbConnection> OpenDatabaseConnectionAsync(CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("storage offline");
        }
    }

    private const string SearchJson = """
        {"success":true,"data":[{"_id":1024,"type":"card","name":"Monkey.D.Luffy","description":"Leader card","tcg":"one-piece","set":{"_id":"one-piece-pillars-of-strength","name":"Pillars of Strength"},"code":"OP03-070","cardNumber":"070","attributes":{"Rarity":"R","Color":"Purple","Power":"7000"},"images":[{"small":"small.png","large":"large.png"}],"markets":{"tcgplayer":{"id":"453022","url":"https://example.test/product/453022","prices":{"low":0.15,"mid":0.35,"high":2.5,"market":0.31}}},"updatedAt":"2026-06-01T08:30:00.000Z"}],"total":1}
        """;

    private const string DetailJson = """
        {"success":true,"data":{"_id":1024,"type":"card","name":"Monkey.D.Luffy","description":"Leader card","tcg":"one-piece","set":{"_id":"one-piece-pillars-of-strength","name":"Pillars of Strength"},"code":"OP03-070","cardNumber":"070","attributes":{"Rarity":"R","Color":"Purple","Power":"7000"},"images":[{"small":"small.png","large":"large.png"}],"markets":{"tcgplayer":{"id":"453022","url":"https://example.test/product/453022","prices":{"low":0.15,"mid":0.35,"high":2.5,"market":0.31}},"tcgmatch":{"id":"match-1","url":"https://example.test/match/1","prices":{"low":0.14,"mid":0.34,"high":2.4,"market":0.30}}},"updatedAt":"2026-06-01T08:30:00.000Z"}}
        """;

    private sealed record ResponseSnapshot(int StatusCode, string Body);
}
