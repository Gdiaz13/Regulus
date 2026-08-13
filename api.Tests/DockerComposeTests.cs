using System.Text.RegularExpressions;
using Xunit;

namespace api.Tests;

public class DockerComposeTests
{
    private static readonly Regex PostgresService = new(
        @"(?ms)^  postgres:\r?\n(?<body>.*?)(?=^(?:\S|  \S)|\z)");
    private static readonly Regex ServiceVolumes = new(
        @"(?m)^    volumes:\r?\n(?<body>(?:^      - [^\r\n]*(?:\r?\n|\z))+)");

    [Fact]
    public void PostgreSql_18_uses_the_major_version_compatible_volume_root()
    {
        var compose = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "docker-compose.yml"));

        Assert.Equal(new[] { "regulas_postgres_data:/var/lib/postgresql" }, PostgresVolumes(compose));
    }

    [Fact]
    public void PostgreSql_volume_parser_stops_before_a_later_service_list()
    {
        Assert.Equal(
            new[] { "regulas_postgres_data:/var/lib/postgresql" },
            PostgresVolumes(ComposeWithLaterList));
    }

    private static List<string> PostgresVolumes(string compose)
    {
        var service = RequireMatch(PostgresService, compose).Groups["body"].Value;
        var volumeBlock = RequireMatch(ServiceVolumes, service).Groups["body"].Value;
        return volumeBlock.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => line.StartsWith("- "))
            .Select(line => line[2..])
            .ToList();
    }

    private static Match RequireMatch(Regex regex, string value)
    {
        var match = regex.Match(value);
        Assert.True(match.Success);
        return match;
    }

    private const string ComposeWithLaterList = """
        services:
          postgres:
            volumes:
              - regulas_postgres_data:/var/lib/postgresql
            depends_on:
              - cache
          cache:
            image: redis:alpine
        """;
}
