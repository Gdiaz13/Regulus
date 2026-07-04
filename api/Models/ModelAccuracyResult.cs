namespace api.Models;

// One scored prediction, persisted by the scoring job so accuracy history
// accumulates instead of being recomputed on every request.
public sealed class ModelAccuracyResult
{
    public long Id { get; init; }
    public long PredictionId { get; init; }
    public Guid UserId { get; init; }
    public string AssetId { get; init; } = string.Empty;
    public string AssetType { get; init; } = string.Empty;
    public string ModelName { get; init; } = string.Empty;
    public string ModelVersion { get; init; } = string.Empty;
    public double PredictedPercentChange { get; init; }
    public double ActualPercentChange { get; init; }
    public double AbsolutePercentError { get; init; }
    public bool DirectionMatched { get; init; }
    public decimal ActualPrice { get; init; }
    // date columns; Npgsql reads them back as DateOnly.
    public DateOnly TargetDate { get; init; }
    public DateOnly ActualDate { get; init; }
    public bool IsMock { get; init; }
    public DateTime PredictedOn { get; init; }
    public DateTime ScoredAt { get; init; }
}
