namespace api.Models;

public sealed class TrainedModelVersion
{
    public long Id { get; init; }
    public string ModelName { get; init; } = string.Empty;
    public string ModelVersion { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string ContractVersion { get; init; } = string.Empty;
    public int SeriesCount { get; init; }
    public string ArtifactJson { get; init; } = string.Empty;
    public string MetricsJson { get; init; } = string.Empty;
    public bool PromotionEligible { get; init; }
    public DateTime CreatedAt { get; init; }
}
