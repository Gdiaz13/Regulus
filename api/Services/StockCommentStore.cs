using api.Models;
using Dapper;

namespace api.Services;

// Dapper-backed notes for portfolio stocks. Notes stay tied to stocks in
// PostgreSQL with cascade deletes from the SQL migration.
public sealed class StockCommentStore
{
    private readonly IDatabaseConnectionFactory _factory;

    public StockCommentStore(IDatabaseConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<List<Comment>> ListAsync(int stockId)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        var rows = await connection.QueryAsync<Comment>(Sql.List, new { StockId = stockId });
        return rows.ToList();
    }

    public async Task<Comment> CreateAsync(Comment comment)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await connection.QuerySingleAsync<Comment>(Sql.Insert, comment);
    }

    public async Task<Comment?> UpdateAsync(int id, string title, string content)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<Comment>(Sql.Update, new { Id = id, Title = title, Content = content });
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await connection.ExecuteAsync(Sql.Delete, new { Id = id }) > 0;
    }

    public async Task<bool> StockExistsAsync(int stockId)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await connection.ExecuteScalarAsync<bool>(Sql.StockExists, new { StockId = stockId });
    }

    private static class Sql
    {
        private const string Columns = """
            id as "Id", title as "Title", content as "Content",
            created_on as "CreatedOn", stock_id as "StockId"
            """;

        public const string List = $"""
            select {Columns}
            from comments
            where stock_id = @StockId
            order by created_on desc;
            """;

        public const string Insert = $"""
            insert into comments (title, content, created_on, stock_id)
            values (@Title, @Content, @CreatedOn, @StockId)
            returning {Columns};
            """;

        public const string Update = $"""
            update comments
            set title = @Title, content = @Content
            where id = @Id
            returning {Columns};
            """;

        public const string Delete = "delete from comments where id = @Id;";

        public const string StockExists = """
            select exists(select 1 from stocks where id = @StockId);
            """;
    }
}
