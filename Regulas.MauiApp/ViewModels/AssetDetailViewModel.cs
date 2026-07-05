using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Regulas.MauiApp.Models;
using Regulas.MauiApp.Services;

namespace Regulas.MauiApp.ViewModels;

// Loads one company profile through Regulas.Api for the asset detail screen.
// The provider key stays server-side; this only sees the proxied response.
public sealed class AssetDetailViewModel : INotifyPropertyChanged
{
    private readonly IRegulasApiClient _apiClient;
    private string _errorText = string.Empty;
    private bool _isBusy;
    private CompanyProfile? _profile;
    private string _symbol = string.Empty;

    public AssetDetailViewModel(IRegulasApiClient apiClient)
    {
        _apiClient = apiClient;
        OpenPriceHistoryCommand = new Command(async () => await NavigationRoutes.OpenPriceHistoryAsync(Symbol));
        OpenPredictionCommand = new Command(async () => await NavigationRoutes.OpenPredictionsAsync(Symbol));
        OpenTradingAgentsCommand = new Command(async () => await OpenTradingAgentsAsync());
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand OpenPriceHistoryCommand { get; }
    public ICommand OpenPredictionCommand { get; }
    public ICommand OpenTradingAgentsCommand { get; }
    public string Symbol { get => _symbol; private set => SetField(ref _symbol, value); }
    public bool IsBusy { get => _isBusy; private set => SetField(ref _isBusy, value); }
    public string ErrorText => _errorText;
    public bool HasError => !IsBusy && !string.IsNullOrWhiteSpace(_errorText);
    public bool HasProfile => _profile is not null;
    public string NameText => _profile?.CompanyName ?? string.Empty;
    public string PriceText => _profile is null ? string.Empty : $"{_profile.Price:N2} {_profile.Currency}";
    public string ChangeText => _profile is null ? string.Empty : $"{_profile.Change:+0.00;-0.00} ({_profile.ChangePercentage:+0.00;-0.00}%) today";
    public string MarketCapText => _profile is null ? string.Empty : $"Market cap {CompactNumber(_profile.MarketCap)} {_profile.Currency}";
    public string SectorText => Join(_profile?.Sector, _profile?.Industry);
    public string ExchangeText => Join(_profile?.ExchangeFullName, _profile?.Country);
    public string CeoText => string.IsNullOrWhiteSpace(_profile?.Ceo) ? string.Empty : $"CEO: {_profile.Ceo}";
    public string WebsiteText => _profile?.Website ?? string.Empty;
    public string DescriptionText => _profile?.Description ?? string.Empty;
    public bool HasDescription => HasProfile && !string.IsNullOrWhiteSpace(DescriptionText);

    public async Task LoadAsync(string symbol)
    {
        if (IsBusy || string.IsNullOrWhiteSpace(symbol))
        {
            return;
        }
        Symbol = symbol.Trim().ToUpperInvariant();
        await RunLoadAsync();
    }

    // Wraps the load so IsBusy always resets, even when the request throws.
    private async Task RunLoadAsync()
    {
        IsBusy = true;
        try
        {
            ApplyResult(await _apiClient.GetCompanyProfileAsync(Symbol, CancellationToken.None));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyResult(ApiClientResult<CompanyProfile> result)
    {
        _profile = result.Ok ? result.Data : null;
        _errorText = result.Ok ? string.Empty : result.Message;
        OnPropertyChanged(string.Empty);
    }

    // Passes loaded profile values so the research tab can start prefilled.
    private Task OpenTradingAgentsAsync()
    {
        return NavigationRoutes.OpenTradingAgentsAsync(Symbol, _profile?.CompanyName, _profile?.Price);
    }

    // 3.2T reads better on a phone screen than 3,200,000,000,000.
    private static string CompactNumber(long value)
    {
        return value switch
        {
            >= 1_000_000_000_000 => $"{value / 1_000_000_000_000d:0.##}T",
            >= 1_000_000_000 => $"{value / 1_000_000_000d:0.##}B",
            >= 1_000_000 => $"{value / 1_000_000d:0.##}M",
            _ => value.ToString("N0"),
        };
    }

    private static string Join(string? left, string? right)
    {
        var parts = new[] { left, right }.Where(part => !string.IsNullOrWhiteSpace(part));
        return string.Join(" - ", parts);
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }
        field = value;
        OnPropertyChanged(name);
        return true;
    }

    private void OnPropertyChanged(string? name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
