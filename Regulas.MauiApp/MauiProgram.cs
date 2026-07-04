using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Regulas.MauiApp.Services;
using Regulas.MauiApp.ViewModels;

namespace Regulas.MauiApp;

public static class MauiProgram
{
    public static Microsoft.Maui.Hosting.MauiApp CreateMauiApp()
    {
        var builder = Microsoft.Maui.Hosting.MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        RegisterServices(builder.Services);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static void RegisterServices(IServiceCollection services)
    {
        RegisterPages(services);
        RegisterViewModels(services);
        RegisterApiServices(services);
    }

    private static void RegisterPages(IServiceCollection services)
    {
        services.AddSingleton<AppShell>();
        services.AddSingleton<MainPage>();
        services.AddSingleton<SearchPage>();
        services.AddSingleton<PredictionsPage>();
        services.AddSingleton<AuthPage>();
        services.AddSingleton<SettingsPage>();
        // Detail pages are transient: each navigation carries its own symbol.
        services.AddTransient<AssetDetailPage>();
        services.AddTransient<PriceHistoryPage>();
        services.AddTransient<TradingAgentsPage>();
    }

    private static void RegisterViewModels(IServiceCollection services)
    {
        services.AddSingleton<HomeViewModel>();
        services.AddSingleton<SearchViewModel>();
        services.AddSingleton<PredictionsViewModel>();
        services.AddSingleton<AuthViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddTransient<AssetDetailViewModel>();
        services.AddTransient<PriceHistoryViewModel>();
        services.AddTransient<TradingAgentsViewModel>();
    }

    private static void RegisterApiServices(IServiceCollection services)
    {
        services.AddSingleton<IAuthTokenStore, SecureAuthTokenStore>();
        services.AddSingleton<AuthSession>();
        services.AddSingleton<IRegulasApiClient, RegulasApiClient>();
        services.AddSingleton(CreateApiHttpClient);
    }

    private static HttpClient CreateApiHttpClient(IServiceProvider services)
    {
        return new HttpClient { BaseAddress = new Uri(ApiBaseUrl.Current) };
    }
}
