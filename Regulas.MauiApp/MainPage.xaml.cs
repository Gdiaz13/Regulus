using Regulas.MauiApp.ViewModels;

namespace Regulas.MauiApp;

public partial class MainPage : ContentPage
{
    private readonly HomeViewModel _viewModel;

    public MainPage(HomeViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, EventArgs e)
    {
        Loaded -= OnLoaded;
        await _viewModel.LoadAsync();
    }
}
