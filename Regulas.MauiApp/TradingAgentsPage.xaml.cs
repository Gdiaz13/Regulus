using System.Globalization;
using Regulas.MauiApp.ViewModels;

namespace Regulas.MauiApp;

// TradingAgents research for one symbol. The detail page passes symbol, price,
// and name in the route query; the analysis runs automatically on appearing.
[QueryProperty(nameof(Symbol), "symbol")]
[QueryProperty(nameof(Price), "price")]
[QueryProperty(nameof(CompanyName), "name")]
public partial class TradingAgentsPage : ContentPage
{
    private readonly TradingAgentsViewModel _viewModel;

    public TradingAgentsPage(TradingAgentsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public string Symbol { get; set; } = string.Empty;

    public string Price { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync(Symbol, ParsePrice(Price), CompanyName);
    }

    private static decimal ParsePrice(string value)
    {
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var price) ? price : 0m;
    }
}
