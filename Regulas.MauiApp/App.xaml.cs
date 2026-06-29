using Microsoft.Extensions.DependencyInjection;

namespace Regulas.MauiApp;

public partial class App : Application
{
    private readonly IServiceProvider _services;

    public App(IServiceProvider services)
    {
        InitializeComponent();
        _services = services;
    }

    // Resolve the shell here, not via the constructor, so App.xaml's merged resource
    // dictionaries (Colors/Styles) load before the pages build. Otherwise the pages'
    // StaticResource lookups (e.g. MidnightBlue) run against empty resources and crash.
    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(_services.GetRequiredService<AppShell>());
    }
}
