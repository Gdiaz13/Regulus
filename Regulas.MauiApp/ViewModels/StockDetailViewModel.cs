using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Regulas.MauiApp.Models;
using Regulas.MauiApp.Services;

namespace Regulas.MauiApp.ViewModels;

public sealed class StockDetailViewModel : INotifyPropertyChanged
{
    private static readonly string[] ProfilePropertyNames = [
        nameof(HasProfile), nameof(ShowStatus), nameof(StatusText), nameof(TitleText),
        nameof(SymbolText), nameof(SubtitleText), nameof(PriceText), nameof(ChangeText),
        nameof(DescriptionText), nameof(WebsiteText)
    ];
    private readonly IRegulasApiClient _apiClient;
    private string _errorText = string.Empty;
    private bool _isBusy;
    private CompanyProfile? _profile;
    private string _symbol = string.Empty;

    public StockDetailViewModel(IRegulasApiClient apiClient)
    {
        _apiClient = apiClient;
        OpenPriceHistoryCommand = new Command(async () => await NavigationRoutes.OpenPriceHistoryAsync(_symbol));
        OpenPredictionCommand = new Command(async () => await NavigationRoutes.OpenPredictionsAsync(_symbol));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<ProfileMetric> Facts { get; } = [];
    public ObservableCollection<ProfileMetric> Metrics { get; } = [];
    public ICommand OpenPriceHistoryCommand { get; }
    public ICommand OpenPredictionCommand { get; }
    public bool IsBusy { get => _isBusy; private set => SetBusy(value); }
    public bool HasProfile => _profile is not null;
    public bool HasError => !string.IsNullOrWhiteSpace(_errorText);
    public bool ShowStatus => IsBusy || HasError || !HasProfile;
    public string StatusText => StatusMessage();
    public string TitleText => Text(_profile?.CompanyName, "Company details");
    public string SymbolText => Text(_profile?.Symbol, _symbol);
    public string SubtitleText => Subtitle(_profile);
    public string PriceText => Money(_profile?.Price, _profile?.Currency);
    public string ChangeText => Change(_profile);
    public string DescriptionText => Text(_profile?.Description, "No company description available.");
    public string WebsiteText => Text(_profile?.Website, "No website listed.");

    public async Task LoadAsync(string? symbol)
    {
        var cleanSymbol = CleanSymbol(symbol);
        if (string.IsNullOrWhiteSpace(cleanSymbol))
        {
            ApplyInvalidSymbol();
            return;
        }
        await RunLoadAsync(cleanSymbol);
    }

    private async Task RunLoadAsync(string symbol)
    {
        _symbol = symbol;
        ClearProfile();
        IsBusy = true;
        try
        {
            ApplyResult(await _apiClient.GetCompanyProfileAsync(symbol, CancellationToken.None));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyResult(ApiClientResult<CompanyProfile> result)
    {
        if (!result.Ok || result.Data is null)
        {
            ApplyFailure(result.Message);
            return;
        }
        ApplyProfile(result.Data);
    }

    private void ApplyProfile(CompanyProfile profile)
    {
        _errorText = string.Empty;
        SetProfile(profile);
        Replace(Metrics, MetricsFor(profile));
        Replace(Facts, FactsFor(profile));
    }

    private void ApplyFailure(string message)
    {
        _errorText = message;
        ClearProfile();
    }

    private void ApplyInvalidSymbol()
    {
        _symbol = string.Empty;
        ApplyFailure("Open a stock from Search or Portfolio.");
    }

    private void ClearProfile()
    {
        SetProfile(null);
        Replace(Metrics, []);
        Replace(Facts, []);
    }

    private void SetProfile(CompanyProfile? profile)
    {
        _profile = profile;
        foreach (var name in ProfilePropertyNames)
        {
            OnPropertyChanged(name);
        }
    }

    private void SetBusy(bool value)
    {
        if (SetField(ref _isBusy, value, nameof(IsBusy)))
        {
            OnPropertyChanged(nameof(ShowStatus));
            OnPropertyChanged(nameof(StatusText));
        }
    }

    private string StatusMessage()
    {
        if (IsBusy)
        {
            return $"Loading {_symbol}...";
        }
        return HasError ? _errorText : "Open a stock from Search or Portfolio.";
    }

    private static ProfileMetric[] MetricsFor(CompanyProfile profile)
    {
        return [
            new("Market cap", Money(profile.MarketCap, profile.Currency)),
            new("Volume", Number(profile.Volume)),
            new("Avg volume", Number(profile.AverageVolume)),
            new("Dividend", Money(profile.LastDividend, profile.Currency)),
            new("Beta", DecimalText(profile.Beta)),
            new("Range", Text(profile.Range, "Not available"))
        ];
    }

    private static ProfileMetric[] FactsFor(CompanyProfile profile)
    {
        return [
            new("Exchange", Text(profile.ExchangeFullName, profile.Exchange ?? "Not available")),
            new("Industry", Text(profile.Industry, "Not available")),
            new("CEO", Text(profile.Ceo, "Not available")),
            new("Employees", Text(profile.FullTimeEmployees, "Not available")),
            new("IPO date", Text(profile.IpoDate, "Not available")),
            new("Trading", profile.IsActivelyTrading == true ? "Active" : "Not confirmed")
        ];
    }

    private static void Replace<T>(ObservableCollection<T> collection, IEnumerable<T> values)
    {
        collection.Clear();
        foreach (var value in values)
        {
            collection.Add(value);
        }
    }

    private static string Subtitle(CompanyProfile? profile)
    {
        var values = new[] { profile?.Sector, profile?.Country, profile?.Exchange };
        var parts = values.Where(value => !string.IsNullOrWhiteSpace(value));
        return string.Join(" | ", parts);
    }

    private static string Change(CompanyProfile? profile)
    {
        var amount = DecimalText(profile?.Change);
        var percent = DecimalText(profile?.ChangePercentage);
        return $"{amount} ({percent}%)";
    }

    private static string Money(decimal? value, string? currency)
    {
        return value is null ? "Not available" : $"{Currency(currency)}{value:N2}";
    }

    private static string Money(long? value, string? currency)
    {
        return value is null ? "Not available" : $"{Currency(currency)}{value:N0}";
    }

    private static string Currency(string? currency)
    {
        return string.IsNullOrWhiteSpace(currency) ? "$" : $"{currency.Trim()} ";
    }

    private static string Number(long? value)
    {
        return value is null ? "Not available" : $"{value:N0}";
    }

    private static string DecimalText(decimal? value)
    {
        return value is null ? "Not available" : $"{value:N2}";
    }

    private static string Text(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    private static string CleanSymbol(string? symbol)
    {
        return symbol?.Trim().ToUpperInvariant() ?? string.Empty;
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
