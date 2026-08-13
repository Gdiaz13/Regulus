using System.ComponentModel;
using Regulas.MauiApp.Services;

namespace Regulas.MauiApp;

public partial class AppShell : Shell
{
    private readonly AuthSession _session;
    private readonly ShellItem _gate;
    private readonly TabBar _tabs;

    public AppShell(
        SignInPage signInPage,
        MainPage mainPage,
        SearchPage searchPage,
        PredictionsPage predictionsPage,
        TradingAgentsPage tradingAgentsPage,
        TcgPage tcgPage,
        AuthPage authPage,
        SettingsPage settingsPage,
        AuthSession session)
    {
        InitializeComponent();
        RegisterRoutes();
        _session = session;
        _gate = Gate(signInPage);
        _tabs = TabBar(mainPage, searchPage, predictionsPage, tradingAgentsPage, tcgPage, authPage, settingsPage);
        Items.Add(_gate);
        Items.Add(_tabs);
        _session.PropertyChanged += OnSessionChanged;
        ApplyAuthState();
    }

    // Signing in is the whole app's front door: the tabs do not exist until the
    // session says who you are, so no screen has to carry an anonymous state.
    private void ApplyAuthState()
    {
        var signedIn = _session.IsAuthenticated;
        _gate.IsVisible = !signedIn;
        _tabs.IsVisible = signedIn;
        CurrentItem = signedIn ? _tabs : _gate;
    }

    private void OnSessionChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(AuthSession.IsAuthenticated))
        {
            MainThread.BeginInvokeOnMainThread(ApplyAuthState);
        }
    }

    private static ShellItem Gate(Page page)
    {
        var item = new ShellItem { Route = "gate", FlyoutItemIsVisible = false };
        item.Items.Add(ShellContent("Sign in", nameof(SignInPage), page));
        return item;
    }

    private static TabBar TabBar(MainPage mainPage, SearchPage searchPage, PredictionsPage predictionsPage, TradingAgentsPage tradingAgentsPage, TcgPage tcgPage, AuthPage authPage, SettingsPage settingsPage)
    {
        var tabBar = new TabBar();
        AddTab(tabBar, "Home", nameof(MainPage), mainPage);
        AddTab(tabBar, "Search", nameof(SearchPage), searchPage);
        AddTab(tabBar, "Predictions", nameof(PredictionsPage), predictionsPage);
        AddTab(tabBar, "Research", nameof(TradingAgentsPage), tradingAgentsPage);
        AddTab(tabBar, "TCG", nameof(TcgPage), tcgPage);
        AddTab(tabBar, "Account", nameof(AuthPage), authPage);
        AddTab(tabBar, "Settings", nameof(SettingsPage), settingsPage);
        return tabBar;
    }

    private static void AddTab(TabBar tabBar, string title, string route, Page page)
    {
        tabBar.Items.Add(ShellContent(title, route, page));
    }

    private static ShellContent ShellContent(string title, string route, Page page)
    {
        return new ShellContent { Title = title, Route = route, Content = page };
    }

    private static void RegisterRoutes()
    {
        // Detail pages are pushed onto the stack, not tabs, so they register as routes.
        Routing.RegisterRoute(nameof(AssetDetailPage), typeof(AssetDetailPage));
        Routing.RegisterRoute(nameof(PriceHistoryPage), typeof(PriceHistoryPage));
        Routing.RegisterRoute(nameof(PortfolioStockPage), typeof(PortfolioStockPage));
    }
}
