namespace api.Contracts;

public sealed record AuthRegisterRequest(string? Email, string? Password, string? DisplayName);

public sealed record AuthLoginRequest(string? Email, string? Password);

public sealed record AuthResponse(string AccessToken, DateTime ExpiresAt, AuthUserResponse User);

public sealed record AuthUserResponse(
    Guid Id,
    string Email,
    string DisplayName,
    DateTime CreatedAt,
    DateTime? LastLoginAt
);
