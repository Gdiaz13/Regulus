namespace api.Services;

public static class PokemonTcgConfiguration
{
    public static string? GetApiKey(IConfiguration configuration)
    {
        return configuration["PokemonTcg:ApiKey"] ?? Environment.GetEnvironmentVariable("POKEMON_TCG_API_KEY");
    }
}
