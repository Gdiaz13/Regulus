namespace api.Services;

public static class ApiTcgConfiguration
{
    public static string? GetApiKey(IConfiguration configuration)
    {
        var configured = configuration["ApiTcg:ApiKey"];
        return string.IsNullOrWhiteSpace(configured)
            ? Environment.GetEnvironmentVariable("APITCG_API_KEY")
            : configured;
    }
}
