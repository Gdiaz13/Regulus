using Regulas.MauiApp.ViewModels;

namespace Regulas.MauiApp;

public partial class TradingAgentsPage : ContentPage, IQueryAttributable
{
    private readonly TradingAgentsViewModel _viewModel;

    public TradingAgentsPage(TradingAgentsViewModel viewModel)
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

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _viewModel.ApplyQuery(
            Value(query, "symbol"),
            Value(query, "companyName"),
            Value(query, "currentPrice"));
    }

    private static string? Value(IDictionary<string, object> query, string key)
    {
        return query.TryGetValue(key, out var value) ? value?.ToString() : null;
    }
}
