using api.Endpoints;
using api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddOpenApi();
builder.Services.AddSingleton<PostgresConnectionFactory>();
builder.Services.AddSingleton<IDatabaseConnectionFactory>(provider => provider.GetRequiredService<PostgresConnectionFactory>());
builder.Services.AddSingleton<PostgresMigrationRunner>();
builder.Services.AddSingleton<PostgresHealthCheck>();
builder.Services.AddSingleton<AssetStore>();
builder.Services.AddSingleton<PortfolioStockStore>();
builder.Services.AddSingleton<PriceHistoryStore>();
builder.Services.AddSingleton<PredictionAccuracyStore>();
builder.Services.AddSingleton<PredictionStore>();
builder.Services.AddSingleton<StockCommentStore>();
builder.Services.AddHttpClient<FinancialModelingPrepClient>(ConfigureFmpClient);
builder.Services.AddHttpClient<RegulasAiClient>(ConfigureRegulasAiClient);
builder.Services.AddHttpClient<TradingAgentsClient>(ConfigureTradingAgentsClient);

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

// Endpoint groups stay in api/Endpoints so Program.cs only wires the app together.
app.MapAssetEndpoints();
app.MapCommentEndpoints();
app.MapHealthEndpoints();
app.MapMarketDataEndpoints();
app.MapPriceHistoryEndpoints();
app.MapPredictionEndpoints();
app.MapStockEndpoints();
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

static void ConfigureTradingAgentsClient(IServiceProvider services, HttpClient client)
{
    var configuration = services.GetRequiredService<IConfiguration>();
    client.BaseAddress = TradingAgentsConfiguration.StockUrl(configuration);
    client.Timeout = TimeSpan.FromSeconds(20);
}
