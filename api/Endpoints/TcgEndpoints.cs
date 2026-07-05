using System.Text.Json;
using api.Services;

namespace api.Endpoints;

public static class TcgEndpoints
{
    public static void MapTcgEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/tcg");
        group.MapGet("pokemon/cards", SearchPokemonCards);
        group.MapGet("pokemon/cards/{id}", GetPokemonCard);
    }

    private static async Task<IResult> SearchPokemonCards(
        string? query,
        int? pageSize,
        PokemonTcgClient client,
        CancellationToken token
    )
    {
        var clean = Clean(query);
        if (string.IsNullOrWhiteSpace(clean))
        {
            return Results.BadRequest("A card name is required.");
        }
        return await ProviderRequest(() => client.SearchAsync(clean, ClampPageSize(pageSize), token));
    }

    private static async Task<IResult> GetPokemonCard(string id, PokemonTcgClient client, CancellationToken token)
    {
        var clean = Clean(id);
        if (string.IsNullOrWhiteSpace(clean))
        {
            return Results.BadRequest("A card id is required.");
        }
        return await ProviderRequest(() => client.GetCardAsync(clean, token));
    }

    private static async Task<IResult> ProviderRequest<T>(Func<Task<T?>> request)
    {
        try
        {
            var result = await request();
            return result is null ? Results.NotFound("Pokemon card data was not found.") : Results.Ok(result);
        }
        catch (Exception exception) when (IsProviderException(exception))
        {
            return Results.Problem("Pokemon card data is unavailable.", statusCode: 503);
        }
    }

    private static int ClampPageSize(int? pageSize)
    {
        return Math.Clamp(pageSize ?? 12, 1, 24);
    }

    private static string Clean(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }

    private static bool IsProviderException(Exception exception)
    {
        return exception is HttpRequestException or TaskCanceledException or JsonException;
    }
}
