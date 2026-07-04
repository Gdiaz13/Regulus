namespace api.Services;

// Periodically scores matured predictions against stored prices and persists
// the results, so Regulas accumulates real accuracy history per model.
public sealed class PredictionScoringService : RecurringJobService
{
    private readonly IConfiguration _configuration;

    public PredictionScoringService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<PredictionScoringService> logger
    )
        : base(scopeFactory, logger)
    {
        _configuration = configuration;
    }

    protected override string JobName => "prediction-scoring";

    protected override bool Enabled => BackgroundJobsConfiguration.PredictionScoringEnabled(_configuration);

    protected override TimeSpan StartupDelay => BackgroundJobsConfiguration.StartupDelay(_configuration);

    protected override TimeSpan Interval => BackgroundJobsConfiguration.PredictionScoringInterval(_configuration);

    protected override async Task<JobOutcome> RunAsync(IServiceProvider services, CancellationToken token)
    {
        var scored = await services.GetRequiredService<ModelAccuracyResultStore>().ScorePendingAsync(token);
        return JobOutcome.Completed($"Scored {scored} matured prediction(s).", scored);
    }
}
