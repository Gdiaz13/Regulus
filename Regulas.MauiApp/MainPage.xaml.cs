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
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
