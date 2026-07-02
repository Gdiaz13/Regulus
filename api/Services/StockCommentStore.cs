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

    public async Task<List<Comment>> ListAsync(Guid userId, int stockId)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        var rows = await connection.QueryAsync<Comment>(Sql.List, new { UserId = userId, StockId = stockId });
        return rows.ToList();
    }

    public async Task<Comment> CreateAsync(Guid userId, Comment comment)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await connection.QuerySingleAsync<Comment>(Sql.Insert, Params(userId, comment));
    }

    public async Task<Comment?> UpdateAsync(Guid userId, int id, string title, string content)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<Comment>(Sql.Update, new { UserId = userId, Id = id, Title = title, Content = content });
    }

    public async Task<bool> DeleteAsync(Guid userId, int id)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await connection.ExecuteAsync(Sql.Delete, new { UserId = userId, Id = id }) > 0;
    }

    public async Task<bool> StockExistsAsync(Guid userId, int stockId)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await connection.ExecuteScalarAsync<bool>(Sql.StockExists, new { UserId = userId, StockId = stockId });
    }

    private static object Params(Guid userId, Comment comment)
    {
        return new
        {
            UserId = userId,
            comment.Title,
            comment.Content,
            comment.CreatedOn,
            comment.StockId,
        };
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
            where user_id = @UserId and stock_id = @StockId
            order by created_on desc;
            """;

        public const string Insert = $"""
            insert into comments (user_id, title, content, created_on, stock_id)
            values (@UserId, @Title, @Content, @CreatedOn, @StockId)
            returning {Columns};
            """;

        public const string Update = $"""
            update comments
            set title = @Title, content = @Content
            where user_id = @UserId and id = @Id
            returning {Columns};
            """;

        public const string Delete = "delete from comments where user_id = @UserId and id = @Id;";

        public const string StockExists = """
            select exists(select 1 from stocks where user_id = @UserId and id = @StockId);
            """;
    }
}
