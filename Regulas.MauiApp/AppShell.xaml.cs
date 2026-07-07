namespace Regulas.MauiApp;

public partial class AppShell : Shell
{
    public AppShell(
        MainPage mainPage,
        SearchPage searchPage,
        PredictionsPage predictionsPage,
        TradingAgentsPage tradingAgentsPage,
        TcgPage tcgPage,
        AuthPage authPage,
        SettingsPage settingsPage)
    {
        InitializeComponent();
        RegisterRoutes();
        Items.Add(TabBar(mainPage, searchPage, predictionsPage, tradingAgentsPage, tcgPage, authPage, settingsPage));
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
    }
}
