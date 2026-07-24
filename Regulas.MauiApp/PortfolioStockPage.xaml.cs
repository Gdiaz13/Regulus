using Regulas.MauiApp.ViewModels;

namespace Regulas.MauiApp;

public partial class PortfolioStockPage : ContentPage, IQueryAttributable
{
    private readonly PortfolioStockViewModel _viewModel;

    public PortfolioStockPage(PortfolioStockViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        var symbol = query.TryGetValue("symbol", out var value) ? value?.ToString() : null;
        _viewModel.ApplySymbol(symbol);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
