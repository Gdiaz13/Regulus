namespace Regulas.MauiApp.Models;

public sealed record CurrentUser(
    Guid Id,
    string Email,
    string DisplayName,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    bool IsAdmin = false
) : IAdminAware;

public sealed record AuthResponse(string Token, DateTime ExpiresAt, CurrentUser User);

public sealed record LoginRequest(string Email, string Password);

public sealed record RegisterRequest(string Email, string Password, string DisplayName);
