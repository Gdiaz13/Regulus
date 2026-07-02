using System.Globalization;
using System.Reflection;
using System.Text.Json;
using api.Contracts;
using api.Endpoints;
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace api.Tests;

// Covers price-history endpoint response metadata that frontend capture clients consume.
public class PriceHistoryEndpointTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Store_capture_result_includes_provider_source()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new PriceHistoryStore(factory);
        var result = await StoreCapture(store, " amd ", AssetType.Stock, [Price("2026-01-02")]);
        Assert.Equal("AMD", result.Symbol);
        Assert.Equal("Stock", result.AssetType);
        Assert.Equal(1, result.Captured);
        Assert.Equal(0, result.Skipped);
        Assert.Equal(PriceHistoryStore.ProviderSource, result.Source);
    }

    private static async Task<CaptureResult> StoreCapture(
        PriceHistoryStore store,
        string symbol,
        AssetType type,
        List<FmpHistoricalPrice> prices
    )
    {
        var method = typeof(PriceHistoryEndpoints).GetMethod("Store", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new MissingMethodException(nameof(PriceHistoryEndpoints), "Store");
        var task = (Task<IResult>?)method.Invoke(null, [store, symbol, type, null, prices])
            ?? throw new InvalidOperationException("Price history capture did not return a result task.");
        var snapshot = await Execute(await task);
        Assert.Equal(StatusCodes.Status200OK, snapshot.StatusCode);
        return JsonSerializer.Deserialize<CaptureResult>(snapshot.Body, JsonOptions)!;
    }

    private static FmpHistoricalPrice Price(string date)
    {
        var day = DateOnly.Parse(date, CultureInfo.InvariantCulture);
        return new FmpHistoricalPrice("AMD", day, 10m, 11m, 9m, 10m, 1000);
    }

    private static async Task<ResponseSnapshot> Execute(IResult result)
    {
        var context = new DefaultHttpContext { Response = { Body = new MemoryStream() } };
        context.RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider();
        await result.ExecuteAsync(context);
        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        return new ResponseSnapshot(context.Response.StatusCode, body);
    }

    private sealed record ResponseSnapshot(int StatusCode, string Body);
}
