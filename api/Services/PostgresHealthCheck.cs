using System.Data.Common;
using Dapper;

namespace api.Services;

// Probes the PostgreSQL path the Dapper repositories use.
public sealed class PostgresHealthCheck
{
    private static readonly TimeSpan ProbeTimeout = TimeSpan.FromSeconds(2);
    private readonly IDatabaseConnectionFactory _factory;

    public PostgresHealthCheck(IDatabaseConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            return await ProbeAsync();
        }
        catch (Exception exception) when (IsDatabaseException(exception))
        {
            return false;
        }
    }

    private async Task<bool> ProbeAsync()
    {
        using var timeout = new CancellationTokenSource(ProbeTimeout);
        await using var connection = await _factory.OpenDatabaseConnectionAsync(timeout.Token);
        var value = await connection.ExecuteScalarAsync<int>(Command(timeout.Token));
        return value == 1;
    }

    private static CommandDefinition Command(CancellationToken token)
    {
        return new CommandDefinition("select 1;", cancellationToken: token);
    }

    private static bool IsDatabaseException(Exception exception)
    {
        return exception is DbException or InvalidOperationException or TimeoutException or TaskCanceledException;
    }
}
