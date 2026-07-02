using api.Endpoints;
using api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace api.Tests;

public class UserOwnedEndpointAuthorizationTests
{
    [Fact]
    public void User_owned_routes_require_authorization()
    {
        using var app = MappedApp();
        foreach (var route in ProtectedRoutes())
        {
            AssertProtected(FindEndpoint(app, route.Pattern, route.Method));
        }
    }

    [Fact]
    public void Prediction_health_stays_public()
    {
        using var app = MappedApp();
        var endpoint = FindEndpoint(app, "/api/predict/health", "GET");
        Assert.NotNull(endpoint.Metadata.GetMetadata<IAllowAnonymous>());
    }

    private static WebApplication MappedApp()
    {
        var builder = WebApplication.CreateBuilder();
        RegisterEndpointServices(builder.Services);
        var app = builder.Build();
        app.MapStockEndpoints();
        app.MapCommentEndpoints();
        app.MapPredictionEndpoints();
        return app;
    }

    private static void RegisterEndpointServices(IServiceCollection services)
    {
        services.AddAuthorization();
        services.AddSingleton<IDatabaseConnectionFactory, SqliteDapperConnectionFactory>();
        services.AddSingleton<PortfolioStockStore>();
        services.AddSingleton<StockCommentStore>();
        services.AddSingleton<PredictionStore>();
        services.AddSingleton<PredictionAccuracyStore>();
        services.AddSingleton(new RegulasAiClient(new HttpClient()));
    }

    private static IEnumerable<RouteCase> ProtectedRoutes()
    {
        yield return new("/api/stocks/", "GET");
        yield return new("/api/stocks/", "POST");
        yield return new("/api/stocks/{symbol}", "GET");
        yield return new("/api/stocks/{id:int}", "PUT");
        yield return new("/api/stocks/{id:int}", "DELETE");
        yield return new("/api/stocks/{stockId:int}/comments", "GET");
        yield return new("/api/stocks/{stockId:int}/comments", "POST");
        yield return new("/api/comments/{id:int}", "PUT");
        yield return new("/api/comments/{id:int}", "DELETE");
        yield return new("/api/predict/", "POST");
        yield return new("/api/predict/history", "GET");
        yield return new("/api/predict/accuracy", "GET");
    }

    private static void AssertProtected(RouteEndpoint endpoint)
    {
        Assert.NotNull(endpoint.Metadata.GetMetadata<IAuthorizeData>());
    }

    private static RouteEndpoint FindEndpoint(WebApplication app, string pattern, string method)
    {
        var endpoints = Endpoints(app);
        return endpoints.SingleOrDefault(endpoint => Matches(endpoint, pattern, method))
            ?? throw new InvalidOperationException($"Missing {method} {pattern}. Found: {EndpointList(endpoints)}");
    }

    private static List<RouteEndpoint> Endpoints(WebApplication app)
    {
        return ((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints).OfType<RouteEndpoint>().ToList();
    }

    private static bool Matches(RouteEndpoint endpoint, string pattern, string method)
    {
        return endpoint.RoutePattern.RawText == pattern && Methods(endpoint).Contains(method);
    }

    private static IEnumerable<string> Methods(RouteEndpoint endpoint)
    {
        return endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods ?? [];
    }

    private static string EndpointList(IEnumerable<RouteEndpoint> endpoints)
    {
        return string.Join(", ", endpoints.Select(endpoint => $"{string.Join('|', Methods(endpoint))} {endpoint.RoutePattern.RawText}"));
    }

    private sealed record RouteCase(string Pattern, string Method);
}
