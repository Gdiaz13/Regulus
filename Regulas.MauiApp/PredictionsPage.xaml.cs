using Regulas.MauiApp.ViewModels;

namespace Regulas.MauiApp;

public partial class PredictionsPage : ContentPage, IQueryAttributable
{
    private readonly PredictionsViewModel _viewModel;

    public PredictionsPage(PredictionsViewModel viewModel)
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
        var symbol = query.TryGetValue("symbol", out var value) ? value?.ToString() : null;
        _viewModel.ApplySymbol(symbol);
    }
}
