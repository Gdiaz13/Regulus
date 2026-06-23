namespace api.Services;

// Where RegulasCoreAI lives. The gateway only ever talks to the commander; the
// commander fans out to the category and specialist AIs on its own.
public static class RegulasAiConfiguration
{
    private const string DefaultCoreUrl = "http://localhost:8301/";

    public static Uri CoreUrl(IConfiguration configuration)
    {
        return new Uri(EnsureTrailingSlash(ConfiguredUrl(configuration)));
    }

    private static string ConfiguredUrl(IConfiguration configuration)
    {
        return BlankToNull(configuration["RegulasAi:CoreUrl"])
            ?? BlankToNull(configuration["REGULAS_CORE_AI_URL"])
            ?? DefaultCoreUrl;
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
