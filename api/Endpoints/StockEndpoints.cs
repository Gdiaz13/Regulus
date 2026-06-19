using api.Data;
using api.Models;
using api.Services;
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
        stocks.MapPut("{id:int}", UpdateStock);
        stocks.MapDelete("{id:int}", DeleteStock);
    }

    private static Task<IResult> GetStocks(ApplicationDBContext db)
    {
        return DatabaseRequest.Run(async () => Results.Ok(await ListStocks(db)));
    }

    private static Task<IResult> GetStock(string symbol, ApplicationDBContext db)
    {
        return DatabaseRequest.Run(() => GetStockCore(symbol, db));
    }

    private static async Task<IResult> GetStockCore(string symbol, ApplicationDBContext db)
    {
        var normalizedSymbol = NormalizeSymbol(symbol);
        var stock = await FindStockBySymbol(db, normalizedSymbol);
        return stock is null
            ? Results.NotFound($"Stock {normalizedSymbol} was not found.")
            : Results.Ok(stock);
    }

    private static Task<IResult> CreateStock(CreateStockRequest request, ApplicationDBContext db)
    {
        return DatabaseRequest.Run(() => CreateStockCore(request, db));
    }

    private static async Task<IResult> CreateStockCore(CreateStockRequest request, ApplicationDBContext db)
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

    private static Task<IResult> DeleteStock(int id, ApplicationDBContext db)
    {
        return DatabaseRequest.Run(() => DeleteStockCore(id, db));
    }

    private static Task<IResult> UpdateStock(
        int id,
        CreateStockRequest request,
        ApplicationDBContext db
    )
    {
        return DatabaseRequest.Run(() => UpdateStockCore(id, request, db));
    }

    private static async Task<IResult> UpdateStockCore(
        int id,
        CreateStockRequest request,
        ApplicationDBContext db
    )
    {
        var symbol = NormalizeSymbol(request.Symbol);
        var validation = await ValidateStockUpdate(db, id, symbol);
        if (validation is not null)
        {
            return validation;
        }
        return await SaveUpdatedStock(id, request, symbol, db);
    }

    private static async Task<IResult> DeleteStockCore(int id, ApplicationDBContext db)
    {
        var stock = await db.Stocks.FindAsync(id);
        if (stock is null)
        {
            return Results.NotFound($"Stock with id {id} was not found.");
        }
        await DeleteStockComments(id, db);
        db.Stocks.Remove(stock);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> SaveUpdatedStock(
        int id,
        CreateStockRequest request,
        string symbol,
        ApplicationDBContext db
    )
    {
        var stock = await db.Stocks.FindAsync(id);
        if (stock is null)
        {
            return StockMissing(id);
        }
        return await SaveStockUpdate(stock, request, symbol, db);
    }

    private static async Task<IResult> SaveStockUpdate(
        Stock stock,
        CreateStockRequest request,
        string symbol,
        ApplicationDBContext db
    )
    {
        ApplyStockUpdate(stock, request, symbol);
        await db.SaveChangesAsync();
        return Results.Ok(stock);
    }

    private static IResult StockMissing(int id)
    {
        return Results.NotFound($"Stock with id {id} was not found.");
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

    private static void ApplyStockUpdate(Stock stock, CreateStockRequest request, string symbol)
    {
        stock.Symbol = symbol;
        stock.CompanyName = request.CompanyName?.Trim() ?? symbol;
        stock.PurchasePrice = request.PurchasePrice ?? 0;
        stock.LastDividend = request.LastDividend ?? 0;
        stock.Industry = request.Industry?.Trim() ?? string.Empty;
        stock.MarketCap = request.MarketCap ?? 0;
    }

    private static Task<List<Stock>> ListStocks(ApplicationDBContext db)
    {
        return db.Stocks.AsNoTracking().OrderBy(stock => stock.Symbol).ToListAsync();
    }

    private static Task DeleteStockComments(int stockId, ApplicationDBContext db)
    {
        return db.Comments.Where(comment => comment.StockId == stockId).ExecuteDeleteAsync();
    }

    private static async Task<IResult?> ValidateStockRequest(ApplicationDBContext db, string symbol)
    {
        var validation = ValidateSymbol(symbol);
        if (validation is not null)
        {
            return validation;
        }
        if (await StockExists(db, symbol))
        {
            return Results.Conflict($"{symbol} is already in your portfolio.");
        }
        return null;
    }

    private static async Task<IResult?> ValidateStockUpdate(ApplicationDBContext db, int id, string symbol)
    {
        var validation = ValidateSymbol(symbol);
        if (validation is not null)
        {
            return validation;
        }
        if (await SymbolBelongsToAnotherStock(db, id, symbol))
        {
            return Results.Conflict($"{symbol} is already in your portfolio.");
        }
        return null;
    }

    private static IResult? ValidateSymbol(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            return Results.BadRequest("A stock symbol is required.");
        }
        return SymbolIsTooLong(symbol) ? SymbolTooLong() : null;
    }

    private static Task<Stock?> FindStockBySymbol(ApplicationDBContext db, string symbol)
    {
        return db.Stocks.AsNoTracking().FirstOrDefaultAsync(stock => stock.Symbol == symbol);
    }

    private static Task<bool> StockExists(ApplicationDBContext db, string symbol)
    {
        return db.Stocks.AnyAsync(stock => stock.Symbol == symbol);
    }

    private static Task<bool> SymbolBelongsToAnotherStock(ApplicationDBContext db, int id, string symbol)
    {
        return db.Stocks.AnyAsync(stock => stock.Id != id && stock.Symbol == symbol);
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
