using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using api.Contracts;

namespace api.Services;

// Calls Scryfall from the backend so Magic provider details stay behind Regulas.Api.
public sealed class MagicTcgClient
{
    public const string SourceName = "Scryfall";
    private readonly HttpClient _httpClient;

    public MagicTcgClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<MagicCardSearchResponse?> SearchAsync(string query, int pageSize, CancellationToken token)
    {
        using var response = await SendAsync(SearchPath(query), token);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return EmptySearchResponse(pageSize);
        }
        response.EnsureSuccessStatusCode();
        var envelope = await response.Content.ReadFromJsonAsync<SearchEnvelope>(token);
        return envelope is null ? null : ToSearchResponse(envelope, pageSize);
    }

    public async Task<MagicCardDetail?> GetCardAsync(string id, CancellationToken token)
    {
        using var response = await SendAsync($"cards/{WebUtility.UrlEncode(id)}", token);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        response.EnsureSuccessStatusCode();
        var card = await response.Content.ReadFromJsonAsync<ScryfallCard>(token);
        return card is null ? null : ToDetail(card);
    }

    private async Task<HttpResponseMessage> SendAsync(string path, CancellationToken token)
    {
        using var request = Request(path);
        return await _httpClient.SendAsync(request, token);
    }

    private static HttpRequestMessage Request(string path)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.UserAgent.ParseAdd("Regulas/1.0");
        request.Headers.Accept.ParseAdd("application/json");
        return request;
    }

    private static string SearchPath(string query)
    {
        var pairs = new Dictionary<string, string> { ["q"] = query.Trim(), ["unique"] = "prints", ["order"] = "released" };
        return $"cards/search?{string.Join("&", pairs.Select(FormatPair))}";
    }

    private static string FormatPair(KeyValuePair<string, string> pair)
    {
        return $"{WebUtility.UrlEncode(pair.Key)}={WebUtility.UrlEncode(pair.Value)}";
    }

    private static MagicCardSearchResponse ToSearchResponse(SearchEnvelope envelope, int pageSize)
    {
        var cards = envelope.Data.Select(ToSummary).Take(pageSize).ToList();
        return new MagicCardSearchResponse(cards, 1, pageSize, cards.Count, envelope.TotalCards);
    }

    private static MagicCardSearchResponse EmptySearchResponse(int pageSize)
    {
        return new MagicCardSearchResponse([], 1, pageSize, 0, 0);
    }

    private static MagicCardSummary ToSummary(ScryfallCard card)
    {
        var price = MarketPrice(card.Prices);
        return new MagicCardSummary(
            card.Id, card.Name, card.SetName, card.SetCode, card.CollectorNumber, card.Rarity,
            card.Images?.Small, price?.Price, price?.Currency, SourceName, card.ReleasedAt
        );
    }

    private static MagicCardDetail ToDetail(ScryfallCard card)
    {
        return new MagicCardDetail(
            card.Id, card.Name, card.TypeLine, card.ManaCost, card.OracleText, card.Colors ?? [],
            card.SetName, card.SetCode, card.CollectorNumber, card.Artist, card.Rarity,
            card.Images?.Small, card.Images?.Large, card.ScryfallUri, SourceName, card.ReleasedAt, Prices(card.Prices)
        );
    }

    private static PriceValue? MarketPrice(ScryfallPrices? prices)
    {
        return PriceValues(prices).FirstOrDefault();
    }

    private static List<MagicCardPrice> Prices(ScryfallPrices? prices)
    {
        return PriceValues(prices).Select(value => new MagicCardPrice(value.Currency, value.Finish, value.Price)).ToList();
    }

    private static IEnumerable<PriceValue> PriceValues(ScryfallPrices? prices)
    {
        if (prices is null)
        {
            yield break;
        }
        foreach (var value in RawPrices(prices))
        {
            if (TryPrice(value.Amount, out var price))
            {
                yield return new PriceValue(value.Currency, value.Finish, price);
            }
        }
    }

    private static IEnumerable<RawPrice> RawPrices(ScryfallPrices prices)
    {
        yield return new RawPrice("usd", "normal", prices.Usd);
        yield return new RawPrice("usd", "foil", prices.UsdFoil);
        yield return new RawPrice("usd", "etched", prices.UsdEtched);
        yield return new RawPrice("eur", "normal", prices.Eur);
        yield return new RawPrice("tix", "normal", prices.Tix);
    }

    private static bool TryPrice(string? value, out decimal price)
    {
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out price);
    }

    private sealed record SearchEnvelope(
        List<ScryfallCard> Data,
        [property: JsonPropertyName("total_cards")] int TotalCards
    );

    private sealed record ScryfallCard(
        string Id,
        string Name,
        [property: JsonPropertyName("set_name")] string? SetName,
        [property: JsonPropertyName("set")] string? SetCode,
        [property: JsonPropertyName("collector_number")] string? CollectorNumber,
        string? Rarity,
        [property: JsonPropertyName("released_at")] string? ReleasedAt,
        [property: JsonPropertyName("type_line")] string? TypeLine,
        [property: JsonPropertyName("mana_cost")] string? ManaCost,
        [property: JsonPropertyName("oracle_text")] string? OracleText,
        List<string>? Colors,
        string? Artist,
        [property: JsonPropertyName("image_uris")] ScryfallImages? Images,
        [property: JsonPropertyName("scryfall_uri")] string? ScryfallUri,
        ScryfallPrices? Prices
    );

    private sealed record ScryfallImages(string? Small, string? Normal, string? Large);
    private sealed record ScryfallPrices(
        string? Usd,
        [property: JsonPropertyName("usd_foil")] string? UsdFoil,
        [property: JsonPropertyName("usd_etched")] string? UsdEtched,
        string? Eur,
        string? Tix
    );
    private sealed record RawPrice(string Currency, string Finish, string? Amount);
    private sealed record PriceValue(string Currency, string Finish, decimal Price);
}
