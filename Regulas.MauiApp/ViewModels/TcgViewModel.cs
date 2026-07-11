using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Regulas.MauiApp.Models;
using Regulas.MauiApp.Services;

namespace Regulas.MauiApp.ViewModels;

// Records TCG card prices by hand (no provider feed yet) and lists what is
// stored, with the source metadata so sold/graded prices never mix silently.
public sealed class TcgViewModel : INotifyPropertyChanged
{
    private readonly IRegulasApiClient _apiClient;
    private readonly AuthSession _authSession;
    private readonly Command _loadCommand;
    private readonly Command _saveCommand;
    private string _condition = string.Empty;
    private string _currency = "USD";
    private string _category = "Pokemon";
    private string _date = Today();
    private string _grade = string.Empty;
    private bool _isAuthenticated;
    private bool _isBusy;
    private string _messageText = "Sign in to record card prices.";
    private string _name = string.Empty;
    private string _price = string.Empty;
    private string _priceType = "Sold";
    private string _symbol = string.Empty;

    public TcgViewModel(IRegulasApiClient apiClient, AuthSession authSession)
    {
        _apiClient = apiClient;
        _authSession = authSession;
        _saveCommand = new Command(async () => await SaveAsync(), () => CanSave);
        _loadCommand = new Command(async () => await LoadStoredAsync(), () => CanLoad);
        OpenAccountCommand = new Command(async () => await NavigationRoutes.OpenAccountAsync());
        _authSession.PropertyChanged += (_, _) => SyncAuthState();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<TcgPointRow> Points { get; } = [];
    public IReadOnlyList<string> TcgGames { get; } = ["Pokemon", "Magic", "One Piece"];
    public IReadOnlyList<string> PriceTypes { get; } = ["Sold", "Listed", "Market"];
    public ICommand SaveCommand => _saveCommand;
    public ICommand LoadCommand => _loadCommand;
    public ICommand OpenAccountCommand { get; }
    public string Symbol { get => _symbol; set => SetInput(ref _symbol, value.ToUpperInvariant(), nameof(Symbol)); }
    public string Name { get => _name; set => SetInput(ref _name, value, nameof(Name)); }
    public string Category { get => _category; set => SetInput(ref _category, value, nameof(Category)); }
    public string Date { get => _date; set => SetInput(ref _date, value, nameof(Date)); }
    public string Price { get => _price; set => SetInput(ref _price, value, nameof(Price)); }
    public string PriceType { get => _priceType; set => SetInput(ref _priceType, value, nameof(PriceType)); }
    public string Condition { get => _condition; set => SetInput(ref _condition, value, nameof(Condition)); }
    public string Grade { get => _grade; set => SetInput(ref _grade, value, nameof(Grade)); }
    public string Currency { get => _currency; set => SetInput(ref _currency, value, nameof(Currency)); }
    public string MessageText { get => _messageText; private set => SetMessage(value); }
    public bool IsBusy { get => _isBusy; private set => SetBusy(value); }
    public bool IsAuthenticated { get => _isAuthenticated; private set => SetAuthenticated(value); }
    public bool IsAnonymous => !IsAuthenticated;
    public bool HasPoints => Points.Count > 0;
    public bool ShowMessage => !IsBusy && !string.IsNullOrWhiteSpace(MessageText);
    public bool CanSave => IsAuthenticated && !IsBusy && HasValidEntry();
    public bool CanLoad => !IsBusy && !string.IsNullOrWhiteSpace(Symbol);

    public async Task LoadAsync()
    {
        await _authSession.RefreshAsync(CancellationToken.None);
        SyncAuthState();
    }

    private async Task SaveAsync()
    {
        if (!CanSave)
        {
            return;
        }
        await RunBusyAsync(async () => await SaveThenReloadAsync());
    }

    private async Task LoadStoredAsync()
    {
        if (!CanLoad)
        {
            return;
        }
        await RunBusyAsync(async () => ApplyHistory(await FetchHistoryAsync(), string.Empty));
    }

    // Saving always re-reads storage so the list shows saved truth, not hope.
    private async Task SaveThenReloadAsync()
    {
        var saved = await _apiClient.RecordManualPriceAsync(Symbol.Trim(), ToRequest(), CancellationToken.None);
        if (!saved.Ok || saved.Data is null)
        {
            ApplyFailure(saved.Message);
            return;
        }
        ApplyHistory(await FetchHistoryAsync(), SaveMessage(saved.Data));
    }

    private Task<ApiClientResult<PriceHistoryResponse>> FetchHistoryAsync()
    {
        return _apiClient.GetPriceHistoryAsync(Symbol.Trim(), "TcgCard", 365, CancellationToken.None);
    }

    private void ApplyHistory(ApiClientResult<PriceHistoryResponse> result, string message)
    {
        if (!result.Ok || result.Data is null)
        {
            ApplyFailure(result.Message);
            return;
        }
        ReplacePoints(result.Data.Points);
        MessageText = Points.Count == 0 ? "No stored prices for this card yet." : message;
    }

    private void ApplyFailure(string message)
    {
        ReplacePoints([]);
        MessageText = message;
    }

    private ManualPriceRequest ToRequest()
    {
        return new ManualPriceRequest(
            ParseDate(Date), ParsePrice(Price), PriceType,
            BlankToNull(Condition), BlankToNull(Grade), BlankToNull(Currency), BlankToNull(Name), BlankToNull(Category)
        );
    }

    private void ReplacePoints(IEnumerable<PricePoint> points)
    {
        Points.Clear();
        foreach (var point in points.Reverse().Take(30))
        {
            Points.Add(Row(point));
        }
        OnPropertyChanged(nameof(HasPoints));
    }

    private static TcgPointRow Row(PricePoint point)
    {
        return new TcgPointRow(point.Date.ToString("yyyy-MM-dd"), $"{point.Close:N2}", Meta(point), $"Source: {point.Source}");
    }

    private static string Meta(PricePoint point)
    {
        var parts = new[] { point.PriceType, point.CardCondition, point.Grade, point.Currency };
        return string.Join(" · ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
    }

    private static string SaveMessage(PriceCaptureResult result)
    {
        return result.Captured > 0
            ? $"{result.Symbol} price saved (source {result.Source})."
            : $"{result.Symbol} already has a price for that date; nothing was added.";
    }

    private bool HasValidEntry()
    {
        return !string.IsNullOrWhiteSpace(Symbol) && ParsePrice(Price) > 0 && ParseDate(Date) != default;
    }

    private static decimal ParsePrice(string value)
    {
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var price) ? price : 0m;
    }

    private static DateOnly ParseDate(string value)
    {
        return DateOnly.TryParse(value, CultureInfo.InvariantCulture, out var date) ? date : default;
    }

    private static string? BlankToNull(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string Today()
    {
        return DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd");
    }

    private async Task RunBusyAsync(Func<Task> action)
    {
        IsBusy = true;
        try
        {
            await action();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void SyncAuthState()
    {
        IsAuthenticated = _authSession.IsAuthenticated;
        if (IsAnonymous)
        {
            ApplySignedOutState();
        }
    }

    private void ApplySignedOutState()
    {
        ReplacePoints([]);
        MessageText = "Sign in to record card prices.";
    }

    private void SetInput<T>(ref T field, T value, string name)
    {
        if (SetField(ref field, value, name))
        {
            RefreshCommands();
        }
    }

    private void SetBusy(bool value)
    {
        if (SetField(ref _isBusy, value, nameof(IsBusy)))
        {
            RefreshCommands();
        }
    }

    private void SetAuthenticated(bool value)
    {
        if (SetField(ref _isAuthenticated, value, nameof(IsAuthenticated)))
        {
            OnPropertyChanged(nameof(IsAnonymous));
            RefreshCommands();
        }
    }

    private void SetMessage(string value)
    {
        if (SetField(ref _messageText, value, nameof(MessageText)))
        {
            OnPropertyChanged(nameof(ShowMessage));
        }
    }

    private void RefreshCommands()
    {
        OnPropertyChanged(nameof(CanSave));
        OnPropertyChanged(nameof(CanLoad));
        OnPropertyChanged(nameof(ShowMessage));
        _saveCommand.ChangeCanExecute();
        _loadCommand.ChangeCanExecute();
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
