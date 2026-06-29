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
