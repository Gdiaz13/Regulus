namespace Regulas.MauiApp;

public partial class AppShell : Shell
{
    public AppShell(MainPage mainPage, SearchPage searchPage, AuthPage authPage, SettingsPage settingsPage)
    {
        InitializeComponent();
        RegisterRoutes();
        Items.Add(TabBar(mainPage, searchPage, authPage, settingsPage));
    }

    private static TabBar TabBar(MainPage mainPage, SearchPage searchPage, AuthPage authPage, SettingsPage settingsPage)
    {
        var tabBar = new TabBar();
        tabBar.Items.Add(ShellContent("Home", nameof(MainPage), mainPage));
        tabBar.Items.Add(ShellContent("Search", nameof(SearchPage), searchPage));
        tabBar.Items.Add(ShellContent("Account", nameof(AuthPage), authPage));
        tabBar.Items.Add(ShellContent("Settings", nameof(SettingsPage), settingsPage));
        return tabBar;
    }

    private static ShellContent ShellContent(string title, string route, Page page)
    {
        return new ShellContent { Title = title, Route = route, Content = page };
    }

    private static void RegisterRoutes()
    {
        Routing.RegisterRoute(nameof(StockDetailPage), typeof(StockDetailPage));
        Routing.RegisterRoute(nameof(PriceHistoryPage), typeof(PriceHistoryPage));
    }
}
