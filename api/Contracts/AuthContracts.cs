namespace api.Contracts;

public sealed record RegisterRequest(string? Email, string? Password, string? DisplayName);

public sealed record LoginRequest(string? Email, string? Password);

public sealed record AuthResponse(string Token, DateTime ExpiresAt, CurrentUserResponse User);

public sealed record CurrentUserResponse(
    Guid Id,
    string Email,
    string DisplayName,
    DateTime CreatedAt,
    DateTime? LastLoginAt
);
