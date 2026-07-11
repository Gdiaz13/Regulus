using System.Data.Common;
using System.Net;
using System.Reflection;
using api.Endpoints;
using api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace api.Tests;

public class TcgEndpointMagicTests
{
    [Fact]
    public async Task Magic_detail_returns_provider_card_when_price_storage_fails()
    {
        var result = await InvokeMagicDetail(new FailingConnectionFactory());
        var response = await Execute(result);
        Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        Assert.Contains("Lightning Bolt", response.Body);
    }

    private static async Task<IResult> InvokeMagicDetail(IDatabaseConnectionFactory factory)
    {
        var method = typeof(TcgEndpoints).GetMethod("GetMagicCard", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new MissingMethodException(nameof(TcgEndpoints), "GetMagicCard");
        using var loggers = LoggerFactory.Create(builder => builder.AddDebug());
        var task = (Task<IResult>?)method.Invoke(null, [CardId, Client(), new PriceHistoryStore(factory), loggers, CancellationToken.None])
            ?? throw new InvalidOperationException("Magic detail endpoint did not return a result task.");
        return await task;
    }

    private static MagicTcgClient Client()
    {
        var httpClient = new HttpClient(new CapturingHandler(JsonResponse(DetailJson))) { BaseAddress = new Uri("https://example.test/") };
        return new MagicTcgClient(httpClient);
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

        public CapturingHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
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

    private const string CardId = "00000000-0000-0000-0000-000000000001";

    private const string DetailJson = """
        {"id":"00000000-0000-0000-0000-000000000001","name":"Lightning Bolt","set_name":"Limited Edition Alpha","set":"lea","collector_number":"161","rarity":"common","released_at":"1993-08-05","artist":"Christopher Rush","type_line":"Instant","mana_cost":"{R}","oracle_text":"Lightning Bolt deals 3 damage to any target.","colors":["R"],"image_uris":{"small":"small.jpg","normal":"normal.jpg","large":"large.jpg"},"scryfall_uri":"https://scryfall.com/card/lea/161/lightning-bolt","prices":{"usd":"399.99","usd_foil":null,"usd_etched":null,"eur":null,"tix":null}}
        """;

    private sealed record ResponseSnapshot(int StatusCode, string Body);
}
