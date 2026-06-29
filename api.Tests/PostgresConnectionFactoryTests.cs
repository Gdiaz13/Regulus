using api.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace api.Tests;

public class PostgresConnectionFactoryTests
{
    [Fact]
    public void Uses_default_connection_from_config()
    {
        WithoutPostgresEnvironment(AssertConfigConnection);
    }

    [Fact]
    public void Environment_connection_string_wins()
    {
        WithEnvironment("DATABASE_CONNECTION_STRING", "Host=env-db", AssertEnvironmentOverride);
    }

    [Fact]
    public void Builds_from_postgres_environment_parts()
    {
        WithPostgresParts(AssertPostgresParts);
    }

    private static void AssertEnvironmentOverride()
    {
        var factory = new PostgresConnectionFactory(Config("Host=config-db"));
        Assert.Equal("Host=env-db", factory.ConnectionString);
    }

    private static void AssertConfigConnection()
    {
        var factory = new PostgresConnectionFactory(Config("Host=config-db"));
        Assert.Equal("Host=config-db", factory.ConnectionString);
    }

    private static void AssertPostgresParts()
    {
        var factory = new PostgresConnectionFactory(Config("Host=config-db"));
        Assert.Contains("Host=parts-db", factory.ConnectionString);
        Assert.Contains("Port=5544", factory.ConnectionString);
        Assert.Contains("Database=regulas_test", factory.ConnectionString);
    }

    private static void WithPostgresParts(Action assert)
    {
        WithEnvironment("DATABASE_CONNECTION_STRING", null, () =>
            WithEnvironment("POSTGRES_HOST", "parts-db", () =>
                WithEnvironment("POSTGRES_PORT", "5544", () =>
                    WithEnvironment("POSTGRES_DB", "regulas_test", assert))));
    }

    private static void WithoutPostgresEnvironment(Action assert)
    {
        WithEnvironment("DATABASE_CONNECTION_STRING", null, () => WithEnvironment("POSTGRES_HOST", null, assert));
    }

    private static void WithEnvironment(string name, string? value, Action assert)
    {
        var previous = Environment.GetEnvironmentVariable(name);
        try
        {
            Environment.SetEnvironmentVariable(name, value);
            assert();
        }
        finally
        {
            Environment.SetEnvironmentVariable(name, previous);
        }
    }

    private static IConfiguration Config(string connectionString)
    {
        var values = new Dictionary<string, string?> { ["ConnectionStrings:DefaultConnection"] = connectionString };
        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }
}
