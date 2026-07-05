using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using api.Contracts;

namespace api.Services;

// Calls the Pokemon TCG provider from the backend so frontends stay on Regulas.Api.
public sealed class PokemonTcgClient
{
    public const string SourceName = "PokemonTCG";
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public PokemonTcgClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<PokemonCardSearchResponse?> SearchAsync(string query, int pageSize, CancellationToken token)
    {
        using var response = await SendAsync(SearchPath(query, pageSize), token);
        response.EnsureSuccessStatusCode();
        var envelope = await response.Content.ReadFromJsonAsync<CardSearchEnvelope>(token);
        return envelope is null ? null : ToSearchResponse(envelope);
    }

    public async Task<PokemonCardDetail?> GetCardAsync(string id, CancellationToken token)
    {
        using var response = await SendAsync($"cards/{WebUtility.UrlEncode(id)}", token);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        response.EnsureSuccessStatusCode();
        var envelope = await response.Content.ReadFromJsonAsync<CardEnvelope>(token);
        return envelope?.Data is null ? null : ToDetail(envelope.Data);
    }

    private async Task<HttpResponseMessage> SendAsync(string path, CancellationToken token)
    {
        using var request = Request(path);
        return await _httpClient.SendAsync(request, token);
    }

    private HttpRequestMessage Request(string path)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        AddApiKey(request);
        return request;
    }

    private void AddApiKey(HttpRequestMessage request)
    {
        var apiKey = PokemonTcgConfiguration.GetApiKey(_configuration);
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            request.Headers.Add("X-Api-Key", apiKey);
        }
    }

    private static string SearchPath(string query, int pageSize)
    {
        var pairs = new Dictionary<string, string>
        {
            ["q"] = NameQuery(query),
            ["pageSize"] = pageSize.ToString(),
            ["orderBy"] = "-set.releaseDate",
        };
        return $"cards?{string.Join("&", pairs.Select(FormatPair))}";
    }

    private static string NameQuery(string query)
    {
        var clean = query.Trim().Replace("\"", string.Empty);
        return clean.Contains(' ') ? $"name:\"{clean}\"" : $"name:{clean}*";
    }

    private static string FormatPair(KeyValuePair<string, string> pair)
    {
        return $"{WebUtility.UrlEncode(pair.Key)}={WebUtility.UrlEncode(pair.Value)}";
    }

    private static PokemonCardSearchResponse ToSearchResponse(CardSearchEnvelope envelope)
    {
        return new PokemonCardSearchResponse(envelope.Data.Select(ToSummary).ToList(), envelope.Page, envelope.PageSize, envelope.Count, envelope.TotalCount);
    }

    private static PokemonCardSummary ToSummary(PokemonCard card)
    {
        return new PokemonCardSummary(
            card.Id, card.Name, card.Set?.Name, card.Set?.Series, card.Number, card.Rarity,
            card.Images?.Small, MarketPrice(card), SourceName, card.TcgPlayer?.UpdatedAt
        );
    }

    private static PokemonCardDetail ToDetail(PokemonCard card)
    {
        return new PokemonCardDetail(
            card.Id, card.Name, card.Supertype, card.Subtypes ?? [], card.Hp, card.Types ?? [],
            card.Set?.Name, card.Set?.Series, card.Number, card.Artist, card.Rarity,
            card.Images?.Small, card.Images?.Large, card.TcgPlayer?.Url, SourceName,
            card.TcgPlayer?.UpdatedAt, Prices(card)
        );
    }

    private static decimal? MarketPrice(PokemonCard card)
    {
        return card.TcgPlayer?.Prices?.Values.Select(price => price.Market).FirstOrDefault(price => price is not null);
    }

    private static List<PokemonCardPrice> Prices(PokemonCard card)
    {
        return card.TcgPlayer?.Prices?.Select(ToPrice).ToList() ?? [];
    }

    private static PokemonCardPrice ToPrice(KeyValuePair<string, ProviderPrice> pair)
    {
        var price = pair.Value;
        return new PokemonCardPrice(pair.Key, price.Low, price.Mid, price.High, price.Market, price.DirectLow);
    }

    private sealed record CardSearchEnvelope(List<PokemonCard> Data, int Page, int PageSize, int Count, int TotalCount);
    private sealed record CardEnvelope(PokemonCard Data);
    private sealed record PokemonCard(
        string Id,
        string Name,
        string? Supertype,
        List<string>? Subtypes,
        string? Hp,
        List<string>? Types,
        PokemonSet? Set,
        string? Number,
        string? Artist,
        string? Rarity,
        PokemonImages? Images,
        [property: JsonPropertyName("tcgplayer")] TcgPlayer? TcgPlayer
    );
    private sealed record PokemonSet(string? Name, string? Series);
    private sealed record PokemonImages(string? Small, string? Large);
    private sealed record TcgPlayer(string? Url, string? UpdatedAt, Dictionary<string, ProviderPrice>? Prices);
    private sealed record ProviderPrice(decimal? Low, decimal? Mid, decimal? High, decimal? Market, decimal? DirectLow);
}
