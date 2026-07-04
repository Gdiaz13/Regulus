using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Regulas.MauiApp.Models;
using Regulas.MauiApp.Services;

namespace Regulas.MauiApp.ViewModels;

public sealed class PriceHistoryViewModel : INotifyPropertyChanged
{
    private static readonly string[] HistoryPropertyNames = [
        nameof(HasHistory), nameof(ShowMessage), nameof(SummaryText), nameof(LatestCloseText), nameof(SourceText)
    ];
    private readonly IRegulasApiClient _apiClient;
    private readonly Command _captureCommand;
    private readonly Command _loadCommand;
    private string _assetType = "Stock";
    private PriceHistoryResponse? _history;
    private bool _isBusy;
    private string _messageText = "Load stored history or capture it from the provider.";
    private string _symbol = string.Empty;
    private string _take = "365";

    public PriceHistoryViewModel(IRegulasApiClient apiClient)
    {
        _apiClient = apiClient;
        _loadCommand = new Command(async () => await LoadAsync(), () => CanRun);
        _captureCommand = new Command(async () => await CaptureAsync(), () => CanRun);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<PricePointRow> Points { get; } = [];
    public IReadOnlyList<string> AssetTypes { get; } = ["Stock", "Etf", "TcgCard", "Crypto", "Collectible"];
    public IReadOnlyList<string> TakeOptions { get; } = ["30", "90", "365", "1000"];
    public ICommand LoadCommand => _loadCommand;
    public ICommand CaptureCommand => _captureCommand;
    public string Symbol { get => _symbol; set => SetSymbol(value); }
    public string AssetType { get => _assetType; set => SetField(ref _assetType, value); }
    public string Take { get => _take; set => SetField(ref _take, value); }
    public string MessageText { get => _messageText; private set => SetMessage(value); }
    public bool IsBusy { get => _isBusy; private set => SetBusy(value); }
    public bool CanRun => !IsBusy && !string.IsNullOrWhiteSpace(Symbol);
    public bool HasHistory => _history is not null && Points.Count > 0;
    public bool ShowMessage => !IsBusy && !string.IsNullOrWhiteSpace(MessageText);
    public string SummaryText => Summary(_history);
    public string LatestCloseText => LatestClose(_history);
    public string SourceText => Source(_history);

    public void ApplySymbol(string? symbol)
    {
        Symbol = Clean(symbol);
    }

    private async Task LoadAsync()
    {
        if (!CanRun)
        {
            return;
        }
        await RunLoadAsync();
    }

    private async Task CaptureAsync()
    {
        if (!CanRun)
        {
            return;
        }
        await RunCaptureAsync();
    }

    private async Task RunLoadAsync()
    {
        await RunBusyAsync(async () => ApplyHistory(await _apiClient.GetPriceHistoryAsync(Clean(Symbol), AssetType, TakeNumber(), CancellationToken.None)));
    }

    private async Task RunCaptureAsync()
    {
        await RunBusyAsync(async () => await CaptureThenLoadAsync());
    }

    private async Task CaptureThenLoadAsync()
    {
        var captured = await _apiClient.CapturePriceHistoryAsync(Clean(Symbol), AssetType, CancellationToken.None);
        if (!captured.Ok || captured.Data is null)
        {
            ApplyFailure(captured.Message);
            return;
        }
        await LoadAfterCaptureAsync(captured.Data);
    }

    private async Task LoadAfterCaptureAsync(PriceCaptureResult captured)
    {
        var loaded = await _apiClient.GetPriceHistoryAsync(captured.Symbol, captured.AssetType, TakeNumber(), CancellationToken.None);
        ApplyHistory(loaded);
        MessageText = CaptureMessage(captured);
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

    private void ApplyHistory(ApiClientResult<PriceHistoryResponse> result)
    {
        if (!result.Ok || result.Data is null)
        {
            ApplyFailure(result.Message);
            return;
        }
        ApplyLoadedHistory(result.Data);
    }

    private void ApplyLoadedHistory(PriceHistoryResponse history)
    {
        _history = history;
        ReplacePoints(history.Points);
        MessageText = history.Points.Count == 0 ? "No stored history yet. Capture from the provider first." : string.Empty;
        NotifyHistoryChanged();
    }

    private void ApplyFailure(string message)
    {
        _history = null;
        ReplacePoints([]);
        MessageText = message;
        NotifyHistoryChanged();
    }

    private void ReplacePoints(IEnumerable<PricePoint> points)
    {
        Points.Clear();
        foreach (var point in points.Reverse().Take(30))
        {
            Points.Add(Row(point));
        }
    }

    private static PricePointRow Row(PricePoint point)
    {
        return new PricePointRow(point.Date.ToString("yyyy-MM-dd"), Money(point.Close), Range(point), $"{point.Volume:N0}", point.Source);
    }

    private static string Summary(PriceHistoryResponse? history)
    {
        return history is null ? "No history loaded." : $"{history.Symbol} | {history.Count} days | {history.AssetType}";
    }

    private static string LatestClose(PriceHistoryResponse? history)
    {
        var latest = history?.Points.LastOrDefault();
        return latest is null ? "Latest close not available." : $"Latest close {Money(latest.Close)}";
    }

    private static string Source(PriceHistoryResponse? history)
    {
        var latest = history?.Points.LastOrDefault();
        return latest is null ? "Source not available." : $"Source {latest.Source}";
    }

    private static string Range(PricePoint point)
    {
        return $"{Money(point.Low)} - {Money(point.High)}";
    }

    private static string Money(decimal value)
    {
        return $"${value:N2}";
    }

    private static string CaptureMessage(PriceCaptureResult result)
    {
        return $"Captured {result.Captured:N0}, skipped {result.Skipped:N0} from {result.Source}.";
    }

    private int TakeNumber()
    {
        return int.TryParse(Take, out var parsed) ? parsed : 365;
    }

    private static string Clean(string? value)
    {
        return value?.Trim().ToUpperInvariant() ?? string.Empty;
    }

    private void SetSymbol(string value)
    {
        if (SetField(ref _symbol, value, nameof(Symbol)))
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

    private void SetMessage(string value)
    {
        if (SetField(ref _messageText, value, nameof(MessageText)))
        {
            OnPropertyChanged(nameof(ShowMessage));
        }
    }

    private void RefreshCommands()
    {
        OnPropertyChanged(nameof(CanRun));
        OnPropertyChanged(nameof(ShowMessage));
        _loadCommand.ChangeCanExecute();
        _captureCommand.ChangeCanExecute();
    }

    private void NotifyHistoryChanged()
    {
        foreach (var name in HistoryPropertyNames)
        {
            OnPropertyChanged(name);
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
