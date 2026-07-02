using Regulas.MauiApp.ViewModels;

namespace Regulas.MauiApp;

public partial class AuthPage : ContentPage
{
    private readonly AuthViewModel _viewModel;

    public AuthPage(AuthViewModel viewModel)
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
