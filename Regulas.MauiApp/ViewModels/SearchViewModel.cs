using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Regulas.MauiApp.Models;
using Regulas.MauiApp.Services;

namespace Regulas.MauiApp.ViewModels;

public sealed class SearchViewModel : INotifyPropertyChanged
{
    private readonly Command<CompanySearchResult> _addCommand;
    private readonly IRegulasApiClient _apiClient;
    private readonly AuthSession _authSession;
    private readonly Command<CompanySearchResult> _detailCommand;
    private readonly Command _searchCommand;
    private bool _hasResults;
    private bool _isAdding;
    private bool _isAuthenticated;
    private bool _isBusy;
    private string _messageText = "Sign in, then search companies to add them to your portfolio.";
    private string _query = string.Empty;

    public SearchViewModel(IRegulasApiClient apiClient, AuthSession authSession)
    {
        _apiClient = apiClient;
        _authSession = authSession;
        _searchCommand = new Command(async () => await SearchAsync(), () => CanSearch);
        _addCommand = new Command<CompanySearchResult>(async c => await AddAsync(c), CanAdd);
        _detailCommand = new Command<CompanySearchResult>(async c => await OpenDetailAsync(c));
        OpenAccountCommand = new Command(async () => await NavigationRoutes.OpenAccountAsync());
        _authSession.PropertyChanged += (_, _) => SyncAuthState();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<CompanySearchResult> Results { get; } = [];
    public ICommand SearchCommand => _searchCommand;
    public ICommand AddToPortfolioCommand => _addCommand;
    public ICommand OpenDetailCommand => _detailCommand;
    public ICommand OpenAccountCommand { get; }
    public string Query { get => _query; set => SetQuery(value); }
    public string MessageText { get => _messageText; private set => SetMessage(value); }
    public bool IsBusy { get => _isBusy; private set => SetBusy(value); }
    public bool IsAdding { get => _isAdding; private set => SetAdding(value); }
    public bool IsAuthenticated { get => _isAuthenticated; private set => SetAuthenticated(value); }
    public bool IsAnonymous => !IsAuthenticated;
    public bool HasResults { get => _hasResults; private set => SetField(ref _hasResults, value); }
    public bool ShowMessage => !IsBusy && !string.IsNullOrWhiteSpace(MessageText);
    public bool CanSearch => !IsBusy && IsAuthenticated && !string.IsNullOrWhiteSpace(Query);

    public async Task LoadAsync()
    {
        await _authSession.RefreshAsync(CancellationToken.None);
        SyncAuthState();
    }

    private async Task SearchAsync()
    {
        if (!CanSearch)
        {
            return;
        }
        await RunSearchAsync();
    }

    private async Task RunSearchAsync()
    {
        IsBusy = true;
        try
        {
            ApplySearchResult(await _apiClient.SearchCompaniesAsync(Query, CancellationToken.None));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplySearchResult(ApiClientResult<IReadOnlyList<CompanySearchResult>> result)
    {
        if (!result.Ok || result.Data is null)
        {
            ApplyFailure(result.Message);
            return;
        }
        ApplyResults(result.Data);
    }

    private void ApplyResults(IReadOnlyList<CompanySearchResult> results)
    {
        ReplaceResults(results);
        MessageText = results.Count == 0 ? $"No companies found for {Query.Trim()}." : string.Empty;
    }

    private async Task AddAsync(CompanySearchResult? company)
    {
        if (company is null || !CanAdd(company))
        {
            return;
        }
        await RunAddAsync(company);
    }

    private async Task OpenDetailAsync(CompanySearchResult? company)
    {
        if (company is not null)
        {
            await NavigationRoutes.OpenStockDetailAsync(company.Symbol);
        }
    }

    private async Task RunAddAsync(CompanySearchResult company)
    {
        IsAdding = true;
        try
        {
            ApplyAddResult(company, await _apiClient.AddPortfolioStockAsync(ToPortfolioRequest(company), CancellationToken.None));
        }
        finally
        {
            IsAdding = false;
        }
    }

    private void ApplyAddResult(CompanySearchResult company, ApiClientResult<PortfolioStock> result)
    {
        MessageText = result.Ok ? $"{company.Symbol} added to your portfolio." : result.Message;
    }

    private static CreatePortfolioStockRequest ToPortfolioRequest(CompanySearchResult company)
    {
        return new CreatePortfolioStockRequest(company.Symbol, company.Name, null, null, company.ExchangeFullName, null);
    }

    private bool CanAdd(CompanySearchResult? company)
    {
        return company is not null && IsAuthenticated && !IsAdding;
    }

    private void ApplyFailure(string message)
    {
        ReplaceResults([]);
        MessageText = message;
    }

    private void ReplaceResults(IEnumerable<CompanySearchResult> results)
    {
        Results.Clear();
        foreach (var result in results)
        {
            Results.Add(result);
        }
        HasResults = Results.Count > 0;
    }

    private void SyncAuthState()
    {
        IsAuthenticated = _authSession.IsAuthenticated;
        if (IsAnonymous)
        {
            ApplySignedOutState();
            return;
        }
        if (!HasResults && string.IsNullOrWhiteSpace(Query))
        {
            MessageText = "Search companies by name or ticker.";
        }
    }

    private void ApplySignedOutState()
    {
        ReplaceResults([]);
        MessageText = "Sign in, then search companies to add them to your portfolio.";
    }

    private void SetQuery(string value)
    {
        if (SetField(ref _query, value, nameof(Query)) && string.IsNullOrWhiteSpace(value))
        {
            ApplyIdleState();
        }
        RefreshSearchState();
    }

    private void ApplyIdleState()
    {
        ReplaceResults([]);
        MessageText = IsAuthenticated ? "Search companies by name or ticker." : MessageText;
    }

    private void SetBusy(bool value)
    {
        if (SetField(ref _isBusy, value, nameof(IsBusy)))
        {
            RefreshSearchState();
        }
    }

    private void SetAdding(bool value)
    {
        if (SetField(ref _isAdding, value, nameof(IsAdding)))
        {
            _addCommand.ChangeCanExecute();
        }
    }

    private void SetAuthenticated(bool value)
    {
        if (!SetField(ref _isAuthenticated, value, nameof(IsAuthenticated)))
        {
            return;
        }
        OnPropertyChanged(nameof(IsAnonymous));
        RefreshSearchState();
    }

    private void SetMessage(string value)
    {
        if (SetField(ref _messageText, value, nameof(MessageText)))
        {
            OnPropertyChanged(nameof(ShowMessage));
        }
    }

    private void RefreshSearchState()
    {
        OnPropertyChanged(nameof(CanSearch));
        OnPropertyChanged(nameof(ShowMessage));
        _searchCommand.ChangeCanExecute();
        _addCommand.ChangeCanExecute();
        _detailCommand.ChangeCanExecute();
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
