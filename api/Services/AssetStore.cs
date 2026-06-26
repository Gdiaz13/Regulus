using System.Data.Common;
using api.Contracts;
using api.Models;
using Dapper;

namespace api.Services;

// Generic assets live here so stocks, cards, ETFs, and crypto share one
// Dapper/PostgreSQL path.
public sealed class AssetStore
{
    private readonly IDatabaseConnectionFactory _factory;

    public AssetStore(IDatabaseConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<List<AssetResponse>> ListAsync(AssetType? type)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        var rows = await connection.QueryAsync<AssetRow>(Sql.List, new { AssetType = type?.ToString() });
        return rows.Select(ToResponse).ToList();
    }

    public async Task<AssetResponse?> FindAsync(int id)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await FindAsync(connection, id);
    }

    public async Task<AssetCreateResult> CreateAsync(AssetCommand command)
    {
        var clean = Normalize(command);
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        if (await Exists(connection, clean))
        {
            return Duplicate(clean);
        }
        var categoryId = await CategoryId(connection, clean);
        var id = await InsertAsset(connection, clean, categoryId);
        return Created((await FindAsync(connection, id))!);
    }

    private static async Task<AssetResponse?> FindAsync(DbConnection connection, int id)
    {
        var row = await connection.QuerySingleOrDefaultAsync<AssetRow>(Sql.Find, new { Id = id });
        return row is null ? null : ToResponse(row);
    }

    private static Task<bool> Exists(DbConnection connection, AssetCommand command)
    {
        return connection.ExecuteScalarAsync<bool>(Sql.Exists, Params(command));
    }

    private static async Task<int?> CategoryId(DbConnection connection, AssetCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Category))
        {
            return null;
        }
        return await FindCategoryId(connection, command) ?? await InsertCategory(connection, command);
    }

    private static Task<int?> FindCategoryId(DbConnection connection, AssetCommand command)
    {
        return connection.ExecuteScalarAsync<int?>(Sql.FindCategoryId, CategoryParams(command));
    }

    private static Task<int> InsertCategory(DbConnection connection, AssetCommand command)
    {
        return connection.ExecuteScalarAsync<int>(Sql.InsertCategory, CategoryParams(command));
    }

    private static Task<int> InsertAsset(DbConnection connection, AssetCommand command, int? categoryId)
    {
        return connection.ExecuteScalarAsync<int>(Sql.InsertAsset, Params(command, categoryId));
    }

    private static AssetCreateResult Created(AssetResponse asset)
    {
        return new AssetCreateResult(asset, null, false);
    }

    private static AssetCreateResult Duplicate(AssetCommand command)
    {
        return new AssetCreateResult(null, $"{command.Symbol} is already tracked as {command.AssetType}.", true);
    }

    private static AssetCommand Normalize(AssetCommand command)
    {
        var symbol = command.Symbol.Trim().ToUpperInvariant();
        var name = string.IsNullOrWhiteSpace(command.Name) ? symbol : command.Name.Trim();
        return new AssetCommand(symbol, name, command.AssetType, Clean(command.Category));
    }

    private static object Params(AssetCommand command, int? categoryId = null)
    {
        return new { command.Symbol, command.Name, AssetType = command.AssetType.ToString(), CategoryId = categoryId };
    }

    private static object CategoryParams(AssetCommand command)
    {
        return new { Name = Clean(command.Category), Slug = Slug(command.Category!), AssetType = command.AssetType.ToString() };
    }

    private static AssetResponse ToResponse(AssetRow row)
    {
        return new AssetResponse(row.Id, row.Symbol, row.Name, row.AssetType, row.Category, row.CreatedOn);
    }

    private static string Slug(string value)
    {
        return Clean(value).ToLowerInvariant().Replace(' ', '-');
    }

    private static string Clean(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }

    private sealed class AssetRow
    {
        public int Id { get; init; }
        public string Symbol { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string AssetType { get; init; } = string.Empty;
        public string? Category { get; init; }
        public DateTime CreatedOn { get; init; }
    }

    private static class Sql
    {
        private const string Columns = """
            a.id as "Id", a.symbol as "Symbol", a.name as "Name",
            a.asset_type as "AssetType", c.name as "Category",
            a.created_on as "CreatedOn"
            """;

        public const string List = $"""
            select {Columns}
            from assets a
            left join asset_categories c on c.id = a.category_id
            where @AssetType is null or a.asset_type = @AssetType
            order by a.asset_type, a.symbol;
            """;

        public const string Find = $"""
            select {Columns}
            from assets a
            left join asset_categories c on c.id = a.category_id
            where a.id = @Id;
            """;

        public const string Exists = """
            select exists(select 1 from assets where symbol = @Symbol and asset_type = @AssetType);
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
            returning id;
            """;
    }
}
