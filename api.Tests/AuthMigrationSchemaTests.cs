using System.Runtime.CompilerServices;
using Xunit;

namespace api.Tests;

// Keeps auth schema changes explicit before Dapper auth stores depend on them.
public class AuthMigrationSchemaTests
{
    [Fact]
    public void Auth_migration_creates_users_foundation()
    {
        var sql = ReadMigration("002_auth_user_tables.sql");
        Assert.Contains("create extension if not exists pgcrypto", sql);
        Assert.Contains("create table if not exists users", sql);
        Assert.Contains("id uuid primary key default gen_random_uuid()", sql);
        Assert.Contains("normalized_email varchar(256) not null", sql);
        Assert.Contains("password_hash text not null", sql);
        Assert.Contains("is_active boolean not null default true", sql);
        Assert.Contains("create unique index if not exists ux_users_normalized_email", sql);
    }

    [Fact]
    public void Auth_migration_creates_refresh_tokens_for_device_logout()
    {
        var sql = ReadMigration("002_auth_user_tables.sql");
        Assert.Contains("create table if not exists refresh_tokens", sql);
        Assert.Contains("id uuid primary key default gen_random_uuid()", sql);
        Assert.Contains("user_id uuid not null references users (id) on delete cascade", sql);
        Assert.Contains("token_hash text not null", sql);
        Assert.Contains("revoked_at timestamptz null", sql);
        Assert.Contains("create unique index if not exists ux_refresh_tokens_token_hash", sql);
    }

    [Fact]
    public void Auth_migration_creates_user_settings_for_uuid_users()
    {
        var sql = ReadMigration("002_auth_user_tables.sql");
        Assert.Contains("create table if not exists user_settings", sql);
        Assert.Contains("user_id uuid primary key references users (id) on delete cascade", sql);
        Assert.Contains("settings_json jsonb not null default '{}'::jsonb", sql);
    }

    [Fact]
    public void Ownership_migration_attaches_portfolio_rows_to_users()
    {
        var sql = ReadMigration("003_user_owned_data.sql");
        Assert.Contains("legacy@local.regulas", sql);
        Assert.Contains("alter table stocks add column if not exists user_id uuid", sql);
        Assert.Contains("create unique index if not exists ux_stocks_user_symbol", sql);
        Assert.Contains("on stocks (user_id, symbol)", sql);
    }

    [Fact]
    public void Ownership_migration_attaches_notes_and_predictions_to_users()
    {
        var sql = ReadMigration("003_user_owned_data.sql");
        Assert.Contains("alter table comments add column if not exists user_id uuid", sql);
        Assert.Contains("alter table predictions add column if not exists user_id uuid", sql);
        Assert.Contains("create index if not exists ix_comments_user_id", sql);
        Assert.Contains("create index if not exists ix_predictions_user_asset_created_on", sql);
    }

    [Fact]
    public void Model_accuracy_signal_migration_preserves_prediction_inputs()
    {
        var sql = ReadMigration("007_model_accuracy_signal_fields.sql");
        Assert.Contains("confidence_score double precision", sql);
        Assert.Contains("risk_score double precision", sql);
        Assert.Contains("bullish_score double precision", sql);
        Assert.Contains("bearish_score double precision", sql);
        Assert.Contains("time_horizon_days integer", sql);
    }

    private static string ReadMigration(string name, [CallerFilePath] string sourceFile = "")
    {
        var path = Path.Combine(ProjectRoot(sourceFile), "api", "Database", "Migrations", name);
        Assert.True(File.Exists(path), $"Missing migration {name}.");
        return File.ReadAllText(path).ToLowerInvariant();
    }

    private static string ProjectRoot(string sourceFile)
    {
        return FindProjectRoot(Path.GetDirectoryName(sourceFile) ?? string.Empty)
            ?? throw new DirectoryNotFoundException("Could not locate Exchange.sln.");
    }

    private static string? FindProjectRoot(string start)
    {
        var directory = new DirectoryInfo(start);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Exchange.sln")))
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }
        return null;
    }
}
