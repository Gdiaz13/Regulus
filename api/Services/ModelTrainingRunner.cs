using api.Contracts;

namespace api.Services;

// One bounded training run. Scheduling and run-history recording stay in the
// hosted service; this class owns the testable data-to-trainer behavior.
public sealed class ModelTrainingRunner
{
    private const string Category = "Technology";
    private const int MaxSeries = 50;
    private const int PointsPerSeries = 1000;
    private readonly ModelTrainingDataStore _data;
    private readonly StockTechAiClient _client;

    public ModelTrainingRunner(ModelTrainingDataStore data, StockTechAiClient client)
    {
        _data = data;
        _client = client;
    }

    public async Task<JobOutcome> RunAsync(CancellationToken token)
    {
        var series = await _data.ListSeriesAsync(Category, MaxSeries, PointsPerSeries, token);
        if (series.Count == 0)
        {
            return JobOutcome.Skipped("No stored Technology price series are ready for training.");
        }
        var response = await _client.TrainAsync(new AiTrainRequest(series), token);
        return Outcome(response, series.Count);
    }

    private static JobOutcome Outcome(AiTrainResponse response, int seriesCount)
    {
        var detail = $"{response.ModelName} {response.ModelVersion}: {response.Message}";
        return response.Trained && !response.IsMock
            ? JobOutcome.Completed(detail, seriesCount)
            : JobOutcome.Skipped(detail);
    }
}
