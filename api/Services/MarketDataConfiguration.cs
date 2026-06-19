namespace api.Services;

public static class MarketDataConfiguration
{
    public static bool HasApiKey(IConfiguration configuration)
    {
        return !string.IsNullOrWhiteSpace(GetApiKey(configuration));
    }

    public static string? GetApiKey(IConfiguration configuration)
    {
        return ConfiguredKey(configuration) ?? EnvironmentKey(configuration);
    }

    private static string? ConfiguredKey(IConfiguration configuration)
    {
        return BlankToNull(configuration["FinancialModelingPrep:ApiKey"]);
    }

    private static string? EnvironmentKey(IConfiguration configuration)
    {
        return BlankToNull(configuration["FMP_API_KEY"]);
    }

    private static string? BlankToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
