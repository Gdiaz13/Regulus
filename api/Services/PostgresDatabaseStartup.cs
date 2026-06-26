using System.Data.Common;

namespace api.Services;

public static class PostgresDatabaseStartup
{
    public static async Task InitializeAsync(WebApplication app)
    {
        if (!ShouldMigrate(app))
        {
            return;
        }
        using var scope = app.Services.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<PostgresMigrationRunner>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("PostgresDatabaseStartup");
        await TryMigrateAsync(runner, logger);
    }

    private static bool ShouldMigrate(WebApplication app)
    {
        return app.Configuration.GetValue("PostgreSql:AutoMigrate", true);
    }

    private static async Task TryMigrateAsync(PostgresMigrationRunner runner, ILogger logger)
    {
        try
        {
            await runner.RunAsync();
        }
        catch (Exception exception) when (IsStartupException(exception))
        {
            logger.LogWarning("PostgreSQL migrations skipped because the database is unavailable.");
        }
    }

    private static bool IsStartupException(Exception exception)
    {
        return exception is DbException or IOException or InvalidOperationException;
    }
}
