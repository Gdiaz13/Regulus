using api.Models;
using api.Services;
using Xunit;

namespace api.Tests;

public class AuthStoreTests
{
    [Fact]
    public async Task CreateUserAsync_round_trips_user_fields()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new AuthStore(factory);
        var user = await store.CreateUserAsync(User("me@example.com"));
        var found = await store.FindByNormalizedEmailAsync("ME@EXAMPLE.COM");
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("me@example.com", found!.Email);
        Assert.True(found.IsActive);
    }

    [Fact]
    public async Task FindUserByTokenHashAsync_returns_active_unexpired_user()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new AuthStore(factory);
        var user = await store.CreateUserAsync(User("token@example.com"));
        var hash = AuthTokenService.Hash("secret-token");
        await store.CreateRefreshTokenAsync(user.Id, hash, DateTime.UtcNow.AddDays(1));
        var found = await store.FindUserByTokenHashAsync(hash);
        Assert.Equal(user.Id, found!.Id);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_removes_token_from_lookup()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new AuthStore(factory);
        var user = await store.CreateUserAsync(User("logout@example.com"));
        var hash = AuthTokenService.Hash("logout-token");
        await store.CreateRefreshTokenAsync(user.Id, hash, DateTime.UtcNow.AddDays(1));
        Assert.True(await store.RevokeRefreshTokenAsync(hash));
        Assert.Null(await store.FindUserByTokenHashAsync(hash));
    }

    private static RegulasUser User(string email)
    {
        return new RegulasUser
        {
            Email = email,
            NormalizedEmail = AuthService.NormalizeEmail(email),
            DisplayName = "Test User",
            PasswordHash = "hashed",
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
        };
    }
}
