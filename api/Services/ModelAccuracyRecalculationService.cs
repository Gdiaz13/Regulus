namespace api.Services;

// Re-scores stored accuracy rows after price history fills in with better target
// dates, keeping model evaluation out of normal user requests.
public sealed class ModelAccuracyRecalculationService : RecurringJobService
{
    private readonly IConfiguration _configuration;

    public ModelAccuracyRecalculationService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<ModelAccuracyRecalculationService> logger
    )
        : base(scopeFactory, logger)
    {
        _configuration = configuration;
    }

    protected override string JobName => "model-accuracy-recalculation";

    protected override bool Enabled => BackgroundJobsConfiguration.ModelAccuracyRecalculationEnabled(_configuration);

    protected override TimeSpan StartupDelay => BackgroundJobsConfiguration.StartupDelay(_configuration);

    protected override TimeSpan Interval => BackgroundJobsConfiguration.ModelAccuracyRecalculationInterval(_configuration);

    protected override async Task<JobOutcome> RunAsync(IServiceProvider services, CancellationToken token)
    {
        var refreshed = await services.GetRequiredService<ModelAccuracyResultStore>().RecalculateExistingAsync(token);
        return JobOutcome.Completed($"Recalculated {refreshed} accuracy result(s).", refreshed);
    }
}
