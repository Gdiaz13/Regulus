using Regulas.MauiApp.ViewModels;

namespace Regulas.MauiApp;

public partial class StockDetailPage : ContentPage, IQueryAttributable
{
    private readonly StockDetailViewModel _viewModel;

    public StockDetailPage(StockDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        var symbol = query.TryGetValue("symbol", out var value) ? value?.ToString() : null;
        _ = _viewModel.LoadAsync(symbol);
    }
}
