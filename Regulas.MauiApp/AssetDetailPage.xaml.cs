using Regulas.MauiApp.ViewModels;

namespace Regulas.MauiApp;

// Detail screen for one asset. Shell passes the symbol as a query parameter
// (AssetDetailPage?symbol=AMD) and the page loads the profile on appearing.
[QueryProperty(nameof(Symbol), "symbol")]
public partial class AssetDetailPage : ContentPage
{
    private readonly AssetDetailViewModel _viewModel;

    public AssetDetailPage(AssetDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public string Symbol { get; set; } = string.Empty;

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync(Symbol);
    }
}
