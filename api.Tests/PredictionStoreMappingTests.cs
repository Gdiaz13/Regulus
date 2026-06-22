using api.Contracts;
using api.Models;
using api.Services;
using Xunit;

namespace api.Tests;

// Covers PredictionStore.ToAiRequests: how the gateway normalises what the
// browser sends before handing it down to the AI services.
public class PredictionStoreMappingTests
{
    private static AiPredictRequest MapOne(PredictAssetRequest asset)
    {
        return PredictionStore.ToAiRequests(new[] { asset }).Single();
    }

    [Fact]
    public void Symbol_is_trimmed_and_uppercased()
    {
        var result = MapOne(new PredictAssetRequest("  amd ", null, null, null, 100m, null));
        Assert.Equal("AMD", result.AssetId);
    }

    [Fact]
    public void Name_defaults_to_symbol_when_null()
    {
        var result = MapOne(new PredictAssetRequest("amd", null, null, null, 100m, null));
        Assert.Equal("AMD", result.AssetName);
    }

    [Fact]
    public void AssetType_defaults_to_stock()
    {
        var result = MapOne(new PredictAssetRequest("amd", null, null, null, 100m, null));
        Assert.Equal(nameof(AssetType.Stock), result.AssetType);
    }

    [Fact]
    public void Category_defaults_to_empty()
    {
        var result = MapOne(new PredictAssetRequest("amd", null, null, null, 100m, null));
        Assert.Equal(string.Empty, result.Category);
    }

    [Fact]
    public void TimeHorizon_defaults_to_90()
    {
        var result = MapOne(new PredictAssetRequest("amd", null, null, null, 100m, null));
        Assert.Equal(90, result.TimeHorizonDays);
    }

    [Fact]
    public void Provided_fields_are_trimmed_and_passed_through()
    {
        var result = MapOne(new PredictAssetRequest("amd", " AMD Inc ", " Stock ", " Tech ", 100m, 30));
        Assert.Equal("AMD Inc", result.AssetName);
        Assert.Equal("Tech", result.Category);
        Assert.Equal(30, result.TimeHorizonDays);
    }
}
