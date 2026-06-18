using api.Data;
using api.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder
    .Services
    .AddDbContext<ApplicationDBContext>(
        options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    );

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

app.MapGet("/", () => "Exchange API running");

app.MapGet(
    "/api/stocks",
    async (ApplicationDBContext db) =>
        await db.Stocks.AsNoTracking().OrderBy(stock => stock.Symbol).ToListAsync()
);

app.MapGet(
    "/api/stocks/{symbol}",
    async (string symbol, ApplicationDBContext db) =>
    {
        var normalizedSymbol = NormalizeSymbol(symbol);
        var stock = await db
            .Stocks
            .AsNoTracking()
            .FirstOrDefaultAsync(stock => stock.Symbol == normalizedSymbol);

        return stock is null
            ? Results.NotFound($"Stock {normalizedSymbol} was not found.")
            : Results.Ok(stock);
    }
);

app.MapPost(
    "/api/stocks",
    async (CreateStockRequest request, ApplicationDBContext db) =>
    {
        var normalizedSymbol = NormalizeSymbol(request.Symbol);

        if (string.IsNullOrWhiteSpace(normalizedSymbol))
        {
            return Results.BadRequest("A stock symbol is required.");
        }

        var exists = await db.Stocks.AnyAsync(stock => stock.Symbol == normalizedSymbol);

        if (exists)
        {
            return Results.Conflict($"{normalizedSymbol} is already in your portfolio.");
        }

        var stock = new Stock
        {
            Symbol = normalizedSymbol,
            CompanyName = request.CompanyName?.Trim() ?? normalizedSymbol,
            PurchasePrice = request.PurchasePrice ?? 0,
            LastDividend = request.LastDividend ?? 0,
            Industry = request.Industry?.Trim() ?? string.Empty,
            MarketCap = request.MarketCap ?? 0,
        };

        db.Stocks.Add(stock);
        await db.SaveChangesAsync();

        return Results.Created($"/api/stocks/{stock.Symbol}", stock);
    }
);

app.MapDelete(
    "/api/stocks/{id:int}",
    async (int id, ApplicationDBContext db) =>
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
);

app.Run();

static string NormalizeSymbol(string? symbol) => symbol?.Trim().ToUpperInvariant() ?? string.Empty;

record CreateStockRequest(
    string? Symbol,
    string? CompanyName,
    decimal? PurchasePrice,
    decimal? LastDividend,
    string? Industry,
    long? MarketCap
);
