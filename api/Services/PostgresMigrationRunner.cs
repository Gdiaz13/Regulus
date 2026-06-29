using Dapper;
using Npgsql;

namespace api.Services;

// Runs plain SQL migrations so PostgreSQL schema changes stay versioned.
public sealed class PostgresMigrationRunner
{
    private const string TableSql = """
        create table if not exists regulas_schema_migrations (
            name text primary key,
            applied_at timestamptz not null default now()
        );
        """;

    private readonly IWebHostEnvironment _environment;
    private readonly PostgresConnectionFactory _factory;
    private readonly ILogger<PostgresMigrationRunner> _logger;

    public PostgresMigrationRunner(
        PostgresConnectionFactory factory,
        IWebHostEnvironment environment,
        ILogger<PostgresMigrationRunner> logger
    )
    {
        _factory = factory;
        _environment = environment;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _factory.OpenConnectionAsync(cancellationToken);
        await EnsureTableAsync(connection, cancellationToken);
        await ApplyPendingAsync(connection, MigrationFiles(), cancellationToken);
    }

    private Task EnsureTableAsync(NpgsqlConnection connection, CancellationToken token)
    {
        return connection.ExecuteAsync(Command(TableSql, token));
    }

    private async Task ApplyPendingAsync(NpgsqlConnection connection, IReadOnlyList<string> files, CancellationToken token)
    {
        var applied = await AppliedAsync(connection, token);
        foreach (var file in files.Where(file => !applied.Contains(Path.GetFileName(file))))
        {
            await ApplyAsync(connection, file, token);
        }
    }

    private static async Task<HashSet<string>> AppliedAsync(NpgsqlConnection connection, CancellationToken token)
    {
        const string sql = "select name from regulas_schema_migrations;";
        var rows = await connection.QueryAsync<string>(Command(sql, token));
        return rows.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private async Task ApplyAsync(NpgsqlConnection connection, string file, CancellationToken token)
    {
        await using var transaction = await connection.BeginTransactionAsync(token);
        await connection.ExecuteAsync(Command(await File.ReadAllTextAsync(file, token), transaction, token));
        await MarkAppliedAsync(connection, Path.GetFileName(file), transaction, token);
        await transaction.CommitAsync(token);
        _logger.LogInformation("Applied PostgreSQL migration {Migration}.", Path.GetFileName(file));
    }

    private static Task MarkAppliedAsync(
        NpgsqlConnection connection,
        string name,
        NpgsqlTransaction transaction,
        CancellationToken token
    )
    {
        const string sql = "insert into regulas_schema_migrations (name) values (@Name);";
        return connection.ExecuteAsync(Command(sql, new { Name = name }, transaction, token));
    }

    private IReadOnlyList<string> MigrationFiles()
    {
        var path = Path.Combine(_environment.ContentRootPath, "Database", "Migrations");
        return Directory.Exists(path) ? Directory.GetFiles(path, "*.sql").Order().ToList() : [];
    }

    private static CommandDefinition Command(string sql, CancellationToken token)
    {
        return new CommandDefinition(sql, cancellationToken: token);
    }

    private static CommandDefinition Command(string sql, NpgsqlTransaction transaction, CancellationToken token)
    {
        return new CommandDefinition(sql, transaction: transaction, cancellationToken: token);
    }

    private static CommandDefinition Command(
        string sql,
        object parameters,
        NpgsqlTransaction transaction,
        CancellationToken token
    )
    {
        return new CommandDefinition(sql, parameters, transaction, cancellationToken: token);
    }
}
