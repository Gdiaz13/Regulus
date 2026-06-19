using System.Data.Common;
using api.Data;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public static class DatabaseStartup
{
    public static async Task InitializeAsync(WebApplication app)
    {
        if (!ShouldMigrate(app))
        {
            return;
        }
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseStartup");
        await TryMigrateAsync(db, logger);
    }

    private static bool ShouldMigrate(WebApplication app)
    {
        return app.Environment.IsDevelopment() && app.Configuration.GetValue("Database:AutoMigrate", true);
    }

    private static async Task TryMigrateAsync(ApplicationDBContext db, ILogger logger)
    {
        try
        {
            await db.Database.MigrateAsync();
        }
        catch (Exception exception) when (IsDatabaseStartupException(exception))
        {
            LogMigrationSkipped(logger);
        }
    }

    private static bool IsDatabaseStartupException(Exception exception)
    {
        return exception is DbException or DbUpdateException or InvalidOperationException;
    }

    private static void LogMigrationSkipped(ILogger logger)
    {
        try
        {
            logger.LogWarning("Database migration skipped because the database is unavailable.");
        }
        catch (Exception)
        {
        }
    }
}
