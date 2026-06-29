namespace Regulas.MauiApp;

public partial class AppShell : Shell
{
    public AppShell(MainPage mainPage, SettingsPage settingsPage)
    {
        InitializeComponent();
        Items.Add(TabBar(mainPage, settingsPage));
    }

    private static TabBar TabBar(MainPage mainPage, SettingsPage settingsPage)
    {
        var tabBar = new TabBar();
        tabBar.Items.Add(ShellContent("Home", nameof(MainPage), mainPage));
        tabBar.Items.Add(ShellContent("Settings", nameof(SettingsPage), settingsPage));
        return tabBar;
    }

    private static ShellContent ShellContent(string title, string route, Page page)
    {
        return new ShellContent { Title = title, Route = route, Content = page };
    }
}
