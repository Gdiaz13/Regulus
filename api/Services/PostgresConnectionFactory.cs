using System.Data.Common;
using Npgsql;

namespace api.Services;

// Opens PostgreSQL connections for Dapper repositories and migration runners.
public sealed class PostgresConnectionFactory : IDatabaseConnectionFactory
{
    private readonly string _connectionString;

    public PostgresConnectionFactory(IConfiguration configuration)
    {
        _connectionString = ConnectionStringFrom(configuration);
    }

    public string ConnectionString => _connectionString;

    public async Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    public async Task<DbConnection> OpenDatabaseConnectionAsync(CancellationToken cancellationToken = default)
    {
        return await OpenConnectionAsync(cancellationToken);
    }

    private static string ConnectionStringFrom(IConfiguration configuration)
    {
        return FromEnvironment()
            ?? FromEnvironmentParts()
            ?? FromConfig(configuration)
            ?? throw new InvalidOperationException("PostgreSQL connection string is not configured.");
    }

    private static string? FromEnvironment()
    {
        return Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
    }

    private static string? FromEnvironmentParts()
    {
        var host = Environment.GetEnvironmentVariable("POSTGRES_HOST");
        if (string.IsNullOrWhiteSpace(host))
        {
            return null;
        }
        return BuildFromParts(host);
    }

    private static string BuildFromParts(string host)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = PortFromEnvironment(),
            Database = EnvOrDefault("POSTGRES_DB", "regulas"),
            Username = EnvOrDefault("POSTGRES_USER", "regulas"),
            Password = EnvOrDefault("POSTGRES_PASSWORD", "regulas"),
        };
        return builder.ConnectionString;
    }

    private static int PortFromEnvironment()
    {
        var value = Environment.GetEnvironmentVariable("POSTGRES_PORT");
        return int.TryParse(value, out var port) ? port : 5432;
    }

    private static string EnvOrDefault(string name, string fallback)
    {
        return Environment.GetEnvironmentVariable(name) ?? fallback;
    }

    private static string? FromConfig(IConfiguration configuration)
    {
        return configuration.GetConnectionString("DefaultConnection");
    }
}
