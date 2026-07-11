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
    private static readonly string[] SupportedTcgCategories = ["Pokemon", "Magic", "One Piece"];
    private readonly IDatabaseConnectionFactory _factory;

    public PriceHistoryStore(IDatabaseConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<PriceHistoryAsset> EnsureAssetAsync(string symbol, AssetType type, string? name, string? category = null)
    {
        var clean = Normalize(symbol);
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        var existing = await FindAsset(connection, clean, type);
        return existing is null
            ? await CreateAsset(connection, clean, type, name, category)
            : await ApplyCategory(connection, existing, type, category);
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

    // A browsed card's provider market price; currency comes from the provider feed.
    public async Task<int> SaveProviderCardPriceAsync(long assetId, DateOnly date, decimal price, string source, string currency = "USD")
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await connection.ExecuteAsync(Sql.InsertManualPrice, CardPriceParams(assetId, date, price, source, currency));
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

    private static async Task<PriceHistoryAsset> ApplyCategory(
        DbConnection connection,
        PriceHistoryAsset asset,
        AssetType type,
        string? category
    )
    {
        var clean = NormalizeCategory(type, category);
        if (NoCategoryChange(asset, clean))
        {
            return asset;
        }
        ThrowIfCategoryConflicts(asset, clean);
        return await AttachCategory(connection, asset, type, clean!);
    }

    private static bool NoCategoryChange(PriceHistoryAsset asset, string? category)
    {
        return category is null || CategoryMatches(asset.Category, category);
    }

    private static void ThrowIfCategoryConflicts(PriceHistoryAsset asset, string? category)
    {
        if (!string.IsNullOrWhiteSpace(asset.Category) && category is not null)
        {
            throw new TcgAssetCategoryConflictException(asset.Symbol, asset.Category, category);
        }
    }

    private static async Task<PriceHistoryAsset> AttachCategory(
        DbConnection connection,
        PriceHistoryAsset asset,
        AssetType type,
        string category
    )
    {
        var categoryId = await CategoryId(connection, type, category);
        await connection.ExecuteAsync(Sql.UpdateAssetCategory, new { asset.Id, CategoryId = categoryId });
        return new PriceHistoryAsset { Id = asset.Id, Symbol = asset.Symbol, Category = Clean(category) };
    }

    private static async Task<PriceHistoryAsset> CreateAsset(DbConnection connection, string symbol, AssetType type, string? name, string? category)
    {
        var categoryId = await CategoryId(connection, type, category);
        return await connection.QuerySingleAsync<PriceHistoryAsset>(Sql.InsertAsset, AssetParams(symbol, type, CleanName(name, symbol), categoryId));
    }

    private static async Task<int?> CategoryId(DbConnection connection, AssetType type, string? category)
    {
        var clean = NormalizeCategory(type, category);
        if (clean is null)
        {
            return null;
        }
        return await FindCategoryId(connection, clean) ?? await InsertCategory(connection, type, clean);
    }

    private static Task<int?> FindCategoryId(DbConnection connection, string category)
    {
        return connection.ExecuteScalarAsync<int?>(Sql.FindCategoryId, CategoryParams(category));
    }

    private static Task<int> InsertCategory(DbConnection connection, AssetType type, string category)
    {
        return connection.ExecuteScalarAsync<int>(Sql.InsertCategory, CategoryParams(category, type));
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

    private static object AssetParams(string symbol, AssetType type, string? name = null, int? categoryId = null)
    {
        return new { Symbol = symbol, AssetType = type.ToString(), Name = name ?? symbol, CategoryId = categoryId };
    }

    private static object CategoryParams(string category, AssetType? type = null)
    {
        return new { Name = Clean(category), Slug = Slug(category), AssetType = type?.ToString() };
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

    private static object CardPriceParams(long assetId, DateOnly date, decimal price, string source, string currency)
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
            Currency = CleanOrNull(currency)?.ToUpperInvariant() ?? "USD",
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

    private static string Slug(string value)
    {
        return Clean(value).ToLowerInvariant().Replace(' ', '-');
    }

    private static string Clean(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }

    private static bool CategoryMatches(string? current, string requested)
    {
        return string.Equals(current?.Trim(), requested.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static string? NormalizeCategory(AssetType type, string? category)
    {
        var clean = CleanOrNull(category);
        if (clean is null || type != AssetType.TcgCard)
        {
            return clean;
        }
        return SupportedTcgCategories.FirstOrDefault(supported => CategoryMatches(supported, clean))
            ?? throw new InvalidTcgAssetCategoryException(clean);
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
            select a.id as "Id", a.symbol as "Symbol", c.name as "Category"
            from assets a
            left join asset_categories c on c.id = a.category_id
            where a.symbol = @Symbol and a.asset_type = @AssetType;
            """;

        public const string FindCategoryId = """
            select id from asset_categories where slug = @Slug;
            """;

        public const string InsertCategory = """
            insert into asset_categories (name, slug, asset_type)
            values (@Name, @Slug, @AssetType)
            returning id;
            """;

        public const string InsertAsset = """
            insert into assets (symbol, name, asset_type, category_id)
            values (@Symbol, @Name, @AssetType, @CategoryId)
            returning id as "Id", symbol as "Symbol";
            """;

        public const string UpdateAssetCategory = """
            update assets
            set category_id = @CategoryId
            where id = @Id;
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
    public string? Category { get; init; }
}

public sealed class TcgAssetCategoryConflictException : Exception
{
    public TcgAssetCategoryConflictException(string symbol, string existing, string requested)
        : base($"{symbol} is already categorized as {existing}, not {requested}.")
    {
    }
}

public sealed class InvalidTcgAssetCategoryException : Exception
{
    public InvalidTcgAssetCategoryException(string category)
        : base($"Unsupported TCG category: {category}. Supported categories are Pokemon, Magic, and One Piece.")
    {
    }
}
