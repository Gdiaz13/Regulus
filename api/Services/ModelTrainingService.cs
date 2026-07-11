namespace api.Services;

// Schedules model training separately from prediction requests. It is disabled
// by default until a real trainer is wired in, but the job slot is ready.
public sealed class ModelTrainingService : RecurringJobService
{
    private readonly IConfiguration _configuration;

    public ModelTrainingService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<ModelTrainingService> logger
    )
        : base(scopeFactory, logger)
    {
        _configuration = configuration;
    }

    protected override string JobName => "model-training";

    protected override bool Enabled => BackgroundJobsConfiguration.ModelTrainingEnabled(_configuration);

    protected override TimeSpan StartupDelay => BackgroundJobsConfiguration.StartupDelay(_configuration);

    protected override TimeSpan Interval => BackgroundJobsConfiguration.ModelTrainingInterval(_configuration);

    protected override Task<JobOutcome> RunAsync(IServiceProvider services, CancellationToken token)
    {
        return Task.FromResult(JobOutcome.Skipped("Model training pipeline is not wired yet."));
    }
}
