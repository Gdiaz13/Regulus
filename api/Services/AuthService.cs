using api.Contracts;
using api.Models;
using Microsoft.AspNetCore.Identity;

namespace api.Services;

public sealed class AuthService
{
    private static readonly TimeSpan SessionLifetime = TimeSpan.FromDays(14);
    private readonly AuthStore _store;
    private readonly IPasswordHasher<RegulasUser> _hasher;

    public AuthService(AuthStore store, IPasswordHasher<RegulasUser> hasher)
    {
        _store = store;
        _hasher = hasher;
    }

    public async Task<AuthServiceResult> RegisterAsync(string email, string password, string displayName)
    {
        var normalizedEmail = NormalizeEmail(email);
        if (await _store.FindByEmailAsync(normalizedEmail) is not null)
        {
            return AuthServiceResult.EmailTaken();
        }
        var user = await _store.CreateUserAsync(NewUser(email, normalizedEmail, displayName, password));
        return AuthServiceResult.Ok(await IssueTokenAsync(user));
    }

    public async Task<AuthServiceResult> LoginAsync(string email, string password)
    {
        var user = await _store.FindByEmailAsync(NormalizeEmail(email));
        if (user is null || !user.IsActive || !await PasswordMatchesAsync(user, password))
        {
            return AuthServiceResult.InvalidLogin();
        }
        await _store.MarkLoginAsync(user.Id);
        return AuthServiceResult.Ok(await IssueTokenAsync(user));
    }

    public Task<bool> LogoutAsync(string tokenHash)
    {
        return string.IsNullOrWhiteSpace(tokenHash) ? Task.FromResult(false) : _store.RevokeSessionAsync(tokenHash);
    }

    public async Task<AuthUserResponse?> CurrentUserAsync(Guid userId)
    {
        var user = await _store.FindByIdAsync(userId);
        return user is null || !user.IsActive ? null : ToResponse(user);
    }

    public static string NormalizeEmail(string email)
    {
        return email.Trim().ToUpperInvariant();
    }

    private RegulasUser NewUser(string email, string normalizedEmail, string displayName, string password)
    {
        var user = BareUser(email, normalizedEmail, displayName);
        return new RegulasUser
        {
            Id = user.Id,
            Email = user.Email,
            NormalizedEmail = user.NormalizedEmail,
            DisplayName = user.DisplayName,
            PasswordHash = _hasher.HashPassword(user, password),
            CreatedAt = user.CreatedAt,
            IsActive = user.IsActive,
        };
    }

    private static RegulasUser BareUser(string email, string normalizedEmail, string displayName)
    {
        return new RegulasUser
        {
            Id = Guid.NewGuid(),
            Email = email.Trim(),
            NormalizedEmail = normalizedEmail,
            DisplayName = displayName.Trim(),
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
        };
    }

    private async Task<AuthResponse> IssueTokenAsync(RegulasUser user)
    {
        var token = AuthTokenService.NewToken();
        var expiresAt = DateTime.UtcNow.Add(SessionLifetime);
        await _store.CreateSessionAsync(user.Id, AuthTokenService.Hash(token), expiresAt);
        return new AuthResponse(token, expiresAt, ToResponse(user));
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

    private static AuthUserResponse ToResponse(RegulasUser user)
    {
        return new AuthUserResponse(user.Id, user.Email, user.DisplayName, user.CreatedAt, user.LastLoginAt);
    }
}

public sealed record AuthServiceResult(AuthServiceStatus Status, AuthResponse? Response)
{
    public static AuthServiceResult Ok(AuthResponse response)
    {
        return new AuthServiceResult(AuthServiceStatus.Ok, response);
    }

    public static AuthServiceResult EmailTaken()
    {
        return new AuthServiceResult(AuthServiceStatus.EmailTaken, null);
    }

    public static AuthServiceResult InvalidLogin()
    {
        return new AuthServiceResult(AuthServiceStatus.InvalidLogin, null);
    }
}

public enum AuthServiceStatus
{
    Ok,
    EmailTaken,
    InvalidLogin,
}
