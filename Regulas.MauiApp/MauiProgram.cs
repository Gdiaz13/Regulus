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
        services.AddSingleton<AppShell>();
        services.AddSingleton<MainPage>();
        services.AddSingleton<SettingsPage>();
        services.AddSingleton<HomeViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<IRegulasApiClient, RegulasApiClient>();
        services.AddSingleton(CreateApiHttpClient);
    }

    private static HttpClient CreateApiHttpClient(IServiceProvider services)
    {
        return new HttpClient { BaseAddress = new Uri(ApiBaseUrl.Current) };
    }
}
