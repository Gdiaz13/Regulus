using System.Globalization;
using System.Reflection;
using System.Text.Json;
using api.Contracts;
using api.Endpoints;
using api.Models;
using api.Services;
using Dapper;
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

    [Fact]
    public async Task Manual_capture_attaches_tcg_game_category()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new PriceHistoryStore(factory);
        var request = Request("Magic");
        var result = await StoreManual(store, "lea-161", AssetType.TcgCard, request);
        Assert.Equal("LEA-161", result.Symbol);
        Assert.Equal("Magic", await AssetCategory(factory, result.AssetId));
    }

    [Fact]
    public async Task Manual_capture_rejects_conflicting_tcg_game_category()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new PriceHistoryStore(factory);
        await store.EnsureAssetAsync("lea-161", AssetType.TcgCard, "Lightning Bolt", "Pokemon");
        var response = await StoreManualResponse(store, "lea-161", AssetType.TcgCard, Request("Magic"));
        Assert.Equal(StatusCodes.Status409Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Manual_capture_rejects_unknown_tcg_game_category()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new PriceHistoryStore(factory);
        var response = await StoreManualResponse(store, "lea-161", AssetType.TcgCard, Request("Digimon"));
        Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
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

    private static async Task<CaptureResult> StoreManual(
        PriceHistoryStore store,
        string symbol,
        AssetType type,
        ManualPriceRequest request
    )
    {
        var snapshot = await StoreManualResponse(store, symbol, type, request);
        Assert.Equal(StatusCodes.Status200OK, snapshot.StatusCode);
        return JsonSerializer.Deserialize<CaptureResult>(snapshot.Body, JsonOptions)!;
    }

    private static async Task<ResponseSnapshot> StoreManualResponse(
        PriceHistoryStore store,
        string symbol,
        AssetType type,
        ManualPriceRequest request
    )
    {
        var method = typeof(PriceHistoryEndpoints).GetMethod("StoreManual", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new MissingMethodException(nameof(PriceHistoryEndpoints), "StoreManual");
        var task = (Task<IResult>?)method.Invoke(null, [store, symbol, type, request])
            ?? throw new InvalidOperationException("Manual price capture did not return a result task.");
        return await Execute(await task);
    }

    private static FmpHistoricalPrice Price(string date)
    {
        var day = DateOnly.Parse(date, CultureInfo.InvariantCulture);
        return new FmpHistoricalPrice("AMD", day, 10m, 11m, 9m, 10m, 1000);
    }

    private static ManualPriceRequest Request(string category)
    {
        return new ManualPriceRequest(
            DateOnly.Parse("2026-02-03", CultureInfo.InvariantCulture),
            5.25m,
            "Sold",
            null,
            null,
            "USD",
            "Lightning Bolt",
            category
        );
    }

    private static async Task<string?> AssetCategory(SqliteDapperConnectionFactory factory, int assetId)
    {
        await using var connection = await factory.OpenDatabaseConnectionAsync();
        return await connection.ExecuteScalarAsync<string?>("""
            select c.name
            from assets a
            join asset_categories c on c.id = a.category_id
            where a.id = @AssetId;
            """, new { AssetId = assetId });
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
