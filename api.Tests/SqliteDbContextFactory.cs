using api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace api.Tests;

// A throwaway ApplicationDBContext backed by a kept-open in-memory SQLite
// connection, so SaveAsync runs against a real relational schema (built from the
// model via EnsureCreated) instead of a fake store.
internal sealed class SqliteDbContextFactory : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteDbContextFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        using var context = Create();
        context.Database.EnsureCreated();
    }

    public ApplicationDBContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseSqlite(_connection)
            .Options;
        return new ApplicationDBContext(options);
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}
