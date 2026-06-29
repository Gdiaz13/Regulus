using api.Models;
using api.Services;

namespace api.Endpoints;

public static class StockEndpoints
{
    public static void MapStockEndpoints(this WebApplication app)
    {
        var stocks = app.MapGroup("/api/stocks");
        stocks.MapGet("", GetStocks);
        stocks.MapGet("{symbol}", GetStock);
        stocks.MapPost("", CreateStock);
        stocks.MapPut("{id:int}", UpdateStock);
        stocks.MapDelete("{id:int}", DeleteStock);
    }

    private static Task<IResult> GetStocks(PortfolioStockStore store)
    {
        return DatabaseRequest.Run(async () => Results.Ok(await store.ListAsync()));
    }

    private static Task<IResult> GetStock(string symbol, PortfolioStockStore store)
    {
        return DatabaseRequest.Run(() => GetStockCore(symbol, store));
    }

    private static async Task<IResult> GetStockCore(string symbol, PortfolioStockStore store)
    {
        var normalizedSymbol = NormalizeSymbol(symbol);
        var stock = await store.FindBySymbolAsync(normalizedSymbol);
        return stock is null ? Results.NotFound($"Stock {normalizedSymbol} was not found.") : Results.Ok(stock);
    }

    private static Task<IResult> CreateStock(CreateStockRequest request, PortfolioStockStore store)
    {
        return DatabaseRequest.Run(() => CreateStockCore(request, store));
    }

    private static async Task<IResult> CreateStockCore(CreateStockRequest request, PortfolioStockStore store)
    {
        var symbol = NormalizeSymbol(request.Symbol);
        var validation = await ValidateCreate(store, request, symbol);
        if (validation is not null)
        {
            return validation;
        }
        var stock = await store.CreateAsync(ToStock(request, symbol));
        return Results.Created($"/api/stocks/{stock.Symbol}", stock);
    }

    private static Task<IResult> UpdateStock(int id, CreateStockRequest request, PortfolioStockStore store)
    {
        return DatabaseRequest.Run(() => UpdateStockCore(id, request, store));
    }

    private static async Task<IResult> UpdateStockCore(int id, CreateStockRequest request, PortfolioStockStore store)
    {
        var symbol = NormalizeSymbol(request.Symbol);
        var validation = await ValidateUpdate(store, id, request, symbol);
        if (validation is not null)
        {
            return validation;
        }
        var stock = await store.UpdateAsync(id, ToStock(request, symbol));
        return stock is null ? StockMissing(id) : Results.Ok(stock);
    }

    private static Task<IResult> DeleteStock(int id, PortfolioStockStore store)
    {
        return DatabaseRequest.Run(async () => await DeleteStockCore(id, store));
    }

    private static async Task<IResult> DeleteStockCore(int id, PortfolioStockStore store)
    {
        return await store.DeleteAsync(id) ? Results.NoContent() : StockMissing(id);
    }

    private static async Task<IResult?> ValidateCreate(
        PortfolioStockStore store,
        CreateStockRequest request,
        string symbol
    )
    {
        var validation = ValidateRequest(request, symbol);
        if (validation is not null)
        {
            return validation;
        }
        return await store.ExistsAsync(symbol) ? Results.Conflict($"{symbol} is already in your portfolio.") : null;
    }

    private static async Task<IResult?> ValidateUpdate(
        PortfolioStockStore store,
        int id,
        CreateStockRequest request,
        string symbol
    )
    {
        var validation = ValidateRequest(request, symbol);
        if (validation is not null)
        {
            return validation;
        }
        return await DuplicateUpdate(store, id, symbol) ? Results.Conflict($"{symbol} is already in your portfolio.") : null;
    }

    private static Task<bool> DuplicateUpdate(PortfolioStockStore store, int id, string symbol)
    {
        return store.SymbolBelongsToAnotherAsync(id, symbol);
    }

    private static IResult? ValidateRequest(CreateStockRequest request, string symbol)
    {
        var validation = ValidateSymbol(symbol);
        return validation ?? ValidateStockNumbers(request);
    }

    private static IResult? ValidateStockNumbers(CreateStockRequest request)
    {
        return HasNegativeNumber(request) ? NegativeNumbers() : null;
    }

    private static bool HasNegativeNumber(CreateStockRequest request)
    {
        return IsNegative(request.PurchasePrice) || IsNegative(request.LastDividend) || IsNegative(request.MarketCap);
    }

    private static bool IsNegative(decimal? value)
    {
        return value < 0;
    }

    private static bool IsNegative(long? value)
    {
        return value < 0;
    }

    private static IResult? ValidateSymbol(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            return Results.BadRequest("A stock symbol is required.");
        }
        return SymbolIsTooLong(symbol) ? SymbolTooLong() : null;
    }

    private static Stock ToStock(CreateStockRequest request, string symbol)
    {
        return new Stock
        {
            Symbol = symbol,
            CompanyName = request.CompanyName?.Trim() ?? symbol,
            PurchasePrice = request.PurchasePrice ?? 0,
            LastDividend = request.LastDividend ?? 0,
            Industry = request.Industry?.Trim() ?? string.Empty,
            MarketCap = request.MarketCap ?? 0,
        };
    }

    private static IResult StockMissing(int id)
    {
        return Results.NotFound($"Stock with id {id} was not found.");
    }

    private static IResult NegativeNumbers()
    {
        return Results.BadRequest("Portfolio numbers cannot be negative.");
    }

    private static bool SymbolIsTooLong(string symbol)
    {
        return symbol.Length > Stock.SymbolMaxLength;
    }

    private static IResult SymbolTooLong()
    {
        return Results.BadRequest($"Stock symbols must be {Stock.SymbolMaxLength} characters or less.");
    }

    private static string NormalizeSymbol(string? symbol)
    {
        return symbol?.Trim().ToUpperInvariant() ?? string.Empty;
    }
}

public sealed record CreateStockRequest(
    string? Symbol,
    string? CompanyName,
    decimal? PurchasePrice,
    decimal? LastDividend,
    string? Industry,
    long? MarketCap
);
