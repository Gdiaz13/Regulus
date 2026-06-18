using api.Data;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Endpoints;

public static class StockEndpoints
{
    public static void MapStockEndpoints(this WebApplication app)
    {
        var stocks = app.MapGroup("/api/stocks");
        stocks.MapGet("", GetStocks);
        stocks.MapGet("{symbol}", GetStock);
        stocks.MapPost("", CreateStock);
        stocks.MapDelete("{id:int}", DeleteStock);
    }

    private static async Task<List<Stock>> GetStocks(ApplicationDBContext db)
    {
        return await db.Stocks.AsNoTracking().OrderBy(stock => stock.Symbol).ToListAsync();
    }

    private static async Task<IResult> GetStock(string symbol, ApplicationDBContext db)
    {
        var normalizedSymbol = NormalizeSymbol(symbol);
        var stock = await FindStockBySymbol(db, normalizedSymbol);
        return stock is null
            ? Results.NotFound($"Stock {normalizedSymbol} was not found.")
            : Results.Ok(stock);
    }

    private static async Task<IResult> CreateStock(CreateStockRequest request, ApplicationDBContext db)
    {
        var normalizedSymbol = NormalizeSymbol(request.Symbol);
        var validationResult = await ValidateStockRequest(db, normalizedSymbol);
        if (validationResult is not null)
        {
            return validationResult;
        }
        var stock = CreateStockEntity(request, normalizedSymbol);
        db.Stocks.Add(stock);
        await db.SaveChangesAsync();
        return Results.Created($"/api/stocks/{stock.Symbol}", stock);
    }

    private static async Task<IResult> DeleteStock(int id, ApplicationDBContext db)
    {
        var stock = await db.Stocks.FindAsync(id);
        if (stock is null)
        {
            return Results.NotFound($"Stock with id {id} was not found.");
        }
        db.Stocks.Remove(stock);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static Stock CreateStockEntity(CreateStockRequest request, string symbol)
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

    private static async Task<IResult?> ValidateStockRequest(ApplicationDBContext db, string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            return Results.BadRequest("A stock symbol is required.");
        }
        if (await StockExists(db, symbol))
        {
            return Results.Conflict($"{symbol} is already in your portfolio.");
        }
        return null;
    }

    private static Task<Stock?> FindStockBySymbol(ApplicationDBContext db, string symbol)
    {
        return db.Stocks.AsNoTracking().FirstOrDefaultAsync(stock => stock.Symbol == symbol);
    }

    private static Task<bool> StockExists(ApplicationDBContext db, string symbol)
    {
        return db.Stocks.AnyAsync(stock => stock.Symbol == symbol);
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
