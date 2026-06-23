using api.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace api.Tests;

// Covers RegulasAiConfiguration.CoreUrl: where the gateway looks for the
// commander AI's address, and the precedence between the config keys.
public class RegulasAiConfigurationTests
{
    private static IConfiguration Config(Dictionary<string, string?> values)
    {
        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }

    [Fact]
    public void Defaults_to_localhost_when_unset()
    {
        var url = RegulasAiConfiguration.CoreUrl(Config(new()));
        Assert.Equal("http://localhost:8301/", url.ToString());
    }

    [Fact]
    public void Prefers_RegulasAi_CoreUrl_key()
    {
        var url = RegulasAiConfiguration.CoreUrl(Config(new() { ["RegulasAi:CoreUrl"] = "http://ai:9000" }));
        Assert.Equal("http://ai:9000/", url.ToString());
    }

    [Fact]
    public void Falls_back_to_env_style_key()
    {
        var url = RegulasAiConfiguration.CoreUrl(Config(new() { ["REGULAS_CORE_AI_URL"] = "http://ai:9100/" }));
        Assert.Equal("http://ai:9100/", url.ToString());
    }

    [Fact]
    public void Always_ends_with_a_trailing_slash()
    {
        var url = RegulasAiConfiguration.CoreUrl(Config(new() { ["RegulasAi:CoreUrl"] = "http://ai:9000" }));
        Assert.EndsWith("/", url.ToString());
    }
}
