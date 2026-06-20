using api.Data;
using api.Endpoints;
using api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddOpenApi();
builder.Services.AddHttpClient<FinancialModelingPrepClient>(ConfigureFmpClient);

builder
    .Services
    .AddDbContext<ApplicationDBContext>(
        options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    );

var app = builder.Build();

await DatabaseStartup.InitializeAsync(app);

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
app.MapCommentEndpoints();
app.MapHealthEndpoints();
app.MapMarketDataEndpoints();
app.MapStockEndpoints();

app.Run();

static void ConfigureFmpClient(HttpClient client)
{
    client.BaseAddress = new Uri("https://financialmodelingprep.com/stable/");
    client.Timeout = TimeSpan.FromSeconds(10);
}
