using Regulas.MauiApp.ViewModels;

namespace Regulas.MauiApp;

// TCG tab: hand-entered card prices with source metadata, loaded on appearing.
public partial class TcgPage : ContentPage
{
    private readonly TcgViewModel _viewModel;

    public TcgPage(TcgViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
