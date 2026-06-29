namespace api.Models;

public sealed class AuthSession
{
    public string TokenHash { get; init; } = string.Empty;
    public Guid UserId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime ExpiresAt { get; init; }
    public DateTime? RevokedAt { get; init; }
}
