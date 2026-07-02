using System.Reflection;
using System.Text.Json;
using api.Contracts;
using api.Endpoints;
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace api.Tests;

public class AuthEndpointTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Register_returns_created_auth_response()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var result = await Invoke("Register", Register(), Service(factory));
        var response = await Execute(result);
        var auth = JsonSerializer.Deserialize<AuthResponse>(response.Body, JsonOptions);
        Assert.Equal(StatusCodes.Status201Created, response.StatusCode);
        Assert.Equal("me@example.com", auth!.User.Email);
        Assert.False(string.IsNullOrWhiteSpace(auth.Token));
    }

    [Fact]
    public async Task Login_returns_unauthorized_for_bad_password()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var service = Service(factory);
        await service.RegisterAsync(Register());
        var result = await Invoke("Login", new LoginRequest("me@example.com", "wrongpass"), service);
        var response = await Execute(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, response.StatusCode);
    }

    private static RegisterRequest Register()
    {
        return new RegisterRequest("me@example.com", "Password123!", "Me");
    }

    private static AuthService Service(SqliteDapperConnectionFactory factory)
    {
        return new AuthService(new AuthStore(factory), new PasswordHasher<RegulasUser>());
    }

    private static async Task<IResult> Invoke(string name, object request, AuthService service)
    {
        var method = typeof(AuthEndpoints).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new MissingMethodException(nameof(AuthEndpoints), name);
        var task = (Task<IResult>?)method.Invoke(null, [request, service])
            ?? throw new InvalidOperationException($"{name} did not return a result task.");
        return await task;
    }

    private static async Task<ResponseSnapshot> Execute(IResult result)
    {
        var context = new DefaultHttpContext { Response = { Body = new MemoryStream() } };
        context.RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider();
        await result.ExecuteAsync(context);
        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        return new ResponseSnapshot(context.Response.StatusCode, body);
    }

    private sealed record ResponseSnapshot(int StatusCode, string Body);
}
