using System.Security.Claims;
using api.Endpoints;
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace api.Tests;

public class AdminAuthorizationTests
{
    [Fact]
    public void Admin_routes_demand_the_admin_policy()
    {
        using var app = MappedApp();

        foreach (var route in AdminRoutes())
        {
            AssertAdminOnly(FindEndpoint(app, route.Pattern, route.Method));
        }
    }

    // Job history exposes which providers fail and how the schedule behaves.
    // It used to answer anyone at all.
    [Fact]
    public void Job_runs_are_no_longer_public()
    {
        using var app = MappedApp();
        var endpoint = FindEndpoint(app, "/api/jobs/runs", "GET");

        Assert.Null(endpoint.Metadata.GetMetadata<IAllowAnonymous>());
        AssertAdminOnly(endpoint);
    }

    [Fact]
    public async Task The_policy_admits_an_admin_and_turns_away_a_signed_in_user()
    {
        var provider = AuthorizationProvider();
        var service = provider.GetRequiredService<IAuthorizationService>();

        var admin = await service.AuthorizeAsync(Principal(isAdmin: true), null, RegulasAuthDefaults.AdminPolicy);
        var ordinary = await service.AuthorizeAsync(Principal(isAdmin: false), null, RegulasAuthDefaults.AdminPolicy);

        Assert.True(admin.Succeeded);
        Assert.False(ordinary.Succeeded);
    }

    [Fact]
    public async Task The_policy_turns_away_an_anonymous_caller()
    {
        var service = AuthorizationProvider().GetRequiredService<IAuthorizationService>();

        var result = await service.AuthorizeAsync(new ClaimsPrincipal(new ClaimsIdentity()), null, RegulasAuthDefaults.AdminPolicy);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void Only_an_admin_row_earns_the_admin_claim()
    {
        Assert.True(CurrentUser.IsAdmin(Principal(isAdmin: true)));
        Assert.False(CurrentUser.IsAdmin(Principal(isAdmin: false)));
    }

    // Losing every admin is unrecoverable without database access, so an admin
    // is not allowed to strip their own rights.
    [Fact]
    public async Task An_admin_cannot_demote_themselves()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new AuthStore(factory);
        var admin = await store.CreateUserAsync(User("boss@example.com"));
        await store.SetAdminAsync(admin.Id, true);

        var result = await InvokeSetAdmin(admin.Id, admin.Id, false, store);

        Assert.Equal(StatusCodes.Status400BadRequest, await StatusOf(result));
        Assert.True((await store.FindByIdAsync(admin.Id))!.IsAdmin);
    }

    [Fact]
    public async Task An_admin_can_promote_someone_else()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new AuthStore(factory);
        var admin = await store.CreateUserAsync(User("boss@example.com"));
        var target = await store.CreateUserAsync(User("new@example.com"));

        await InvokeSetAdmin(admin.Id, target.Id, true, store);

        Assert.True((await store.FindByIdAsync(target.Id))!.IsAdmin);
    }

    [Fact]
    public async Task Promoting_a_stranger_reports_not_found()
    {
        using var factory = new SqliteDapperConnectionFactory();
        var store = new AuthStore(factory);
        var admin = await store.CreateUserAsync(User("boss@example.com"));

        var result = await InvokeSetAdmin(admin.Id, Guid.NewGuid(), true, store);

        Assert.Equal(StatusCodes.Status404NotFound, await StatusOf(result));
    }

    private static async Task<IResult> InvokeSetAdmin(Guid callerId, Guid targetId, bool isAdmin, AuthStore store)
    {
        var method = typeof(AdminEndpoints).GetMethod("SetAdmin", BindingFlags())
            ?? throw new MissingMethodException(nameof(AdminEndpoints), "SetAdmin");
        var args = new object[] { targetId, new SetAdminRequest(isAdmin), Context(callerId), store };
        return await (Task<IResult>)method.Invoke(null, args)!;
    }

    private static System.Reflection.BindingFlags BindingFlags()
    {
        return System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;
    }

    private static async Task<int> StatusOf(IResult result)
    {
        var context = new DefaultHttpContext { RequestServices = ResultServices() };
        context.Response.Body = new MemoryStream();
        await result.ExecuteAsync(context);
        return context.Response.StatusCode;
    }

    private static ServiceProvider ResultServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        return services.BuildServiceProvider();
    }

    private static DefaultHttpContext Context(Guid callerId)
    {
        return new DefaultHttpContext { User = Principal(isAdmin: true, id: callerId) };
    }

    private static ClaimsPrincipal Principal(bool isAdmin, Guid? id = null)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, (id ?? Guid.NewGuid()).ToString()) };
        if (isAdmin)
        {
            claims.Add(new Claim(RegulasAuthDefaults.AdminClaim, "true"));
        }
        return new ClaimsPrincipal(new ClaimsIdentity(claims, RegulasAuthDefaults.Scheme));
    }

    private static ServiceProvider AuthorizationProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization(options => options.AddPolicy(
            RegulasAuthDefaults.AdminPolicy,
            policy => policy.RequireAuthenticatedUser().RequireClaim(RegulasAuthDefaults.AdminClaim, "true")));
        return services.BuildServiceProvider();
    }

    private static RegulasUser User(string email)
    {
        return new RegulasUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            DisplayName = email,
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow,
        };
    }

    private static WebApplication MappedApp()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddAuthorization();
        builder.Services.AddSingleton<IDatabaseConnectionFactory, SqliteDapperConnectionFactory>();
        builder.Services.AddSingleton<BackgroundJobRunStore>();
        builder.Services.AddSingleton<AuthStore>();
        var app = builder.Build();
        app.MapJobEndpoints();
        app.MapAdminEndpoints();
        return app;
    }

    private static IEnumerable<RouteCase> AdminRoutes()
    {
        yield return new("/api/jobs/runs", "GET");
        yield return new("/api/v1/admin/users/{id:guid}/admin", "PUT");
    }

    private static void AssertAdminOnly(RouteEndpoint endpoint)
    {
        var data = endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>();
        Assert.Contains(data, item => item.Policy == RegulasAuthDefaults.AdminPolicy);
    }

    private static RouteEndpoint FindEndpoint(WebApplication app, string pattern, string method)
    {
        var endpoints = ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(source => source.Endpoints).OfType<RouteEndpoint>().ToList();
        return endpoints.SingleOrDefault(endpoint => Matches(endpoint, pattern, method))
            ?? throw new InvalidOperationException($"Missing {method} {pattern}.");
    }

    private static bool Matches(RouteEndpoint endpoint, string pattern, string method)
    {
        var methods = endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods ?? [];
        return endpoint.RoutePattern.RawText == pattern && methods.Contains(method);
    }

    private sealed record RouteCase(string Pattern, string Method);
}
