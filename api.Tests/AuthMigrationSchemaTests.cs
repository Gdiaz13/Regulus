using Xunit;

namespace api.Tests;

// Keeps auth schema changes explicit before Dapper auth stores depend on them.
public class AuthMigrationSchemaTests
{
    [Fact]
    public void Auth_migration_creates_users_foundation()
    {
        var sql = ReadMigration("002_auth_user_tables.sql");
        Assert.Contains("create table if not exists users", sql);
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
        Assert.Contains("user_id integer not null references users (id) on delete cascade", sql);
        Assert.Contains("token_hash text not null", sql);
        Assert.Contains("revoked_at timestamptz null", sql);
        Assert.Contains("create unique index if not exists ux_refresh_tokens_token_hash", sql);
    }

    private static string ReadMigration(string name)
    {
        var path = Path.Combine(ProjectRoot(), "api", "Database", "Migrations", name);
        Assert.True(File.Exists(path), $"Missing migration {name}.");
        return File.ReadAllText(path).ToLowerInvariant();
    }

    private static string ProjectRoot()
    {
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
    }
}
