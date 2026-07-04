using Regulas.MauiApp.ViewModels;

namespace Regulas.MauiApp;

public partial class PriceHistoryPage : ContentPage, IQueryAttributable
{
    private readonly PriceHistoryViewModel _viewModel;

    public PriceHistoryPage(PriceHistoryViewModel viewModel)
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
}
