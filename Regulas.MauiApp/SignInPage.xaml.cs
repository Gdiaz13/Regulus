using Regulas.MauiApp.ViewModels;

namespace Regulas.MauiApp;

public partial class SignInPage : ContentPage
{
    private readonly AuthViewModel _viewModel;
    private bool _revealed;

    public SignInPage(AuthViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    // Refreshing on appear means a still-valid stored token signs you straight
    // through: the gate shows itself only when it is actually needed.
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RevealOnceAsync();
        await _viewModel.LoadAsync();
    }

    private async Task RevealOnceAsync()
    {
        if (_revealed)
        {
            return;
        }
        _revealed = true;
        await Card.FadeToAsync(1, 450, Easing.CubicOut);
    }
}
