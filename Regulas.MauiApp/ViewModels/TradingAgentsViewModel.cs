using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;
using Regulas.MauiApp.Models;
using Regulas.MauiApp.Services;

namespace Regulas.MauiApp.ViewModels;

public sealed class TradingAgentsViewModel : INotifyPropertyChanged
{
    private static readonly string[] ResultProperties = [
        nameof(HasResult), nameof(SummaryText), nameof(RecommendationText), nameof(ScoreText),
        nameof(ModelText), nameof(ResultDateText), nameof(HasRawDecision), nameof(RawDecisionText), nameof(IsResultMock)
    ];
    private static readonly string[] StatusProperties = [
        nameof(StatusText), nameof(ModelStatusText), nameof(PurposeText), nameof(IsModelMock)
    ];
    private readonly IRegulasApiClient _apiClient;
    private readonly Command _analyzeCommand;
    private readonly Command _refreshStatusCommand;
    private StockTradingAgentsResponse? _analysis;
    private string _analysisDate = string.Empty;
    private string _companyName = string.Empty;
    private string _currentPrice = string.Empty;
    private TradingAgentsHealth? _health;
    private bool _isBusy;
    private bool _isStatusBusy;
    private string _messageText = "Enter a stock symbol and price to run TradingAgents research.";
    private TradingAgentsModelInfo? _model;
    private string _symbol = string.Empty;

    public TradingAgentsViewModel(IRegulasApiClient apiClient)
    {
        _apiClient = apiClient;
        _analyzeCommand = new Command(async () => await AnalyzeAsync(), () => CanAnalyze);
        _refreshStatusCommand = new Command(async () => await RefreshStatusAsync(), () => !IsStatusBusy);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<TradingArgumentRow> BullishArguments { get; } = [];
    public ObservableCollection<TradingArgumentRow> BearishArguments { get; } = [];
    public ObservableCollection<TradingArgumentRow> Warnings { get; } = [];
    public ICommand AnalyzeCommand => _analyzeCommand;
    public ICommand RefreshStatusCommand => _refreshStatusCommand;
    public string Symbol { get => _symbol; set => SetInput(ref _symbol, Upper(value), nameof(Symbol)); }
    public string CompanyName { get => _companyName; set => SetInput(ref _companyName, value, nameof(CompanyName)); }
    public string CurrentPrice { get => _currentPrice; set => SetInput(ref _currentPrice, value, nameof(CurrentPrice)); }
    public string AnalysisDate { get => _analysisDate; set => SetInput(ref _analysisDate, value, nameof(AnalysisDate)); }
    public string MessageText { get => _messageText; private set => SetMessage(value); }
    public bool IsBusy { get => _isBusy; private set => SetBusy(value); }
    public bool IsStatusBusy { get => _isStatusBusy; private set => SetStatusBusy(value); }
    public bool CanAnalyze => !IsBusy && !string.IsNullOrWhiteSpace(Symbol) && TryPrice(CurrentPrice, out _);
    public bool ShowMessage => !IsBusy && !string.IsNullOrWhiteSpace(MessageText);
    public bool HasResult => _analysis is not null;
    public bool HasBullish => BullishArguments.Count > 0;
    public bool HasBearish => BearishArguments.Count > 0;
    public bool HasWarnings => Warnings.Count > 0;
    public bool HasRawDecision => !string.IsNullOrWhiteSpace(RawDecisionText);
    public bool IsResultMock => _analysis?.IsMock == true;
    public bool IsModelMock => _model?.IsMock == true;
    public string StatusText => StatusMessage();
    public string ModelStatusText => ModelStatus();
    public string PurposeText => _model?.Purpose ?? "Model purpose not loaded.";
    public string SummaryText => _analysis?.Summary ?? "Run an analysis to see the research summary.";
    public string RecommendationText => _analysis is null ? string.Empty : $"{_analysis.Symbol}: {_analysis.Recommendation}";
    public string ScoreText => _analysis is null ? string.Empty : ScoreSummaryText(_analysis);
    public string ModelText => _analysis is null ? string.Empty : $"{_analysis.ModelName} v{_analysis.ModelVersion}";
    public string ResultDateText => _analysis is null ? string.Empty : $"Analysis date {_analysis.AnalysisDate:yyyy-MM-dd}";
    public string RawDecisionText => RawDecisionJson(_analysis?.RawDecision);

    public async Task LoadAsync()
    {
        await RefreshStatusAsync();
    }

    public void ApplyQuery(string? symbol, string? companyName, string? currentPrice)
    {
        Symbol = Upper(symbol);
        CompanyName = Clean(companyName);
        CurrentPrice = Clean(currentPrice);
    }

    private async Task RefreshStatusAsync()
    {
        if (IsStatusBusy)
        {
            return;
        }
        await RunStatusBusyAsync(LoadStatusCoreAsync);
    }

    private async Task LoadStatusCoreAsync()
    {
        var health = await _apiClient.GetTradingAgentsHealthAsync(CancellationToken.None);
        var model = await _apiClient.GetTradingAgentsModelInfoAsync(CancellationToken.None);
        ApplyStatus(health, model);
    }

    private async Task AnalyzeAsync()
    {
        if (!CanAnalyze || !TryBuildRequest(out var request))
        {
            return;
        }
        await RunAnalyzeBusyAsync(async () => ApplyAnalysisResult(await _apiClient.AnalyzeStockWithTradingAgentsAsync(request, CancellationToken.None)));
    }

    private bool TryBuildRequest(out StockTradingAgentsRequest request)
    {
        request = default!;
        if (!TryPrice(CurrentPrice, out var price) || !TryDate(out var date))
        {
            MessageText = "Use a positive price and a yyyy-mm-dd date if provided.";
            return false;
        }
        request = new StockTradingAgentsRequest(Upper(Symbol), EmptyAsNull(CompanyName), price, date);
        return true;
    }

    private bool TryDate(out DateOnly? date)
    {
        date = null;
        if (string.IsNullOrWhiteSpace(AnalysisDate))
        {
            return true;
        }
        var valid = DateOnly.TryParse(AnalysisDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed);
        date = valid ? parsed : null;
        return valid;
    }

    private void ApplyStatus(ApiClientResult<TradingAgentsHealth> health, ApiClientResult<TradingAgentsModelInfo> model)
    {
        _health = health.Ok ? health.Data : null;
        _model = model.Ok ? model.Data : null;
        MessageText = StatusMessage(health, model);
        NotifyStatusChanged();
    }

    private void ApplyAnalysisResult(ApiClientResult<StockTradingAgentsResponse> result)
    {
        if (!result.Ok || result.Data is null)
        {
            ApplyAnalysisFailure(result.Message);
            return;
        }
        ApplyAnalysis(result.Data);
    }

    private void ApplyAnalysis(StockTradingAgentsResponse value)
    {
        _analysis = value;
        Replace(BullishArguments, value.BullishArguments);
        Replace(BearishArguments, value.BearishArguments);
        Replace(Warnings, value.Warnings);
        MessageText = "TradingAgents research complete.";
        NotifyResultChanged();
    }

    private void ApplyAnalysisFailure(string message)
    {
        _analysis = null;
        Replace(BullishArguments, []);
        Replace(BearishArguments, []);
        Replace(Warnings, []);
        MessageText = message;
        NotifyResultChanged();
    }

    private async Task RunStatusBusyAsync(Func<Task> action)
    {
        IsStatusBusy = true;
        try
        {
            await action();
        }
        finally
        {
            IsStatusBusy = false;
        }
    }

    private async Task RunAnalyzeBusyAsync(Func<Task> action)
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

    private void Replace(ObservableCollection<TradingArgumentRow> rows, IEnumerable<string> values)
    {
        rows.Clear();
        foreach (var value in values)
        {
            rows.Add(new TradingArgumentRow(value));
        }
    }

    private string StatusMessage()
    {
        if (IsStatusBusy)
        {
            return "Checking StockTradingAgentsAI...";
        }
        return _health?.AiAvailable == true ? "StockTradingAgentsAI online." : "StockTradingAgentsAI offline or not checked.";
    }

    private static string StatusMessage(ApiClientResult<TradingAgentsHealth> health, ApiClientResult<TradingAgentsModelInfo> model)
    {
        if (!health.Ok)
        {
            return health.Message;
        }
        return model.Ok ? string.Empty : model.Message;
    }

    private string ModelStatus()
    {
        if (_model is null)
        {
            return "Model info not loaded.";
        }
        return $"{_model.ModelName} v{_model.ModelVersion} | {_model.AssetType} | {_model.Category}";
    }

    private static string ScoreSummaryText(StockTradingAgentsResponse value)
    {
        return $"Price {Money(value.CurrentPrice)} | Confidence {Score(value.ConfidenceScore)} | Risk {Score(value.RiskScore)}";
    }

    private static string RawDecisionJson(JsonElement? value)
    {
        return value?.GetRawText() ?? string.Empty;
    }

    private static bool TryPrice(string value, out decimal price)
    {
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out price) && price > 0;
    }

    private static string Money(decimal value)
    {
        return $"${value:N2}";
    }

    private static string Score(double value)
    {
        return $"{value * 100:N2}%";
    }

    private void SetBusy(bool value)
    {
        if (SetField(ref _isBusy, value, nameof(IsBusy)))
        {
            RefreshAnalyzeCommand();
        }
    }

    private void SetStatusBusy(bool value)
    {
        if (SetField(ref _isStatusBusy, value, nameof(IsStatusBusy)))
        {
            RefreshStatusCanExecute();
        }
    }

    private void SetMessage(string value)
    {
        if (SetField(ref _messageText, value, nameof(MessageText)))
        {
            OnPropertyChanged(nameof(ShowMessage));
        }
    }

    private void SetInput(ref string field, string value, string name)
    {
        if (SetField(ref field, value, name))
        {
            RefreshAnalyzeCommand();
        }
    }

    private void RefreshAnalyzeCommand()
    {
        OnPropertyChanged(nameof(CanAnalyze));
        OnPropertyChanged(nameof(ShowMessage));
        _analyzeCommand.ChangeCanExecute();
    }

    private void RefreshStatusCanExecute()
    {
        OnPropertyChanged(nameof(StatusText));
        _refreshStatusCommand.ChangeCanExecute();
    }

    private void NotifyResultChanged()
    {
        foreach (var property in ResultProperties)
        {
            OnPropertyChanged(property);
        }
        NotifyArgumentStateChanged();
    }

    private void NotifyStatusChanged()
    {
        foreach (var property in StatusProperties)
        {
            OnPropertyChanged(property);
        }
    }

    private void NotifyArgumentStateChanged()
    {
        OnPropertyChanged(nameof(HasBullish));
        OnPropertyChanged(nameof(HasBearish));
        OnPropertyChanged(nameof(HasWarnings));
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
