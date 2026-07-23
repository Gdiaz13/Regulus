using api.Endpoints;
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddOpenApi();
builder.Services
    .AddAuthentication(RegulasAuthDefaults.Scheme)
    .AddScheme<AuthenticationSchemeOptions, RegulasBearerAuthenticationHandler>(RegulasAuthDefaults.Scheme, null);
builder.Services.AddAuthorization();
builder.Services.AddSingleton<PostgresConnectionFactory>();
builder.Services.AddSingleton<IDatabaseConnectionFactory>(provider => provider.GetRequiredService<PostgresConnectionFactory>());
builder.Services.AddSingleton<PostgresMigrationRunner>();
builder.Services.AddSingleton<PostgresHealthCheck>();
builder.Services.AddSingleton<IPasswordHasher<RegulasUser>, PasswordHasher<RegulasUser>>();
builder.Services.AddSingleton<AuthStore>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<AssetStore>();
builder.Services.AddSingleton<PortfolioStockStore>();
builder.Services.AddSingleton<PriceHistoryStore>();
builder.Services.AddSingleton<PredictionAccuracyStore>();
builder.Services.AddSingleton<PredictionStore>();
builder.Services.AddSingleton<StockCommentStore>();
builder.Services.AddSingleton<BackgroundJobRunStore>();
builder.Services.AddSingleton<ModelAccuracyResultStore>();
builder.Services.AddHttpClient<FinancialModelingPrepClient>(ConfigureFmpClient);
builder.Services.AddHttpClient<MagicTcgClient>(ConfigureMagicTcgClient);
builder.Services.AddHttpClient<PokemonTcgClient>(ConfigurePokemonTcgClient);
builder.Services.AddHttpClient<OnePieceTcgClient>(ConfigureOnePieceTcgClient);
builder.Services.AddHttpClient<RegulasAiClient>(ConfigureRegulasAiClient);
builder.Services.AddHttpClient<TradingAgentsClient>(ConfigureTradingAgentsClient);
builder.Services.AddHostedService<PriceSnapshotService>();
builder.Services.AddHostedService<PredictionScoringService>();
builder.Services.AddHostedService<ModelAccuracyRecalculationService>();
builder.Services.AddHostedService<ModelTrainingService>();

var app = builder.Build();

await PostgresDatabaseStartup.InitializeAsync(app);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

app.MapGet("/", () => "Exchange API running");

app.UseAuthentication();
app.UseAuthorization();

// Endpoint groups stay in api/Endpoints so Program.cs only wires the app together.
app.MapAssetEndpoints();
app.MapAuthEndpoints();
app.MapCommentEndpoints();
app.MapHealthEndpoints();
app.MapJobEndpoints();
app.MapMarketDataEndpoints();
app.MapPriceHistoryEndpoints();
app.MapPredictionEndpoints();
app.MapStockEndpoints();
app.MapTcgEndpoints();
app.MapTradingAgentsEndpoints();

app.Run();

static void ConfigureFmpClient(HttpClient client)
{
    client.BaseAddress = new Uri("https://financialmodelingprep.com/stable/");
    client.Timeout = TimeSpan.FromSeconds(10);
}

static void ConfigureRegulasAiClient(IServiceProvider services, HttpClient client)
{
    var configuration = services.GetRequiredService<IConfiguration>();
    client.BaseAddress = RegulasAiConfiguration.CoreUrl(configuration);
    client.Timeout = TimeSpan.FromSeconds(15);
}

static void ConfigurePokemonTcgClient(HttpClient client)
{
    client.BaseAddress = new Uri("https://api.pokemontcg.io/v2/");
    client.Timeout = TimeSpan.FromSeconds(10);
}

static void ConfigureMagicTcgClient(HttpClient client)
{
    client.BaseAddress = new Uri("https://api.scryfall.com/");
    client.Timeout = TimeSpan.FromSeconds(10);
}

static void ConfigureOnePieceTcgClient(HttpClient client)
{
    client.BaseAddress = new Uri("https://api.apitcg.com/");
    client.Timeout = TimeSpan.FromSeconds(10);
}

static void ConfigureTradingAgentsClient(IServiceProvider services, HttpClient client)
{
    var configuration = services.GetRequiredService<IConfiguration>();
    client.BaseAddress = TradingAgentsConfiguration.StockUrl(configuration);
    client.Timeout = TimeSpan.FromSeconds(20);
}
