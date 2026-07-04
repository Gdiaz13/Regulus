using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Regulas.MauiApp.Models;
using Regulas.MauiApp.Services;

namespace Regulas.MauiApp.ViewModels;

public sealed class HomeViewModel : INotifyPropertyChanged
{
    private readonly IRegulasApiClient _apiClient;
    private readonly AuthSession _authSession;
    private readonly Command _openAccountCommand;
    private readonly Command<PortfolioStock> _openStockCommand;
    private readonly Command _refreshCommand;
    private string _accountText = "Sign in to load your portfolio.";
    private string _databaseText = "Database not checked";
    private string _errorText = string.Empty;
    private bool _isBusy;
    private bool _isAuthenticated;
    private string _marketDataText = "Market data key not checked";
    private string _statusText = "Connect to Regulas.Api";

    public HomeViewModel(IRegulasApiClient apiClient, AuthSession authSession)
    {
        _apiClient = apiClient;
        _authSession = authSession;
        _openAccountCommand = new Command(async () => await NavigationRoutes.OpenAccountAsync());
        _openStockCommand = new Command<PortfolioStock>(async stock => await OpenStockAsync(stock));
        _refreshCommand = new Command(async () => await LoadAsync(), () => CanRefresh);
        _authSession.PropertyChanged += (_, _) => SyncAuthState();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<PortfolioStock> Stocks { get; } = [];

    public ICommand RefreshCommand => _refreshCommand;

    public ICommand OpenAccountCommand => _openAccountCommand;

    public ICommand OpenStockCommand => _openStockCommand;

    public string StatusText { get => _statusText; private set => SetField(ref _statusText, value); }

    public string AccountText { get => _accountText; private set => SetField(ref _accountText, value); }

    public string DatabaseText { get => _databaseText; private set => SetField(ref _databaseText, value); }

    public string MarketDataText { get => _marketDataText; private set => SetField(ref _marketDataText, value); }

    public string ErrorText { get => _errorText; private set => SetErrorText(value); }

    public bool IsBusy { get => _isBusy; private set => SetBusy(value); }

    public bool CanRefresh => !IsBusy;

    public bool IsAuthenticated { get => _isAuthenticated; private set => SetAuthenticated(value); }

    public bool IsAnonymous => !IsAuthenticated;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorText);

    public async Task LoadAsync()
    {
        if (IsBusy)
        {
            return;
        }
        await RunLoadAsync();
    }

    // Wraps the load so IsBusy is always reset, even when a request throws.
    private async Task RunLoadAsync()
    {
        IsBusy = true;
        try
        {
            await RefreshCoreAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RefreshCoreAsync()
    {
        ErrorText = string.Empty;
        await _authSession.RefreshAsync(CancellationToken.None);
        SyncAuthState();
        await LoadHealthAsync();
        if (!IsAuthenticated)
        {
            ReplaceStocks([]);
            return;
        }
        await LoadStocksAsync();
    }

    private async Task LoadHealthAsync()
    {
        var result = await _apiClient.GetHealthAsync(CancellationToken.None);
        if (!result.Ok || result.Data is null)
        {
            ApplyHealthFailure(result.Message);
            return;
        }
        ApplyHealth(result.Data);
    }

    private async Task LoadStocksAsync()
    {
        var result = await _apiClient.GetPortfolioStocksAsync(CancellationToken.None);
        if (!result.Ok || result.Data is null)
        {
            ErrorText = result.Message;
            return;
        }
        ReplaceStocks(result.Data);
    }

    private void ApplyHealth(ApiHealth health)
    {
        StatusText = $"API status: {health.Status}";
        DatabaseText = health.DatabaseAvailable ? "PostgreSQL connected" : "PostgreSQL unavailable";
        MarketDataText = health.MarketDataConfigured ? "Market data configured" : "Market data key missing";
    }

    private void ApplyHealthFailure(string message)
    {
        StatusText = "API unavailable";
        DatabaseText = "Database not checked";
        MarketDataText = "Market data key not checked";
        ErrorText = message;
    }

    private void ReplaceStocks(IEnumerable<PortfolioStock> stocks)
    {
        Stocks.Clear();
        foreach (var stock in stocks)
        {
            Stocks.Add(stock);
        }
    }

    private static async Task OpenStockAsync(PortfolioStock? stock)
    {
        if (stock is not null)
        {
            await NavigationRoutes.OpenStockDetailAsync(stock.Symbol);
        }
    }

    private void SyncAuthState()
    {
        var user = _authSession.CurrentUser;
        IsAuthenticated = user is not null;
        AccountText = user is null ? "Sign in to load your portfolio." : $"Signed in as {user.DisplayName}";
    }

    private void SetAuthenticated(bool value)
    {
        if (SetField(ref _isAuthenticated, value, nameof(IsAuthenticated)))
        {
            OnPropertyChanged(nameof(IsAnonymous));
        }
    }

    private void SetBusy(bool value)
    {
        if (!SetField(ref _isBusy, value, nameof(IsBusy)))
        {
            return;
        }
        OnPropertyChanged(nameof(CanRefresh));
        _refreshCommand.ChangeCanExecute();
    }

    private void SetErrorText(string value)
    {
        if (!SetField(ref _errorText, value, nameof(ErrorText)))
        {
            return;
        }
        OnPropertyChanged(nameof(HasError));
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
