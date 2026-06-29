using api.Services;
using Xunit;

namespace api.Tests;

public class PriceHistoryQueryOptionsTests
{
    [Fact]
    public void NormalizeTake_uses_default_when_missing()
    {
        Assert.Equal(365, PriceHistoryQueryOptions.NormalizeTake(null));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-4, 1)]
    [InlineData(50, 50)]
    [InlineData(5000, 1000)]
    public void NormalizeTake_clamps_to_safe_chart_bounds(int requested, int expected)
    {
        Assert.Equal(expected, PriceHistoryQueryOptions.NormalizeTake(requested));
    }
}
