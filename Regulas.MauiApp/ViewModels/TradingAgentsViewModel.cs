using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Regulas.MauiApp.Models;
using Regulas.MauiApp.Services;

namespace Regulas.MauiApp.ViewModels;

// Runs StockTradingAgents research for one symbol through the gateway. The
// TradingAgents service stays separate; the app only sees the clean response.
public sealed class TradingAgentsViewModel : INotifyPropertyChanged
{
    private readonly IRegulasApiClient _apiClient;
    private readonly Command _rerunCommand;
    private string _companyName = string.Empty;
    private string _errorText = string.Empty;
    private bool _isBusy;
    private decimal _price;
    private StockTradingAgentsResult? _result;
    private string _symbol = string.Empty;

    public TradingAgentsViewModel(IRegulasApiClient apiClient)
    {
        _apiClient = apiClient;
        _rerunCommand = new Command(async () => await RunAnalysisAsync(), () => CanRun);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand RerunCommand => _rerunCommand;
    public string Symbol { get => _symbol; private set => SetField(ref _symbol, value); }
    public bool IsBusy { get => _isBusy; private set => SetBusy(value); }
    public string ErrorText => _errorText;
    public bool HasError => !IsBusy && !string.IsNullOrWhiteSpace(_errorText);
    public StockTradingAgentsResult? Result => _result;
    public bool HasResult => _result is not null;
    public bool CanRun => !IsBusy && !string.IsNullOrWhiteSpace(Symbol) && _price > 0;

    public async Task LoadAsync(string symbol, decimal price, string companyName)
    {
        if (IsBusy || string.IsNullOrWhiteSpace(symbol))
        {
            return;
        }
        Apply(symbol, price, companyName);
        await RunAnalysisAsync();
    }

    private void Apply(string symbol, decimal price, string companyName)
    {
        Symbol = symbol.Trim().ToUpperInvariant();
        _price = price;
        _companyName = companyName;
    }

    private async Task RunAnalysisAsync()
    {
        if (!CanRun)
        {
            return;
        }
        await ExecuteAnalysisAsync();
    }

    // Wraps the run so IsBusy always resets, even when the request throws.
    private async Task ExecuteAnalysisAsync()
    {
        IsBusy = true;
        try
        {
            ApplyResult(await _apiClient.AnalyzeStockAsync(ToRequest(), CancellationToken.None));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private StockTradingAgentsRequest ToRequest()
    {
        return new StockTradingAgentsRequest(Symbol, _companyName, _price, null);
    }

    private void ApplyResult(ApiClientResult<StockTradingAgentsResult> result)
    {
        _result = result.Ok ? result.Data : null;
        _errorText = result.Ok ? string.Empty : result.Message;
        OnPropertyChanged(string.Empty);
    }

    private void SetBusy(bool value)
    {
        if (SetField(ref _isBusy, value, nameof(IsBusy)))
        {
            OnPropertyChanged(nameof(CanRun));
            _rerunCommand.ChangeCanExecute();
        }
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
