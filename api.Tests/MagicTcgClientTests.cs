using System.Net;
using api.Services;
using Xunit;

namespace api.Tests;

public class MagicTcgClientTests
{
    [Fact]
    public async Task Search_maps_scryfall_cards_without_client_side_keys()
    {
        var handler = new CapturingHandler(JsonResponse(SearchJson));
        var result = await Client(handler).SearchAsync("lightning bolt", 12, CancellationToken.None);
        Assert.DoesNotContain("api_key", handler.Query, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("q=lightning+bolt", handler.Query);
        Assert.Equal("Regulas", handler.UserAgentProduct);
        Assert.Equal("00000000-0000-0000-0000-000000000001", result!.Cards.Single().Id);
        Assert.Equal("Limited Edition Alpha", result.Cards.Single().SetName);
        Assert.Equal(399.99m, result.Cards.Single().MarketPrice);
        Assert.Equal("usd", result.Cards.Single().MarketCurrency);
    }

    [Fact]
    public async Task Search_tracks_summary_currency_when_usd_is_missing()
    {
        var handler = new CapturingHandler(JsonResponse(EuroSearchJson));
        var result = await Client(handler).SearchAsync("euro card", 12, CancellationToken.None);
        Assert.Equal(1.23m, result!.Cards.Single().MarketPrice);
        Assert.Equal("eur", result.Cards.Single().MarketCurrency);
    }

    [Fact]
    public async Task Search_uses_first_card_face_image_when_top_level_image_is_missing()
    {
        var handler = new CapturingHandler(JsonResponse(DoubleFacedSearchJson));
        var result = await Client(handler).SearchAsync("invasion", 12, CancellationToken.None);
        Assert.Equal("front-small.jpg", result!.Cards.Single().SmallImageUrl);
    }

    [Fact]
    public async Task Detail_maps_card_metadata_and_prices()
    {
        var handler = new CapturingHandler(JsonResponse(DetailJson));
        var result = await Client(handler).GetCardAsync("00000000-0000-0000-0000-000000000001", CancellationToken.None);
        Assert.Equal("Lightning Bolt", result!.Name);
        Assert.Equal("{R}", result.ManaCost);
        Assert.Equal(["R"], result.Colors);
        Assert.Equal("usd", result.Prices.Single().Currency);
        Assert.Equal("normal", result.Prices.Single().Finish);
        Assert.Equal(399.99m, result.Prices.Single().MarketPrice);
    }

    [Fact]
    public async Task Detail_uses_first_card_face_metadata_when_top_level_fields_are_missing()
    {
        var handler = new CapturingHandler(JsonResponse(DoubleFacedDetailJson));
        var result = await Client(handler).GetCardAsync("dfc", CancellationToken.None);
        Assert.Equal("Invasion of Ravnica", result!.Name);
        Assert.Equal("Battle - Siege", result.TypeLine);
        Assert.Equal("{5}{W}{U}", result.ManaCost);
        Assert.Equal("When Invasion of Ravnica enters, exile target nonland permanent.", result.OracleText);
        Assert.Equal(["W", "U"], result.Colors);
        Assert.Equal("front-small.jpg", result.SmallImageUrl);
        Assert.Equal("front-large.jpg", result.LargeImageUrl);
    }

    [Fact]
    public async Task Detail_returns_null_when_provider_card_is_missing()
    {
        var handler = new CapturingHandler(new HttpResponseMessage(HttpStatusCode.NotFound));
        var result = await Client(handler).GetCardAsync("missing", CancellationToken.None);
        Assert.Null(result);
    }

    [Fact]
    public async Task Search_returns_empty_response_when_provider_reports_no_matches()
    {
        var handler = new CapturingHandler(new HttpResponseMessage(HttpStatusCode.NotFound));
        var result = await Client(handler).SearchAsync("not a real card", 12, CancellationToken.None);
        Assert.NotNull(result);
        Assert.Empty(result.Cards);
        Assert.Equal(0, result.Count);
        Assert.Equal(0, result.TotalCount);
    }

    private static MagicTcgClient Client(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        return new MagicTcgClient(httpClient);
    }

    private static HttpResponseMessage JsonResponse(string content)
    {
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(content) };
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;
        public string Query { get; private set; } = string.Empty;
        public string? UserAgentProduct { get; private set; }

        public CapturingHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Query = request.RequestUri?.Query ?? string.Empty;
            UserAgentProduct = request.Headers.UserAgent.SingleOrDefault()?.Product?.Name;
            return Task.FromResult(_response);
        }
    }

    private const string SearchJson = """
        {"data":[{"id":"00000000-0000-0000-0000-000000000001","name":"Lightning Bolt","set_name":"Limited Edition Alpha","set":"lea","collector_number":"161","rarity":"common","released_at":"1993-08-05","image_uris":{"small":"small.jpg","normal":"normal.jpg","large":"large.jpg"},"prices":{"usd":"399.99","usd_foil":null,"usd_etched":null,"eur":null,"tix":null}}],"total_cards":1,"has_more":false}
        """;

    private const string EuroSearchJson = """
        {"data":[{"id":"00000000-0000-0000-0000-000000000002","name":"Euro Card","set_name":"Foreign Set","set":"for","collector_number":"2","rarity":"rare","released_at":"2024-01-01","image_uris":{"small":"small.jpg","normal":"normal.jpg","large":"large.jpg"},"prices":{"usd":null,"usd_foil":null,"usd_etched":null,"eur":"1.23","tix":null}}],"total_cards":1,"has_more":false}
        """;

    private const string DoubleFacedSearchJson = """
        {"data":[{"id":"dfc","name":"Invasion of Ravnica","set_name":"March of the Machine","set":"mom","collector_number":"1","rarity":"mythic","released_at":"2023-04-21","prices":{"usd":"3.25","usd_foil":null,"usd_etched":null,"eur":null,"tix":null},"card_faces":[{"image_uris":{"small":"front-small.jpg","normal":"front-normal.jpg","large":"front-large.jpg"}}]}],"total_cards":1,"has_more":false}
        """;

    private const string DetailJson = """
        {"id":"00000000-0000-0000-0000-000000000001","name":"Lightning Bolt","set_name":"Limited Edition Alpha","set":"lea","collector_number":"161","rarity":"common","released_at":"1993-08-05","artist":"Christopher Rush","type_line":"Instant","mana_cost":"{R}","oracle_text":"Lightning Bolt deals 3 damage to any target.","colors":["R"],"image_uris":{"small":"small.jpg","normal":"normal.jpg","large":"large.jpg"},"scryfall_uri":"https://scryfall.com/card/lea/161/lightning-bolt","prices":{"usd":"399.99","usd_foil":null,"usd_etched":null,"eur":null,"tix":null}}
        """;

    private const string DoubleFacedDetailJson = """
        {"id":"dfc","name":"Invasion of Ravnica","set_name":"March of the Machine","set":"mom","collector_number":"1","rarity":"mythic","released_at":"2023-04-21","artist":"Leon Tukker","scryfall_uri":"https://scryfall.com/card/mom/1/invasion-of-ravnica","prices":{"usd":"3.25","usd_foil":null,"usd_etched":null,"eur":null,"tix":null},"card_faces":[{"name":"Invasion of Ravnica","type_line":"Battle - Siege","mana_cost":"{5}{W}{U}","oracle_text":"When Invasion of Ravnica enters, exile target nonland permanent.","colors":["W","U"],"image_uris":{"small":"front-small.jpg","normal":"front-normal.jpg","large":"front-large.jpg"}},{"name":"Guildpact Paragon","type_line":"Creature - Angel","mana_cost":"","oracle_text":"Flying.","colors":["W","U"],"image_uris":{"small":"back-small.jpg","normal":"back-normal.jpg","large":"back-large.jpg"}}]}
        """;
}
