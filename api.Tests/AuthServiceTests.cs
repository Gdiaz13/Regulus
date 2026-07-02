using api.Contracts;
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace api.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_hashes_password_and_returns_token()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new AuthStore(factory);
        var result = await Service(store).RegisterAsync(Register());
        var user = await store.FindByNormalizedEmailAsync("ME@EXAMPLE.COM");
        Assert.Equal(AuthResultStatus.Ok, result.Status);
        Assert.False(string.IsNullOrWhiteSpace(result.Response!.Token));
        Assert.NotEqual("Password123!", user!.PasswordHash);
    }

    [Fact]
    public async Task RegisterAsync_rejects_duplicate_email()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new AuthStore(factory);
        await Service(store).RegisterAsync(Register(email: "me@example.com"));
        var duplicate = await Service(store).RegisterAsync(Register(email: " ME@example.com "));
        Assert.Equal(AuthResultStatus.EmailTaken, duplicate.Status);
    }

    [Fact]
    public async Task LoginAsync_rejects_invalid_password()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var service = Service(new AuthStore(factory));
        await service.RegisterAsync(Register());
        var result = await service.LoginAsync(new LoginRequest("me@example.com", "wrongpass"));
        Assert.Equal(AuthResultStatus.InvalidLogin, result.Status);
    }

    [Fact]
    public async Task LoginAsync_returns_token_for_valid_password()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var service = Service(new AuthStore(factory));
        await service.RegisterAsync(Register());
        var result = await service.LoginAsync(new LoginRequest("me@example.com", "Password123!"));
        Assert.Equal(AuthResultStatus.Ok, result.Status);
        Assert.False(string.IsNullOrWhiteSpace(result.Response!.Token));
    }

    [Fact]
    public async Task LogoutAsync_revokes_returned_token()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new AuthStore(factory);
        var result = await Service(store).RegisterAsync(Register());
        var hash = AuthTokenService.Hash(result.Response!.Token);
        Assert.True(await Service(store).LogoutAsync(hash));
        Assert.Null(await store.FindUserByTokenHashAsync(hash));
    }

    private static AuthService Service(AuthStore store)
    {
        return new AuthService(store, new PasswordHasher<RegulasUser>());
    }

    private static RegisterRequest Register(string email = "me@example.com")
    {
        return new RegisterRequest(email, "Password123!", "Me");
    }
}
