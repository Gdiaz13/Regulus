using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using api.Contracts;

namespace api.Services;

// Calls APITCG from the backend so its API key and One Piece data stay behind Regulas.Api.
public sealed class OnePieceTcgClient
{
    public const string SourceName = "APITCG";
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public OnePieceTcgClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<OnePieceCardSearchResponse?> SearchAsync(string query, int pageSize, CancellationToken token)
    {
        using var response = await SendAsync(SearchPath(query, pageSize), token);
        response.EnsureSuccessStatusCode();
        var envelope = await response.Content.ReadFromJsonAsync<ProductSearchEnvelope>(token);
        return envelope is null ? null : ToSearchResponse(envelope, pageSize);
    }

    public async Task<OnePieceCardDetail?> GetCardAsync(string id, CancellationToken token)
    {
        using var response = await SendAsync($"api/products/{WebUtility.UrlEncode(id)}?populate=set", token);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        response.EnsureSuccessStatusCode();
        var envelope = await response.Content.ReadFromJsonAsync<ProductEnvelope>(token);
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
        var apiKey = ApiTcgConfiguration.GetApiKey(_configuration);
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            request.Headers.Add("x-api-key", apiKey);
        }
        return request;
    }

    private static string SearchPath(string query, int pageSize)
    {
        var pairs = new Dictionary<string, string>
        {
            ["tcg"] = "one-piece", ["type"] = "card", ["name"] = query.Trim(),
            ["limit"] = pageSize.ToString(), ["page"] = "1", ["populate"] = "set",
        };
        return $"api/products?{string.Join("&", pairs.Select(FormatPair))}";
    }

    private static string FormatPair(KeyValuePair<string, string> pair)
    {
        return $"{WebUtility.UrlEncode(pair.Key)}={WebUtility.UrlEncode(pair.Value)}";
    }

    private static OnePieceCardSearchResponse ToSearchResponse(ProductSearchEnvelope envelope, int pageSize)
    {
        var cards = envelope.Data.Select(ToSummary).ToList();
        return new OnePieceCardSearchResponse(cards, 1, pageSize, cards.Count, envelope.Total);
    }

    private static OnePieceCardSummary ToSummary(Product product)
    {
        var image = product.Images?.FirstOrDefault();
        return new OnePieceCardSummary(
            product.Id.ToString(), product.Name, SetName(product.Set), product.Code,
            Attribute(product, "Rarity"), Attribute(product, "Color"), image?.Small,
            MarketPrice(product), SourceName, product.UpdatedAt
        );
    }

    private static OnePieceCardDetail ToDetail(Product product)
    {
        var image = product.Images?.FirstOrDefault();
        return new OnePieceCardDetail(
            product.Id.ToString(), product.Name, product.Description, SetName(product.Set),
            product.Code, product.CardNumber, Attribute(product, "Rarity"), Attribute(product, "Color"),
            Attribute(product, "Power"), image?.Small, image?.Large, product.Markets?.TcgPlayer?.Url,
            SourceName, product.UpdatedAt, Prices(product)
        );
    }

    private static decimal? MarketPrice(Product product)
    {
        return product.Markets?.TcgPlayer?.Prices?.Market ?? product.Markets?.TcgMatch?.Prices?.Market;
    }

    private static List<OnePieceCardPrice> Prices(Product product)
    {
        var prices = new List<OnePieceCardPrice>();
        AddPrice(prices, "tcgplayer", product.Markets?.TcgPlayer?.Prices);
        AddPrice(prices, "tcgmatch", product.Markets?.TcgMatch?.Prices);
        return prices;
    }

    private static void AddPrice(List<OnePieceCardPrice> values, string market, ProductPrices? price)
    {
        if (price is not null)
        {
            values.Add(new OnePieceCardPrice(market, "USD", price.Low, price.Mid, price.High, price.Market));
        }
    }

    private static string? SetName(JsonElement? set)
    {
        if (set is null || set.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined) return null;
        if (set.Value.ValueKind == JsonValueKind.String) return set.Value.GetString();
        return set.Value.TryGetProperty("name", out var name) ? name.GetString() : null;
    }

    private static string? Attribute(Product product, string name)
    {
        return product.Attributes?.GetValueOrDefault(name);
    }

    private sealed record ProductSearchEnvelope(bool Success, List<Product> Data, int Total);
    private sealed record ProductEnvelope(bool Success, Product? Data);
    private sealed record Product(
        [property: JsonPropertyName("_id")] long Id,
        string Name,
        string? Description,
        JsonElement? Set,
        string? Code,
        string? CardNumber,
        Dictionary<string, string>? Attributes,
        List<ProductImage>? Images,
        ProductMarkets? Markets,
        string? UpdatedAt
    );
    private sealed record ProductImage(string? Small, string? Medium, string? Large);
    private sealed record ProductMarkets(ProductMarket? TcgPlayer, ProductMarket? TcgMatch);
    private sealed record ProductMarket(string? Id, string? Url, ProductPrices? Prices);
    private sealed record ProductPrices(decimal? Low, decimal? Mid, decimal? High, decimal? Market);
}