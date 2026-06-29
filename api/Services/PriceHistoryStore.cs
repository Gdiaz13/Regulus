using System.Data.Common;
using api.Contracts;
using api.Models;
using Dapper;

namespace api.Services;

// Stores provider price history through Dapper so charts and future models read
// from PostgreSQL.
public sealed class PriceHistoryStore
{
    public const string ProviderSource = "FMP";
    private readonly IDatabaseConnectionFactory _factory;

    public PriceHistoryStore(IDatabaseConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<PriceHistoryAsset> EnsureAssetAsync(string symbol, AssetType type, string? name)
    {
        var clean = Normalize(symbol);
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await FindAsset(connection, clean, type) ?? await CreateAsset(connection, clean, type, name);
    }

    public async Task<int> SaveAsync(long assetId, IEnumerable<FmpHistoricalPrice> prices)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        var rows = prices.Select(price => Params(assetId, price)).ToList();
        return rows.Count == 0 ? 0 : await connection.ExecuteAsync(Sql.InsertPrice, rows);
    }

    public async Task<List<PricePoint>> ListPointsAsync(string symbol, AssetType type, int take)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        var query = ListParams(Normalize(symbol), type, take);
        var rows = await connection.QueryAsync<PricePointRow>(Sql.ListPoints, query);
        return rows.Select(ToPoint).ToList();
    }

    private static Task<PriceHistoryAsset?> FindAsset(DbConnection connection, string symbol, AssetType type)
    {
        return connection.QuerySingleOrDefaultAsync<PriceHistoryAsset>(Sql.FindAsset, AssetParams(symbol, type));
    }

    private static Task<PriceHistoryAsset> CreateAsset(DbConnection connection, string symbol, AssetType type, string? name)
    {
        return connection.QuerySingleAsync<PriceHistoryAsset>(Sql.InsertAsset, AssetParams(symbol, type, CleanName(name, symbol)));
    }

    private static object Params(long assetId, FmpHistoricalPrice price)
    {
        return new
        {
            AssetId = assetId,
            Date = price.Date.ToDateTime(TimeOnly.MinValue),
            price.Open,
            price.High,
            price.Low,
            price.Close,
            price.Volume,
            Source = ProviderSource,
        };
    }

    private static object AssetParams(string symbol, AssetType type, string? name = null)
    {
        return new { Symbol = symbol, AssetType = type.ToString(), Name = name ?? symbol };
    }

    private static object ListParams(string symbol, AssetType type, int take)
    {
        return new { Symbol = symbol, AssetType = type.ToString(), Take = take };
    }

    private static PricePoint ToPoint(PricePointRow row)
    {
        return new PricePoint(DateOnly.FromDateTime(row.Date), row.Open, row.High, row.Low, row.Close, row.Volume, row.Source);
    }

    private static string CleanName(string? name, string symbol)
    {
        return string.IsNullOrWhiteSpace(name) ? symbol : name.Trim();
    }

    private static string Normalize(string symbol)
    {
        return symbol.Trim().ToUpperInvariant();
    }

    private sealed class PricePointRow
    {
        public DateTime Date { get; init; }
        public decimal Open { get; init; }
        public decimal High { get; init; }
        public decimal Low { get; init; }
        public decimal Close { get; init; }
        public long Volume { get; init; }
        public string Source { get; init; } = string.Empty;
    }

    private static class Sql
    {
        public const string FindAsset = """
            select id as "Id", symbol as "Symbol"
            from assets
            where symbol = @Symbol and asset_type = @AssetType;
            """;

        public const string InsertAsset = """
            insert into assets (symbol, name, asset_type)
            values (@Symbol, @Name, @AssetType)
            returning id as "Id", symbol as "Symbol";
            """;

        public const string InsertPrice = """
            insert into price_history
                (asset_id, date, open_price, high_price, low_price, close_price, volume, source)
            values
                (@AssetId, @Date, @Open, @High, @Low, @Close, @Volume, @Source)
            on conflict(asset_id, date) do nothing;
            """;

        public const string ListPoints = """
            select "Date", "Open", "High", "Low", "Close", "Volume", "Source"
            from (
                select p.date as "Date", p.open_price as "Open", p.high_price as "High",
                       p.low_price as "Low", p.close_price as "Close", p.volume as "Volume",
                       p.source as "Source"
                from price_history p
                join assets a on a.id = p.asset_id
                where a.symbol = @Symbol and a.asset_type = @AssetType
                order by p.date desc
                limit @Take
            ) latest
            order by "Date";
            """;
    }
}

public sealed class PriceHistoryAsset
{
    public long Id { get; init; }
    public string Symbol { get; init; } = string.Empty;
}
