using System.Net;
using api.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace api.Tests;

public class PokemonTcgClientTests
{
    [Fact]
    public async Task Search_maps_provider_cards_and_adds_server_side_key()
    {
        var handler = new CapturingHandler(JsonResponse(SearchJson));
        var result = await Client(handler, Config("pokemon-key")).SearchAsync("charizard", 12, CancellationToken.None);
        Assert.Equal("pokemon-key", handler.ApiKey);
        Assert.Contains("q=name%3Acharizard*", handler.Query);
        Assert.Equal("base1-4", result!.Cards.Single().Id);
        Assert.Equal(399.99m, result.Cards.Single().MarketPrice);
    }

    [Fact]
    public async Task Detail_maps_card_metadata_and_prices()
    {
        var handler = new CapturingHandler(JsonResponse(DetailJson));
        var result = await Client(handler, Config(null)).GetCardAsync("base1-4", CancellationToken.None);
        Assert.Null(handler.ApiKey);
        Assert.Equal("Charizard", result!.Name);
        Assert.Equal(["Stage 2"], result.Subtypes);
        Assert.Equal("holofoil", result.Prices.Single().Variant);
    }

    [Fact]
    public async Task Detail_returns_null_when_provider_card_is_missing()
    {
        var handler = new CapturingHandler(new HttpResponseMessage(HttpStatusCode.NotFound));
        var result = await Client(handler, Config(null)).GetCardAsync("missing", CancellationToken.None);
        Assert.Null(result);
    }

    private static PokemonTcgClient Client(HttpMessageHandler handler, IConfiguration config)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        return new PokemonTcgClient(httpClient, config);
    }

    private static IConfiguration Config(string? apiKey)
    {
        return new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["PokemonTcg:ApiKey"] = apiKey }).Build();
    }

    private static HttpResponseMessage JsonResponse(string content)
    {
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(content) };
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;
        public string Query { get; private set; } = string.Empty;
        public string? ApiKey { get; private set; }

        public CapturingHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Query = request.RequestUri?.Query ?? string.Empty;
            ApiKey = request.Headers.TryGetValues("X-Api-Key", out var values) ? values.Single() : null;
            return Task.FromResult(_response);
        }
    }

    private const string SearchJson = """
        {"data":[{"id":"base1-4","name":"Charizard","set":{"name":"Base","series":"Base"},"number":"4","rarity":"Rare Holo","images":{"small":"small.png"},"tcgplayer":{"updatedAt":"2026/01/01","prices":{"holofoil":{"market":399.99}}}}],"page":1,"pageSize":12,"count":1,"totalCount":1}
        """;

    private const string DetailJson = """
        {"data":{"id":"base1-4","name":"Charizard","supertype":"Pokemon","subtypes":["Stage 2"],"hp":"120","types":["Fire"],"set":{"name":"Base","series":"Base"},"number":"4","artist":"Mitsuhiro Arita","rarity":"Rare Holo","images":{"small":"small.png","large":"large.png"},"tcgplayer":{"url":"https://prices.pokemontcg.io/tcgplayer/base1-4","updatedAt":"2026/01/01","prices":{"holofoil":{"low":250,"mid":375,"high":600,"market":399.99,"directLow":350}}}}}
        """;
}
