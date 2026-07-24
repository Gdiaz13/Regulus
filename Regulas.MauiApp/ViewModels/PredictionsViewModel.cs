using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Regulas.MauiApp.Models;
using Regulas.MauiApp.Services;

namespace Regulas.MauiApp.ViewModels;

public sealed class PredictionsViewModel : INotifyPropertyChanged
{
    private static readonly string[] OverviewProperties = [
        nameof(HasPredictionResults), nameof(ShowPredictionSummary), nameof(SummaryText), nameof(ModelText)
    ];
    private readonly IRegulasApiClient _apiClient;
    private readonly AuthSession _authSession;
    private readonly Command _accuracyCommand;
    private readonly Command _historyCommand;
    private readonly Command<PredictAssetRow> _removeCommand;
    private readonly Command _runCommand;
    private readonly Command _stageCommand;
    private string _accuracyMessageText = "Sign in to load model accuracy.";
    private string _assetName = string.Empty;
    private string _assetType = "Stock";
    private string _category = string.Empty;
    private string _currentPrice = string.Empty;
    private string _historyMessageText = "Sign in to load saved prediction history.";
    private bool _isAccuracyBusy;
    private bool _isAuthenticated;
    private bool _isBusy;
    private bool _isHistoryBusy;
    private string _messageText = "Sign in to run predictions.";
    private AiOverview? _overview;
    private string _symbol = string.Empty;
    private string _timeHorizonDays = "90";

    public PredictionsViewModel(IRegulasApiClient apiClient, AuthSession authSession)
    {
        _apiClient = apiClient;
        _authSession = authSession;
        _stageCommand = new Command(StageAsset, () => CanStage);
        _runCommand = new Command(async () => await RunPredictionAsync(), () => CanRun);
        _accuracyCommand = new Command(async () => await LoadAccuracyAsync(), () => CanRefreshAccuracy);
        _historyCommand = new Command(async () => await LoadHistoryAsync(), () => CanRefreshHistory);
        _removeCommand = new Command<PredictAssetRow>(RemoveAsset);
        OpenAccountCommand = new Command(async () => await NavigationRoutes.OpenAccountAsync());
        _authSession.PropertyChanged += (_, _) => SyncAuthState();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<PredictAssetRow> StagedAssets { get; } = [];
    public ObservableCollection<PredictionResultRow> Predictions { get; } = [];
    public ObservableCollection<ModelAccuracySummaryRow> AccuracySummaries { get; } = [];
    public ObservableCollection<PredictionHistoryRow> History { get; } = [];
    public IReadOnlyList<string> AssetTypes { get; } = ["Stock", "Etf", "TcgCard", "Crypto", "Collectible"];
    public ICommand StageCommand => _stageCommand;
    public ICommand RunCommand => _runCommand;
    public ICommand RefreshAccuracyCommand => _accuracyCommand;
    public ICommand RefreshHistoryCommand => _historyCommand;
    public ICommand RemoveStagedCommand => _removeCommand;
    public ICommand OpenAccountCommand { get; }
    public string Symbol { get => _symbol; set => SetInput(ref _symbol, Upper(value), nameof(Symbol)); }
    public string AssetName { get => _assetName; set => SetInput(ref _assetName, value, nameof(AssetName)); }
    public string AssetType { get => _assetType; set => SetInput(ref _assetType, value, nameof(AssetType)); }
    public string Category { get => _category; set => SetInput(ref _category, value, nameof(Category)); }
    public string CurrentPrice { get => _currentPrice; set => SetInput(ref _currentPrice, value, nameof(CurrentPrice)); }
    public string TimeHorizonDays { get => _timeHorizonDays; set => SetInput(ref _timeHorizonDays, value, nameof(TimeHorizonDays)); }
    public string MessageText { get => _messageText; private set => SetMessage(value); }
    public string AccuracyMessageText { get => _accuracyMessageText; private set => SetAccuracyMessage(value); }
    public string HistoryMessageText { get => _historyMessageText; private set => SetHistoryMessage(value); }
    public bool IsBusy { get => _isBusy; private set => SetBusy(value); }
    public bool IsAccuracyBusy { get => _isAccuracyBusy; private set => SetAccuracyBusy(value); }
    public bool IsHistoryBusy { get => _isHistoryBusy; private set => SetHistoryBusy(value); }
    public bool IsAuthenticated { get => _isAuthenticated; private set => SetAuthenticated(value); }
    public bool IsAnonymous => !IsAuthenticated;
    public bool HasStaged => StagedAssets.Count > 0;
    public bool HasPredictionResults => Predictions.Count > 0;
    public bool HasAccuracySummaries => AccuracySummaries.Count > 0;
    public bool ShowAccuracySummaries => IsAuthenticated && HasAccuracySummaries;
    public bool HasHistory => History.Count > 0;
    public bool ShowMessage => !IsBusy && !string.IsNullOrWhiteSpace(MessageText);
    public bool ShowAccuracyMessage => !IsAccuracyBusy && !string.IsNullOrWhiteSpace(AccuracyMessageText);
    public bool ShowHistoryMessage => !IsHistoryBusy && !string.IsNullOrWhiteSpace(HistoryMessageText);
    public bool ShowPredictionSummary => _overview is not null;
    public bool CanStage => IsAuthenticated && !IsBusy && HasValidAsset();
    public bool CanRun => IsAuthenticated && !IsBusy && HasStaged;
    public bool CanRefreshAccuracy => IsAuthenticated && !IsAccuracyBusy;
    public bool CanRefreshHistory => IsAuthenticated && !IsHistoryBusy;
    public string SummaryText => _overview?.Summary ?? "Run a prediction to see the RegulasCoreAI overview.";
    public string ModelText => _overview is null ? "No model response yet." : $"{_overview.ModelName} v{_overview.ModelVersion}";

    public async Task LoadAsync()
    {
        await _authSession.RefreshAsync(CancellationToken.None);
        SyncAuthState();
        if (IsAuthenticated)
        {
            await LoadAccuracyAsync();
            await LoadHistoryAsync();
        }
    }

    public void ApplySymbol(string? symbol)
    {
        var clean = Upper(symbol);
        if (string.IsNullOrWhiteSpace(clean))
        {
            return;
        }
        Symbol = clean;
        MessageText = "Enter a current price, then stage the asset.";
    }

    private void StageAsset()
    {
        if (!TryBuildAsset(out var asset))
        {
            return;
        }
        if (StageNewAsset(asset))
        {
            ClearAssetForm();
            RefreshStagedState();
        }
    }

    private async Task RunPredictionAsync()
    {
        if (!CanRun)
        {
            return;
        }
        await RunBusyAsync(async () => await ApplyPrediction(await _apiClient.PredictAsync(Requests(), CancellationToken.None)));
    }

    private async Task LoadHistoryAsync()
    {
        if (!CanRefreshHistory)
        {
            return;
        }
        await RunHistoryBusyAsync(async () => ApplyHistory(await _apiClient.GetPredictionHistoryAsync(10, CancellationToken.None)));
    }

    private async Task LoadAccuracyAsync()
    {
        if (!CanRefreshAccuracy)
        {
            return;
        }
        var generation = _authSession.Generation;
        await RunAccuracyBusyAsync(() => LoadAccuracyForSessionAsync(generation));
    }

    private async Task LoadAccuracyForSessionAsync(int generation)
    {
        var result = await _apiClient.GetPredictionAccuracySummaryAsync(CancellationToken.None);
        if (_authSession.Generation == generation)
        {
            ApplyAccuracy(result);
        }
    }

    private async Task ApplyPrediction(ApiClientResult<AiOverview> result)
    {
        if (!result.Ok || result.Data is null)
        {
            ApplyPredictionFailure(result.Message);
            return;
        }
        ApplyOverview(result.Data);
        await LoadHistoryAsync();
    }

    private void ApplyOverview(AiOverview overview)
    {
        _overview = overview;
        ReplacePredictions(ResultRows(overview));
        MessageText = Predictions.Count == 0 ? "No predictions came back." : "Prediction saved to history.";
        NotifyOverviewChanged();
    }

    private void ApplyPredictionFailure(string message)
    {
        _overview = null;
        ReplacePredictions([]);
        MessageText = message;
        NotifyOverviewChanged();
    }

    private void ApplyHistory(ApiClientResult<IReadOnlyList<PredictionHistoryItem>> result)
    {
        if (!result.Ok || result.Data is null)
        {
            ApplyHistoryFailure(result.Message);
            return;
        }
        ReplaceHistory(result.Data.Select(HistoryRow));
        HistoryMessageText = History.Count == 0 ? "No saved predictions yet." : string.Empty;
    }

    private void ApplyHistoryFailure(string message)
    {
        ReplaceHistory([]);
        HistoryMessageText = message;
    }

    private void ApplyAccuracy(ApiClientResult<IReadOnlyList<ModelAccuracySummary>> result)
    {
        if (!result.Ok || result.Data is null)
        {
            AccuracyMessageText = result.Message;
            return;
        }
        ReplaceAccuracy(result.Data.Select(AccuracyRow));
        AccuracyMessageText = AccuracySummaries.Count == 0 ? "No scored predictions yet." : string.Empty;
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

    private async Task RunHistoryBusyAsync(Func<Task> action)
    {
        IsHistoryBusy = true;
        try
        {
            await action();
        }
        finally
        {
            IsHistoryBusy = false;
        }
    }

    private async Task RunAccuracyBusyAsync(Func<Task> action)
    {
        IsAccuracyBusy = true;
        try
        {
            await action();
        }
        finally
        {
            IsAccuracyBusy = false;
        }
    }

    private bool TryBuildAsset(out PredictAssetRow asset)
    {
        asset = default!;
        if (!TryPrice(CurrentPrice, out var price) || !TryHorizon(TimeHorizonDays, out var horizon))
        {
            MessageText = "Use a positive price and horizon.";
            return false;
        }
        asset = NewAsset(price, horizon);
        return true;
    }

    private PredictAssetRow NewAsset(decimal price, int? horizon)
    {
        var symbol = Upper(Symbol);
        var name = Clean(AssetName);
        return new PredictAssetRow(symbol, name, AssetType, Clean(Category), price, horizon, DisplayName(symbol, name), Money(price), HorizonText(horizon));
    }

    private bool IsDuplicate(PredictAssetRow asset)
    {
        return StagedAssets.Any(item => item.Symbol == asset.Symbol && item.AssetType == asset.AssetType);
    }

    private bool StageNewAsset(PredictAssetRow asset)
    {
        if (IsDuplicate(asset))
        {
            MessageText = $"{asset.Symbol} is already staged.";
            return false;
        }
        StagedAssets.Add(asset);
        MessageText = $"{asset.Symbol} staged.";
        return true;
    }

    private IReadOnlyList<PredictAssetRequest> Requests()
    {
        return StagedAssets.Select(Request).ToList();
    }

    private static PredictAssetRequest Request(PredictAssetRow asset)
    {
        return new PredictAssetRequest(asset.Symbol, EmptyAsNull(asset.Name), asset.AssetType, EmptyAsNull(asset.Category), asset.CurrentPrice, asset.TimeHorizonDays);
    }

    private static IEnumerable<PredictionResultRow> ResultRows(AiOverview overview)
    {
        return overview.Categories.SelectMany(ResultRows);
    }

    private static IEnumerable<PredictionResultRow> ResultRows(AiCategoryPrediction category)
    {
        return category.Predictions.Select(prediction => ResultRow(category, prediction));
    }

    private static PredictionResultRow ResultRow(AiCategoryPrediction category, AiPrediction prediction)
    {
        return new PredictionResultRow(
            prediction.AssetId, prediction.AssetName, CategoryText(category, prediction), PriceText(prediction),
            Change(prediction.PredictedPercentChange), ScoreText(prediction), PredictionModelText(prediction),
            First(prediction.Reasons), First(prediction.Warnings), IsMock(prediction),
            prediction.Reasons.Count > 0, prediction.Warnings.Count > 0
        );
    }

    private static PredictionHistoryRow HistoryRow(PredictionHistoryItem item)
    {
        return new PredictionHistoryRow(
            item.AssetId, item.AssetName, DateText(item.CreatedOn), PriceText(item.CurrentPrice, item.PredictedPrice),
            Change(item.PredictedPercentChange), ScoreText(item.ConfidenceScore, item.RiskScore),
            First(item.Reasons), First(item.Warnings), item.IsMock, item.Reasons.Count > 0, item.Warnings.Count > 0
        );
    }

    private static ModelAccuracySummaryRow AccuracyRow(ModelAccuracySummary summary)
    {
        return new ModelAccuracySummaryRow(
            summary.ModelName, ScoredText(summary.ScoredCount), Percent(summary.WinRate),
            Percent(summary.AverageAbsolutePercentError), Percent(summary.ConfidenceCalibrationError),
            AverageHorizon(summary.AverageTimeHorizonDays)
        );
    }

    private static string CategoryText(AiCategoryPrediction category, AiPrediction prediction)
    {
        var value = string.IsNullOrWhiteSpace(prediction.Category) ? category.Category : prediction.Category;
        return $"{prediction.AssetType} | {value}";
    }

    private static string PriceText(AiPrediction prediction)
    {
        return PriceText(prediction.CurrentPrice, prediction.PredictedPrice);
    }

    private static string PriceText(decimal current, decimal predicted)
    {
        return $"{Money(current)} -> {Money(predicted)}";
    }

    private static string ScoreText(AiPrediction prediction)
    {
        return ScoreText(prediction.ConfidenceScore, prediction.RiskScore);
    }

    private static string ScoreText(double confidence, double risk)
    {
        return $"Confidence {Score(confidence)} | Risk {Score(risk)}";
    }

    private static string PredictionModelText(AiPrediction prediction)
    {
        return $"{prediction.ModelName} v{prediction.ModelVersion} | {prediction.TimeHorizonDays} days";
    }

    private static string Change(double value)
    {
        return $"{value:N2}%";
    }

    private static string ScoredText(int count)
    {
        return $"{count} scored {(count == 1 ? "prediction" : "predictions")}";
    }

    private static string Percent(double value)
    {
        return $"{value:N2}%";
    }

    private static string AverageHorizon(double value)
    {
        var days = (int)Math.Round(value, MidpointRounding.AwayFromZero);
        return $"{days} {(days == 1 ? "day" : "days")}";
    }

    private static string Score(double value)
    {
        return $"{value * 100:N2}%";
    }

    private static string Money(decimal value)
    {
        return $"${value:N2}";
    }

    private static string DisplayName(string symbol, string name)
    {
        return string.IsNullOrWhiteSpace(name) ? symbol : name;
    }

    private static string HorizonText(int? horizon)
    {
        return horizon is null ? "Default horizon" : $"{horizon} days";
    }

    private static string First(IReadOnlyList<string> values)
    {
        return values.Count == 0 ? string.Empty : values[0];
    }

    private static string DateText(DateTime value)
    {
        return value.ToLocalTime().ToString("MMM d, yyyy", CultureInfo.CurrentCulture);
    }

    private static bool IsMock(AiPrediction prediction)
    {
        return prediction.Warnings.Any(warning => warning.Contains("MOCK", StringComparison.OrdinalIgnoreCase));
    }

    private bool HasValidAsset()
    {
        return !string.IsNullOrWhiteSpace(Symbol) && TryPrice(CurrentPrice, out _) && TryHorizon(TimeHorizonDays, out _);
    }

    private static bool TryPrice(string value, out decimal price)
    {
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out price) && price > 0;
    }

    private static bool TryHorizon(string value, out int? horizon)
    {
        horizon = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }
        var valid = int.TryParse(value, out var parsed) && parsed > 0;
        horizon = valid ? parsed : null;
        return valid;
    }

    private void RemoveAsset(PredictAssetRow? asset)
    {
        if (asset is null)
        {
            return;
        }
        StagedAssets.Remove(asset);
        RefreshStagedState();
    }

    private void ClearAssetForm()
    {
        Symbol = string.Empty;
        AssetName = string.Empty;
        Category = string.Empty;
        CurrentPrice = string.Empty;
    }

    private void ReplacePredictions(IEnumerable<PredictionResultRow> rows)
    {
        Predictions.Clear();
        foreach (var row in rows)
        {
            Predictions.Add(row);
        }
    }

    private void ReplaceHistory(IEnumerable<PredictionHistoryRow> rows)
    {
        History.Clear();
        foreach (var row in rows)
        {
            History.Add(row);
        }
        RefreshHistoryState();
    }

    private void ReplaceAccuracy(IEnumerable<ModelAccuracySummaryRow> rows)
    {
        AccuracySummaries.Clear();
        foreach (var row in rows)
        {
            AccuracySummaries.Add(row);
        }
        RefreshAccuracyState();
    }

    private void SyncAuthState()
    {
        IsAuthenticated = _authSession.IsAuthenticated;
        if (IsAnonymous)
        {
            ApplySignedOutState();
            return;
        }
        MessageText = HasStaged ? MessageText : "Stage an asset, then run a prediction.";
    }

    private void ApplySignedOutState()
    {
        _overview = null;
        StagedAssets.Clear();
        ReplacePredictions([]);
        ReplaceAccuracy([]);
        ReplaceHistory([]);
        MessageText = "Sign in to run predictions.";
        AccuracyMessageText = "Sign in to load model accuracy.";
        HistoryMessageText = "Sign in to load saved prediction history.";
        RefreshStagedState();
        NotifyOverviewChanged();
    }

    private void SetBusy(bool value)
    {
        if (SetField(ref _isBusy, value, nameof(IsBusy)))
        {
            RefreshCommands();
        }
    }

    private void SetHistoryBusy(bool value)
    {
        if (SetField(ref _isHistoryBusy, value, nameof(IsHistoryBusy)))
        {
            RefreshHistoryState();
        }
    }

    private void SetAccuracyBusy(bool value)
    {
        if (SetField(ref _isAccuracyBusy, value, nameof(IsAccuracyBusy)))
        {
            RefreshAccuracyState();
        }
    }

    private void SetAuthenticated(bool value)
    {
        if (!SetField(ref _isAuthenticated, value, nameof(IsAuthenticated)))
        {
            return;
        }
        OnPropertyChanged(nameof(IsAnonymous));
        RefreshCommands();
        RefreshAccuracyState();
        RefreshHistoryState();
    }

    private void SetMessage(string value)
    {
        if (SetField(ref _messageText, value, nameof(MessageText)))
        {
            OnPropertyChanged(nameof(ShowMessage));
        }
    }

    private void SetHistoryMessage(string value)
    {
        if (SetField(ref _historyMessageText, value, nameof(HistoryMessageText)))
        {
            OnPropertyChanged(nameof(ShowHistoryMessage));
        }
    }

    private void SetAccuracyMessage(string value)
    {
        if (SetField(ref _accuracyMessageText, value, nameof(AccuracyMessageText)))
        {
            OnPropertyChanged(nameof(ShowAccuracyMessage));
        }
    }

    private void SetInput(ref string field, string value, string name)
    {
        if (SetField(ref field, value, name))
        {
            RefreshCommands();
        }
    }

    private void RefreshCommands()
    {
        OnPropertyChanged(nameof(CanStage));
        OnPropertyChanged(nameof(CanRun));
        OnPropertyChanged(nameof(ShowMessage));
        _stageCommand.ChangeCanExecute();
        _runCommand.ChangeCanExecute();
    }

    private void RefreshStagedState()
    {
        OnPropertyChanged(nameof(HasStaged));
        RefreshCommands();
    }

    private void RefreshHistoryState()
    {
        OnPropertyChanged(nameof(HasHistory));
        OnPropertyChanged(nameof(CanRefreshHistory));
        OnPropertyChanged(nameof(ShowHistoryMessage));
        _historyCommand.ChangeCanExecute();
    }

    private void RefreshAccuracyState()
    {
        OnPropertyChanged(nameof(HasAccuracySummaries));
        OnPropertyChanged(nameof(ShowAccuracySummaries));
        OnPropertyChanged(nameof(CanRefreshAccuracy));
        OnPropertyChanged(nameof(ShowAccuracyMessage));
        _accuracyCommand.ChangeCanExecute();
    }

    private void NotifyOverviewChanged()
    {
        foreach (var property in OverviewProperties)
        {
            OnPropertyChanged(property);
        }
    }

    private static string Clean(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }

    private static string Upper(string? value)
    {
        return Clean(value).ToUpperInvariant();
    }

    private static string? EmptyAsNull(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
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
