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

    public async Task<List<Stock>> ListAsync()
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        var rows = await connection.QueryAsync<Stock>(Sql.List);
        return rows.ToList();
    }

    public async Task<Stock?> FindBySymbolAsync(string symbol)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<Stock>(Sql.FindBySymbol, new { Symbol = symbol });
    }

    public async Task<Stock> CreateAsync(Stock stock)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await connection.QuerySingleAsync<Stock>(Sql.Insert, stock);
    }

    public async Task<Stock?> UpdateAsync(int id, Stock stock)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<Stock>(Sql.Update, Params(id, stock));
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await connection.ExecuteAsync(Sql.Delete, new { Id = id }) > 0;
    }

    public async Task<bool> ExistsAsync(string symbol)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await connection.ExecuteScalarAsync<bool>(Sql.Exists, new { Symbol = symbol });
    }

    public async Task<bool> SymbolBelongsToAnotherAsync(int id, string symbol)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await connection.ExecuteScalarAsync<bool>(Sql.ExistsForOther, new { Id = id, Symbol = symbol });
    }

    private static object Params(int id, Stock stock)
    {
        return new
        {
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
            order by symbol;
            """;

        public const string FindBySymbol = $"""
            select {Columns}
            from stocks
            where symbol = @Symbol;
            """;

        public const string Exists = """
            select exists(select 1 from stocks where symbol = @Symbol);
            """;

        public const string ExistsForOther = """
            select exists(select 1 from stocks where id <> @Id and symbol = @Symbol);
            """;

        public const string Insert = $"""
            insert into stocks (symbol, company_name, purchase_price, last_dividend, industry, market_cap)
            values (@Symbol, @CompanyName, @PurchasePrice, @LastDividend, @Industry, @MarketCap)
            returning {Columns};
            """;

        public const string Update = $"""
            update stocks
            set symbol = @Symbol, company_name = @CompanyName, purchase_price = @PurchasePrice,
                last_dividend = @LastDividend, industry = @Industry, market_cap = @MarketCap
            where id = @Id
            returning {Columns};
            """;

        public const string Delete = "delete from stocks where id = @Id;";
    }
}
