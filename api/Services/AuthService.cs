using api.Contracts;
using api.Models;
using Microsoft.AspNetCore.Identity;

namespace api.Services;

public sealed class AuthService
{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromDays(14);
    private readonly AuthStore _store;
    private readonly IPasswordHasher<RegulasUser> _hasher;

    public AuthService(AuthStore store, IPasswordHasher<RegulasUser> hasher)
    {
        _store = store;
        _hasher = hasher;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        var user = NewUser(request);
        if (await _store.FindByNormalizedEmailAsync(user.NormalizedEmail) is not null)
        {
            return AuthResult.EmailTaken();
        }
        var saved = await _store.CreateUserAsync(user);
        return AuthResult.Ok(await IssueTokenAsync(saved));
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        var user = await _store.FindByNormalizedEmailAsync(NormalizeEmail(request.Email));
        if (user is null || !user.IsActive || !await PasswordMatchesAsync(user, request.Password ?? string.Empty))
        {
            return AuthResult.InvalidLogin();
        }
        await _store.MarkLoginAsync(user.Id);
        return AuthResult.Ok(await IssueTokenAsync(user));
    }

    public async Task<CurrentUserResponse?> CurrentUserAsync(Guid userId)
    {
        var user = await _store.FindByIdAsync(userId);
        return user is null || !user.IsActive ? null : ToUserResponse(user);
    }

    public Task<bool> LogoutAsync(string tokenHash)
    {
        return string.IsNullOrWhiteSpace(tokenHash) ? Task.FromResult(false) : _store.RevokeRefreshTokenAsync(tokenHash);
    }

    public static string NormalizeEmail(string? email)
    {
        return email?.Trim().ToUpperInvariant() ?? string.Empty;
    }

    private RegulasUser NewUser(RegisterRequest request)
    {
        var user = BareUser(request);
        user.PasswordHash = _hasher.HashPassword(user, request.Password ?? string.Empty);
        return user;
    }

    private static RegulasUser BareUser(RegisterRequest request)
    {
        return new RegulasUser
        {
            Id = Guid.NewGuid(),
            Email = request.Email?.Trim() ?? string.Empty,
            NormalizedEmail = NormalizeEmail(request.Email),
            DisplayName = request.DisplayName?.Trim() ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
        };
    }

    private async Task<AuthResponse> IssueTokenAsync(RegulasUser user)
    {
        var token = AuthTokenService.NewToken();
        var expiresAt = DateTime.UtcNow.Add(TokenLifetime);
        await _store.CreateRefreshTokenAsync(user.Id, AuthTokenService.Hash(token), expiresAt);
        return new AuthResponse(token, expiresAt, ToUserResponse(user));
    }

    private async Task<bool> PasswordMatchesAsync(RegulasUser user, string password)
    {
        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        await RehashIfNeeded(user, password, result);
        return result is not PasswordVerificationResult.Failed;
    }

    private Task RehashIfNeeded(RegulasUser user, string password, PasswordVerificationResult result)
    {
        return result is PasswordVerificationResult.SuccessRehashNeeded
            ? _store.UpdatePasswordHashAsync(user.Id, _hasher.HashPassword(user, password))
            : Task.CompletedTask;
    }

    private static CurrentUserResponse ToUserResponse(RegulasUser user)
    {
        return new CurrentUserResponse(user.Id, user.Email, user.DisplayName, user.CreatedAt, user.LastLoginAt);
    }
}

public sealed record AuthResult(AuthResultStatus Status, AuthResponse? Response)
{
    public static AuthResult Ok(AuthResponse response) => new(AuthResultStatus.Ok, response);

    public static AuthResult EmailTaken() => new(AuthResultStatus.EmailTaken, null);

    public static AuthResult InvalidLogin() => new(AuthResultStatus.InvalidLogin, null);
}

public enum AuthResultStatus
{
    Ok,
    EmailTaken,
    InvalidLogin,
}
