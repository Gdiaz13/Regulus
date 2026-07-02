using api.Models;
using Dapper;

namespace api.Services;

// Dapper-backed portfolio stock store. This is the first portfolio path moved
// onto PostgreSQL while endpoint contracts stay the same for the web app.
public sealed class PortfolioStockStore
{
    private readonly IDatabaseConnectionFactory _factory;

    public PortfolioStockStore(IDatabaseConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<List<Stock>> ListAsync(Guid userId)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        var rows = await connection.QueryAsync<Stock>(Sql.List, new { UserId = userId });
        return rows.ToList();
    }

    public async Task<Stock?> FindBySymbolAsync(Guid userId, string symbol)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<Stock>(Sql.FindBySymbol, new { UserId = userId, Symbol = symbol });
    }

    public async Task<Stock> CreateAsync(Guid userId, Stock stock)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await connection.QuerySingleAsync<Stock>(Sql.Insert, Params(userId, 0, stock));
    }

    public async Task<Stock?> UpdateAsync(Guid userId, int id, Stock stock)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<Stock>(Sql.Update, Params(userId, id, stock));
    }

    public async Task<bool> DeleteAsync(Guid userId, int id)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await connection.ExecuteAsync(Sql.Delete, new { UserId = userId, Id = id }) > 0;
    }

    public async Task<bool> ExistsAsync(Guid userId, string symbol)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await connection.ExecuteScalarAsync<bool>(Sql.Exists, new { UserId = userId, Symbol = symbol });
    }

    public async Task<bool> SymbolBelongsToAnotherAsync(Guid userId, int id, string symbol)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await connection.ExecuteScalarAsync<bool>(Sql.ExistsForOther, new { UserId = userId, Id = id, Symbol = symbol });
    }

    // Every distinct symbol anyone tracks. The background snapshot job uses this
    // because price history is global market data, not per-user data.
    public async Task<List<string>> ListTrackedSymbolsAsync(CancellationToken token = default)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync(token);
        var rows = await connection.QueryAsync<string>(Sql.TrackedSymbols);
        return rows.ToList();
    }

    private static object Params(Guid userId, int id, Stock stock)
    {
        return new
        {
            UserId = userId,
            Id = id,
            stock.Symbol,
            stock.CompanyName,
            stock.PurchasePrice,
            stock.LastDividend,
            stock.Industry,
            stock.MarketCap,
        };
    }

    private static class Sql
    {
        private const string Columns = """
            id as "Id", symbol as "Symbol", company_name as "CompanyName",
            purchase_price as "PurchasePrice", last_dividend as "LastDividend",
            industry as "Industry", market_cap as "MarketCap"
            """;

        public const string List = $"""
            select {Columns}
            from stocks
            where user_id = @UserId
            order by symbol;
            """;

        public const string FindBySymbol = $"""
            select {Columns}
            from stocks
            where user_id = @UserId and symbol = @Symbol;
            """;

        public const string Exists = """
            select exists(select 1 from stocks where user_id = @UserId and symbol = @Symbol);
            """;

        public const string ExistsForOther = """
            select exists(select 1 from stocks where user_id = @UserId and id <> @Id and symbol = @Symbol);
            """;

        public const string Insert = $"""
            insert into stocks (user_id, symbol, company_name, purchase_price, last_dividend, industry, market_cap)
            values (@UserId, @Symbol, @CompanyName, @PurchasePrice, @LastDividend, @Industry, @MarketCap)
            returning {Columns};
            """;

        public const string Update = $"""
            update stocks
            set symbol = @Symbol, company_name = @CompanyName, purchase_price = @PurchasePrice,
                last_dividend = @LastDividend, industry = @Industry, market_cap = @MarketCap
            where user_id = @UserId and id = @Id
            returning {Columns};
            """;

        public const string Delete = "delete from stocks where user_id = @UserId and id = @Id;";

        public const string TrackedSymbols = "select distinct symbol from stocks order by symbol;";
    }
}
