namespace api.Services;

// Reads background-job settings so jobs can be tuned or turned off without code changes.
public static class BackgroundJobsConfiguration
{
    public static bool PriceSnapshotEnabled(IConfiguration configuration)
    {
        return configuration.GetValue("BackgroundJobs:PriceSnapshotEnabled", true);
    }

    public static TimeSpan PriceSnapshotInterval(IConfiguration configuration)
    {
        return TimeSpan.FromMinutes(configuration.GetValue("BackgroundJobs:PriceSnapshotIntervalMinutes", 720));
    }

    public static bool PredictionScoringEnabled(IConfiguration configuration)
    {
        return configuration.GetValue("BackgroundJobs:PredictionScoringEnabled", true);
    }

    // Daily by default: predictions mature over days, not minutes.
    public static TimeSpan PredictionScoringInterval(IConfiguration configuration)
    {
        return TimeSpan.FromMinutes(configuration.GetValue("BackgroundJobs:PredictionScoringIntervalMinutes", 1440));
    }

    public static bool ModelAccuracyRecalculationEnabled(IConfiguration configuration)
    {
        return configuration.GetValue("BackgroundJobs:ModelAccuracyRecalculationEnabled", true);
    }

    public static TimeSpan ModelAccuracyRecalculationInterval(IConfiguration configuration)
    {
        return TimeSpan.FromMinutes(configuration.GetValue("BackgroundJobs:ModelAccuracyRecalculationIntervalMinutes", 1440));
    }

    public static TimeSpan StartupDelay(IConfiguration configuration)
    {
        return TimeSpan.FromSeconds(configuration.GetValue("BackgroundJobs:StartupDelaySeconds", 15));
    }
}
