using api.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace api.Tests;

public class BackgroundJobsConfigurationTests
{
    [Fact]
    public void Model_training_job_defaults_to_disabled_and_daily()
    {
        var config = Config([]);
        Assert.False(BackgroundJobsConfiguration.ModelTrainingEnabled(config));
        Assert.Equal(TimeSpan.FromDays(1), BackgroundJobsConfiguration.ModelTrainingInterval(config));
    }

    [Fact]
    public void Model_training_job_can_be_enabled_and_tuned()
    {
        var config = Config(new Dictionary<string, string?>
        {
            ["BackgroundJobs:ModelTrainingEnabled"] = "true",
            ["BackgroundJobs:ModelTrainingIntervalMinutes"] = "60",
        });
        Assert.True(BackgroundJobsConfiguration.ModelTrainingEnabled(config));
        Assert.Equal(TimeSpan.FromHours(1), BackgroundJobsConfiguration.ModelTrainingInterval(config));
    }

    [Fact]
    public void Model_training_interval_falls_back_when_configured_with_non_positive_minutes()
    {
        var config = Config(new Dictionary<string, string?> { ["BackgroundJobs:ModelTrainingIntervalMinutes"] = "0" });
        Assert.Equal(TimeSpan.FromDays(1), BackgroundJobsConfiguration.ModelTrainingInterval(config));
    }

    private static IConfiguration Config(Dictionary<string, string?> values)
    {
        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }
}
