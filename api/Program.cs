using api.Data;
using api.Endpoints;
using api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHttpClient<FinancialModelingPrepClient>(ConfigureFmpClient);

builder
    .Services
    .AddDbContext<ApplicationDBContext>(
        options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    );

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

app.MapGet("/", () => "Exchange API running");
app.MapMarketDataEndpoints();
app.MapStockEndpoints();

app.Run();

static void ConfigureFmpClient(HttpClient client)
{
    client.BaseAddress = new Uri("https://financialmodelingprep.com/stable/");
}
