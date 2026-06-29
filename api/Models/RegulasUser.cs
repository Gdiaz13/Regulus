namespace api.Models;

public sealed class RegulasUser
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string NormalizedEmail { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string PasswordHash { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public bool IsActive { get; init; } = true;
}
