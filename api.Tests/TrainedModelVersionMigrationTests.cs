using System.Runtime.CompilerServices;
using Xunit;

namespace api.Tests;

public class TrainedModelVersionMigrationTests
{
    [Fact]
    public void Migration_defines_the_artifact_ledger_and_lookup_indexes()
    {
        var sql = ReadMigration("008_trained_model_versions.sql");

        Assert.Contains("create table if not exists trained_model_versions", sql);
        Assert.Contains("artifact_json text not null", sql);
        Assert.Contains("metrics_json text not null", sql);
        Assert.Contains("promotion_eligible boolean not null", sql);
        Assert.Contains("series_count > 0", sql);
        Assert.Contains("octet_length(artifact_json) between 2 and 262144", sql);
        Assert.Contains("octet_length(metrics_json) between 2 and 65536", sql);
        Assert.Contains("ix_trained_model_versions_model", sql);
    }

    private static string ReadMigration(string name, [CallerFilePath] string sourceFile = "")
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFile)!, ".."));
        var path = Path.Combine(root, "api", "Database", "Migrations", name);
        Assert.True(File.Exists(path), $"Missing migration {name}.");
        return File.ReadAllText(path).ToLowerInvariant();
    }
}
