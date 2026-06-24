using api.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace api.Tests;

public class TradingAgentsConfigurationTests
{
    [Fact]
    public void Defaults_to_localhost_when_unset()
    {
        var url = TradingAgentsConfiguration.StockUrl(Config(new()));
        Assert.Equal("http://localhost:8261/", url.ToString());
    }

    [Fact]
    public void Prefers_TradingAgents_StockUrl_key()
    {
        var url = TradingAgentsConfiguration.StockUrl(Config(new() { ["TradingAgents:StockUrl"] = "http://ta:9000" }));
        Assert.Equal("http://ta:9000/", url.ToString());
    }

    [Fact]
    public void Falls_back_to_env_style_key()
    {
        var url = TradingAgentsConfiguration.StockUrl(Config(new() { ["TRADINGAGENTS_STOCK_AI_URL"] = "http://ta:9100/" }));
        Assert.Equal("http://ta:9100/", url.ToString());
    }

    private static IConfiguration Config(Dictionary<string, string?> values)
    {
        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }
}
