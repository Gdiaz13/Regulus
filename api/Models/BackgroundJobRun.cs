namespace api.Models;

// One execution of a background job, saved so the app can show what ran and when.
// A class (not a positional record) so Dapper maps columns with type conversion,
// the same way it materializes the other stored models.
public sealed class BackgroundJobRun
{
    public long Id { get; init; }
    public string JobName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? Detail { get; init; }
    public int ItemsProcessed { get; init; }
    public DateTime StartedAt { get; init; }
    public DateTime? FinishedAt { get; init; }
}
