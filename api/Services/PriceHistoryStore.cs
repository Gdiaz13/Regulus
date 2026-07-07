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
    public const string ManualSource = "Manual";
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

    // Manual entries exist for assets with no provider feed yet (TCG cards first).
    // One price stands in for the whole day, so it fills all four OHLC columns.
    public async Task<int> SaveManualAsync(long assetId, ManualPriceRequest request)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await connection.ExecuteAsync(Sql.InsertManualPrice, ManualParams(assetId, request));
    }

    // A browsed card's provider market price; tcgplayer prices are USD market quotes.
    public async Task<int> SaveProviderCardPriceAsync(long assetId, DateOnly date, decimal price, string source)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await connection.ExecuteAsync(Sql.InsertManualPrice, CardPriceParams(assetId, date, price, source));
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

    private static object ManualParams(long assetId, ManualPriceRequest request)
    {
        return new
        {
            AssetId = assetId,
            Date = request.Date.ToDateTime(TimeOnly.MinValue),
            request.Price,
            Source = ManualSource,
            PriceType = CleanOrNull(request.PriceType),
            CardCondition = CleanOrNull(request.CardCondition),
            Grade = CleanOrNull(request.Grade),
            Currency = CleanOrNull(request.Currency)?.ToUpperInvariant(),
        };
    }

    private static object CardPriceParams(long assetId, DateOnly date, decimal price, string source)
    {
        return new
        {
            AssetId = assetId,
            Date = date.ToDateTime(TimeOnly.MinValue),
            Price = price,
            Source = source,
            PriceType = "Market",
            CardCondition = (string?)null,
            Grade = (string?)null,
            Currency = "USD",
        };
    }

    private static PricePoint ToPoint(PricePointRow row)
    {
        return new PricePoint(
            row.Date, row.Open, row.High, row.Low, row.Close, row.Volume,
            row.Source, row.PriceType, row.CardCondition, row.Grade, row.Currency
        );
    }

    private static string? CleanOrNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string CleanName(string? name, string symbol)
    {
        return string.IsNullOrWhiteSpace(name) ? symbol : name.Trim();
    }

    private static string Normalize(string symbol)
    {
        return symbol.Trim().ToUpperInvariant();
    }

    // Npgsql returns PostgreSQL date columns as DateOnly, so the row matches that.
    private sealed class PricePointRow
    {
        public DateOnly Date { get; init; }
        public decimal Open { get; init; }
        public decimal High { get; init; }
        public decimal Low { get; init; }
        public decimal Close { get; init; }
        public long Volume { get; init; }
        public string Source { get; init; } = string.Empty;
        public string? PriceType { get; init; }
        public string? CardCondition { get; init; }
        public string? Grade { get; init; }
        public string? Currency { get; init; }
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

        public const string InsertManualPrice = """
            insert into price_history
                (asset_id, date, open_price, high_price, low_price, close_price, volume, source,
                 price_type, card_condition, grade, currency)
            values
                (@AssetId, @Date, @Price, @Price, @Price, @Price, 0, @Source,
                 @PriceType, @CardCondition, @Grade, @Currency)
            on conflict(asset_id, date) do nothing;
            """;

        public const string ListPoints = """
            select "Date", "Open", "High", "Low", "Close", "Volume", "Source",
                   "PriceType", "CardCondition", "Grade", "Currency"
            from (
                select p.date as "Date", p.open_price as "Open", p.high_price as "High",
                       p.low_price as "Low", p.close_price as "Close", p.volume as "Volume",
                       p.source as "Source", p.price_type as "PriceType",
                       p.card_condition as "CardCondition", p.grade as "Grade", p.currency as "Currency"
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
