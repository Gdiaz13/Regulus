using Regulas.MauiApp.ViewModels;

namespace Regulas.MauiApp;

public partial class MainPage : ContentPage
{
    private readonly HomeViewModel _viewModel;
    private bool _revealed;

    public MainPage(HomeViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RevealOnceAsync();
        await _viewModel.LoadAsync();
    }

    // The sky fades up the first time the tab opens, so the page arrives rather
    // than blinking in. Later visits skip it; a repeated fade would nag.
    private async Task RevealOnceAsync()
    {
        if (_revealed)
        {
            return;
        }
        _revealed = true;
        await PageBody.FadeToAsync(1, 450, Easing.CubicOut);
    }
}
