using System.Runtime.CompilerServices;
using Xunit;

namespace api.Tests;

public class UserAdminFlagMigrationTests
{
    [Fact]
    public void Migration_adds_the_admin_flag_defaulting_to_ordinary_user()
    {
        var sql = ReadMigration("009_user_admin_flag.sql");

        Assert.Contains("add column if not exists is_admin boolean not null default false", sql);
        Assert.Contains("ix_users_is_admin", sql);
    }

    // Two migrations once shared the number 008 after a parallel branch landed.
    // The runner sorts and tracks by filename so it survived, but the numbering
    // is the ordering contract, so duplicates are worth failing on.
    [Fact]
    public void Every_migration_number_is_used_once()
    {
        var numbers = MigrationFiles().Select(file => Path.GetFileName(file)[..3]).ToList();

        Assert.Equal(numbers.Count, numbers.Distinct().Count());
    }

    [Fact]
    public void Migrations_run_in_the_order_they_are_numbered()
    {
        var names = MigrationFiles().Select(Path.GetFileName).ToList();

        Assert.Equal(names.OrderBy(name => name, StringComparer.Ordinal), names);
    }

    private static List<string> MigrationFiles([CallerFilePath] string sourceFile = "")
    {
        return [.. Directory.GetFiles(MigrationsPath(sourceFile), "*.sql").Order()];
    }

    private static string MigrationsPath(string sourceFile)
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFile)!, ".."));
        return Path.Combine(root, "api", "Database", "Migrations");
    }

    private static string ReadMigration(string name, [CallerFilePath] string sourceFile = "")
    {
        var path = Path.Combine(MigrationsPath(sourceFile), name);
        Assert.True(File.Exists(path), $"Missing migration {name}.");
        return File.ReadAllText(path).ToLowerInvariant();
    }
}
