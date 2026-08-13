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
    public async Task New_accounts_are_ordinary_users()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new AuthStore(factory);

        var user = await store.CreateUserAsync(User("plain@example.com"));

        Assert.False(user.IsAdmin);
        Assert.False((await store.FindByNormalizedEmailAsync("PLAIN@EXAMPLE.COM"))!.IsAdmin);
    }

    // Registration must not be able to hand itself admin rights, so the insert
    // ignores the flag on the incoming user entirely.
    [Fact]
    public async Task Registering_cannot_grant_itself_admin_rights()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new AuthStore(factory);
        var claimed = User("sneaky@example.com");
        claimed.IsAdmin = true;

        var created = await store.CreateUserAsync(claimed);

        Assert.False(created.IsAdmin);
    }

    [Fact]
    public async Task A_promoted_user_reads_back_as_an_admin()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new AuthStore(factory);
        var user = await store.CreateUserAsync(User("boss@example.com"));

        Assert.True(await store.SetAdminAsync(user.Id, true));

        var found = await store.FindByIdAsync(user.Id);
        Assert.True(found!.IsAdmin);
        Assert.True(((IAdminAware)found).IsAdmin);
    }

    [Fact]
    public async Task Admin_rights_can_be_taken_away_again()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new AuthStore(factory);
        var user = await store.CreateUserAsync(User("demoted@example.com"));
        await store.SetAdminAsync(user.Id, true);

        await store.SetAdminAsync(user.Id, false);

        Assert.False((await store.FindByIdAsync(user.Id))!.IsAdmin);
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
            Id = Guid.NewGuid(),
            Email = email,
            NormalizedEmail = AuthService.NormalizeEmail(email),
            DisplayName = "Test User",
            PasswordHash = "hashed",
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
        };
    }
}
