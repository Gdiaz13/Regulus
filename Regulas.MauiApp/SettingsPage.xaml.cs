using Regulas.MauiApp.ViewModels;

namespace Regulas.MauiApp;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
