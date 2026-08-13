namespace api.Services;

// Prediction traffic uses RegulasCoreAI; the opt-in training job targets the
// one specialist that currently has a real trainer.
public static class RegulasAiConfiguration
{
    private const string DefaultCoreUrl = "http://localhost:8301/";
    private const string DefaultStockTechUrl = "http://localhost:8101/";

    public static Uri CoreUrl(IConfiguration configuration)
    {
        return new Uri(EnsureTrailingSlash(ConfiguredCoreUrl(configuration)));
    }

    public static Uri StockTechUrl(IConfiguration configuration)
    {
        return new Uri(EnsureTrailingSlash(ConfiguredStockTechUrl(configuration)));
    }

    private static string ConfiguredCoreUrl(IConfiguration configuration)
    {
        return BlankToNull(configuration["RegulasAi:CoreUrl"])
            ?? BlankToNull(configuration["REGULAS_CORE_AI_URL"])
            ?? DefaultCoreUrl;
    }

    private static string ConfiguredStockTechUrl(IConfiguration configuration)
    {
        return BlankToNull(configuration["STOCK_TECH_AI_URL"])
            ?? BlankToNull(configuration["RegulasAi:StockTechUrl"])
            ?? DefaultStockTechUrl;
    }

    private static string EnsureTrailingSlash(string url)
    {
        return url.EndsWith('/') ? url : url + "/";
    }

    private static string? BlankToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
