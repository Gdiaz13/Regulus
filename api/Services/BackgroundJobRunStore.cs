using api.Models;
using Dapper;

namespace api.Services;

// Records background job executions through Dapper so runs can be audited later.
public sealed class BackgroundJobRunStore
{
    private readonly IDatabaseConnectionFactory _factory;

    public BackgroundJobRunStore(IDatabaseConnectionFactory factory)
    {
        _factory = factory;
    }

    // Opens a run as "running" and returns its id so FinishAsync can close it.
    public async Task<long> StartAsync(string jobName, CancellationToken token = default)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync(token);
        return await connection.ExecuteScalarAsync<long>(Sql.Start, new { JobName = jobName, StartedAt = DateTime.UtcNow });
    }

    public async Task FinishAsync(long id, string status, string? detail, int itemsProcessed, CancellationToken token = default)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync(token);
        await connection.ExecuteAsync(Sql.Finish, FinishParams(id, status, detail, itemsProcessed));
    }

    public async Task<List<BackgroundJobRun>> ListRecentAsync(int take, CancellationToken token = default)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync(token);
        var rows = await connection.QueryAsync<BackgroundJobRun>(Sql.ListRecent, new { Take = take });
        return rows.ToList();
    }

    private static object FinishParams(long id, string status, string? detail, int itemsProcessed)
    {
        return new { Id = id, Status = status, Detail = detail, ItemsProcessed = itemsProcessed, FinishedAt = DateTime.UtcNow };
    }

    // Timestamps are passed from C# (not now()) so the same SQL runs on PostgreSQL and the SQLite test db.
    private static class Sql
    {
        public const string Start = """
            insert into background_job_runs (job_name, status, started_at)
            values (@JobName, 'running', @StartedAt)
            returning id;
            """;

        public const string Finish = """
            update background_job_runs
            set status = @Status, detail = @Detail, items_processed = @ItemsProcessed, finished_at = @FinishedAt
            where id = @Id;
            """;

        public const string ListRecent = """
            select id as "Id", job_name as "JobName", status as "Status", detail as "Detail",
                   items_processed as "ItemsProcessed", started_at as "StartedAt", finished_at as "FinishedAt"
            from background_job_runs
            order by started_at desc, id desc
            limit @Take;
            """;
    }
}
