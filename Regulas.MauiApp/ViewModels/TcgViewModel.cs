using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Regulas.MauiApp.Models;
using Regulas.MauiApp.Services;

namespace Regulas.MauiApp.ViewModels;

// Records manual TCG prices and browses provider data through Regulas.Api.
public sealed class TcgViewModel : INotifyPropertyChanged
{
    private readonly IRegulasApiClient _apiClient;
    private readonly AuthSession _authSession;
    private readonly Command _loadCommand;
    private readonly Command<TcgProviderCardRow> _openCardCommand;
    private readonly Command _saveCommand;
    private readonly Command _searchCardsCommand;
    private string _browseDetailName = string.Empty;
    private string? _browseDetailImageUrl;
    private string _browseDetailText = string.Empty;
    private string _browseGame = "Pokemon";
    private string _browseMessageText = "Search Pokemon or Magic cards through Regulas.Api.";
    private string _browseQuery = string.Empty;
    private int _browseGeneration;
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
        _searchCardsCommand = new Command(async () => await SearchCardsAsync(), () => CanSearchCards);
        _openCardCommand = new Command<TcgProviderCardRow>(async card => await OpenCardAsync(card), _ => CanOpenCard);

        OpenAccountCommand = new Command(async () => await NavigationRoutes.OpenAccountAsync());
        _authSession.PropertyChanged += (_, _) => SyncAuthState();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<TcgPointRow> Points { get; } = [];
    public ObservableCollection<TcgProviderCardRow> ProviderCards { get; } = [];
    public ObservableCollection<TcgProviderPriceRow> ProviderPrices { get; } = [];

    public IReadOnlyList<string> TcgGames { get; } = ["Pokemon", "Magic", "One Piece"];
    public IReadOnlyList<string> BrowseGames { get; } = ["Pokemon", "Magic"];
    public IReadOnlyList<string> PriceTypes { get; } = ["Sold", "Listed", "Market"];
    public ICommand SaveCommand => _saveCommand;
    public ICommand LoadCommand => _loadCommand;
    public ICommand SearchCardsCommand => _searchCardsCommand;
    public ICommand OpenCardCommand => _openCardCommand;

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
    public string BrowseGame { get => _browseGame; set => SetBrowseGame(value); }
    public string BrowseQuery { get => _browseQuery; set => SetInput(ref _browseQuery, value, nameof(BrowseQuery)); }
    public string BrowseMessageText { get => _browseMessageText; private set => SetBrowseMessage(value); }
    public string BrowseDetailName { get => _browseDetailName; private set => SetBrowseDetailName(value); }
    public string? BrowseDetailImageUrl { get => _browseDetailImageUrl; private set => SetField(ref _browseDetailImageUrl, value, nameof(BrowseDetailImageUrl)); }
    public string BrowseDetailText { get => _browseDetailText; private set => SetField(ref _browseDetailText, value, nameof(BrowseDetailText)); }

    public bool IsBusy { get => _isBusy; private set => SetBusy(value); }
    public bool IsAuthenticated { get => _isAuthenticated; private set => SetAuthenticated(value); }
    public bool IsAnonymous => !IsAuthenticated;
    public bool HasPoints => Points.Count > 0;
    public bool HasProviderCards => ProviderCards.Count > 0;
    public bool HasProviderDetail => !string.IsNullOrWhiteSpace(BrowseDetailName);

    public bool ShowMessage => !IsBusy && !string.IsNullOrWhiteSpace(MessageText);
    public bool ShowBrowseMessage => !IsBusy && !string.IsNullOrWhiteSpace(BrowseMessageText);

    public bool CanSave => IsAuthenticated && !IsBusy && HasValidEntry();
    public bool CanLoad => !IsBusy && !string.IsNullOrWhiteSpace(Symbol);
    public bool CanSearchCards => !IsBusy && !string.IsNullOrWhiteSpace(BrowseQuery);
    public bool CanOpenCard => !IsBusy;


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

    private async Task SearchCardsAsync()
    {
        if (!CanSearchCards)
        {
            return;
        }
        var generation = _browseGeneration;
        var game = BrowseGame;
        var query = BrowseQuery.Trim();
        await RunBusyAsync(() => SearchCardsCoreAsync(generation, game, query));
    }

    private async Task SearchCardsCoreAsync(int generation, string game, string query)
    {
        ClearProviderDetail();
        if (game == "Pokemon")
        {
            await SearchPokemonAsync(query, generation);
            return;
        }
        await SearchMagicProviderAsync(query, generation);
    }

    private async Task SearchPokemonAsync(string query, int generation)
    {
        var result = await _apiClient.SearchPokemonCardsAsync(query, 12, CancellationToken.None);
        if (!IsCurrentBrowse(generation))
        {
            return;
        }
        if (!result.Ok || result.Data is null)
        {
            ApplyProviderSearchFailure(result.Message);
            return;
        }
        ReplaceProviderCards(result.Data.Cards.Select(PokemonRow));
        BrowseMessageText = FoundMessage("Pokemon", ProviderCards.Count);
    }

    private async Task SearchMagicProviderAsync(string query, int generation)
    {
        var result = await _apiClient.SearchMagicCardsAsync(query, 12, CancellationToken.None);
        if (!IsCurrentBrowse(generation))
        {
            return;
        }
        if (!result.Ok || result.Data is null)
        {
            ApplyProviderSearchFailure(result.Message);
            return;
        }
        ReplaceProviderCards(result.Data.Cards.Select(MagicProviderRow));
        BrowseMessageText = FoundMessage("Magic", ProviderCards.Count);
    }

    private async Task OpenCardAsync(TcgProviderCardRow? card)
    {
        if (card is null)
        {
            return;
        }
        var generation = _browseGeneration;
        await RunBusyAsync(() => OpenCardCoreAsync(card, generation));
    }

    private Task OpenCardCoreAsync(TcgProviderCardRow card, int generation)
    {
        return card.Game == "Pokemon"
            ? OpenPokemonProviderAsync(card.Id, generation)
            : OpenMagicProviderAsync(card.Id, generation);
    }

    private async Task OpenPokemonProviderAsync(string id, int generation)
    {
        var result = await _apiClient.GetPokemonCardAsync(id, CancellationToken.None);
        if (!IsCurrentBrowse(generation))
        {
            return;
        }
        if (!result.Ok || result.Data is null)
        {
            ApplyProviderDetailFailure(result.Message);
            return;
        }
        ApplyPokemonDetail(result.Data);
    }

    private async Task OpenMagicProviderAsync(string id, int generation)
    {
        var result = await _apiClient.GetMagicCardAsync(id, CancellationToken.None);
        if (!IsCurrentBrowse(generation))
        {
            return;
        }
        if (!result.Ok || result.Data is null)
        {
            ApplyProviderDetailFailure(result.Message);
            return;
        }
        ApplyMagicProviderDetail(result.Data);
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

    private void ApplyProviderSearchFailure(string message)
    {
        ReplaceProviderCards([]);
        ClearProviderDetail();
        BrowseMessageText = message;
    }

    private void ApplyProviderDetailFailure(string message)
    {
        ClearProviderDetail();
        BrowseMessageText = message;
    }

    private void ApplyPokemonDetail(PokemonCardDetail card)
    {
        BrowseDetailName = card.Name;
        BrowseDetailImageUrl = card.LargeImageUrl ?? card.SmallImageUrl;
        BrowseDetailText = PokemonDetailText(card);
        ReplaceProviderPrices(card.Prices.Select(PokemonPriceRow));
        FillPokemonEntry(card);
        BrowseMessageText = $"{card.Name} loaded. Provider price was captured by the API when available.";
    }

    private void ApplyMagicProviderDetail(MagicCardDetail card)
    {
        BrowseDetailName = card.Name;
        BrowseDetailImageUrl = card.LargeImageUrl ?? card.SmallImageUrl;
        BrowseDetailText = DetailText(card);
        ReplaceProviderPrices(card.Prices.Select(MagicProviderPriceRow));
        FillManualEntry(card);
        BrowseMessageText = $"{card.Name} loaded. Provider price was captured by the API when available.";
    }

    private void FillPokemonEntry(PokemonCardDetail card)
    {
        Symbol = card.Id;
        Name = card.Name;
        Category = "Pokemon";
        PriceType = "Market";
        Currency = "USD";
        var price = BestPokemonPrice(card.Prices);
        Price = price?.ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty;
    }


    private void FillManualEntry(MagicCardDetail card)
    {
        Symbol = card.Id;
        Name = card.Name;
        Category = "Magic";
        PriceType = "Market";
        var price = card.Prices.FirstOrDefault();
        Currency = price?.Currency?.ToUpperInvariant() ?? string.Empty;
        Price = price?.MarketPrice.ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty;
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

    private void ReplaceProviderCards(IEnumerable<TcgProviderCardRow> cards)
    {
        ProviderCards.Clear();
        foreach (var card in cards)
        {
            ProviderCards.Add(card);
        }
        OnPropertyChanged(nameof(HasProviderCards));
    }

    private void ReplaceProviderPrices(IEnumerable<TcgProviderPriceRow> prices)
    {
        ProviderPrices.Clear();
        foreach (var price in prices)
        {
            ProviderPrices.Add(price);
        }
    }

    private void ClearProviderDetail()
    {
        BrowseDetailName = string.Empty;
        BrowseDetailImageUrl = null;
        BrowseDetailText = string.Empty;
        ProviderPrices.Clear();
    }


    private static TcgPointRow Row(PricePoint point)
    {
        return new TcgPointRow(point.Date.ToString("yyyy-MM-dd"), $"{point.Close:N2}", Meta(point), $"Source: {point.Source}");
    }


    private static TcgProviderCardRow PokemonRow(PokemonCardSummary card)
    {
        var details = Join(card.SetName, card.Number, card.Rarity);
        var price = card.MarketPrice is null ? "No provider price" : ProviderMoney(card.MarketPrice.Value, "USD");
        return new TcgProviderCardRow(card.Id, "Pokemon", card.Name, details, card.SmallImageUrl, price);
    }

    private static TcgProviderCardRow MagicProviderRow(MagicCardSummary card)
    {
        return new TcgProviderCardRow(card.Id, "Magic", card.Name, SummaryDetails(card), card.SmallImageUrl, SummaryPrice(card));
    }


    private static TcgProviderPriceRow PokemonPriceRow(PokemonCardPrice price)
    {
        var selected = PokemonVariantPrice(price);
        var value = selected.Value is null ? "Unavailable" : ProviderMoney(selected.Value.Value, "USD");
        return new TcgProviderPriceRow($"{Title(price.Variant)} {selected.Label}", value);
    }

    private static TcgProviderPriceRow MagicProviderPriceRow(MagicCardPrice price)
    {
        return new TcgProviderPriceRow($"{Title(price.Finish)} {price.Currency.ToUpperInvariant()}", ProviderMoney(price.MarketPrice, price.Currency));
    }

    private static decimal? BestPokemonPrice(IReadOnlyList<PokemonCardPrice> prices)
    {
        return FirstValue(prices.Select(price => price.Market))
            ?? FirstValue(prices.Select(price => price.Mid))
            ?? FirstValue(prices.Select(price => price.Low))
            ?? FirstValue(prices.Select(price => price.High))
            ?? FirstValue(prices.Select(price => price.DirectLow));
    }

    private static decimal? FirstValue(IEnumerable<decimal?> values)
    {
        return values.FirstOrDefault(value => value is not null);
    }

    private static (string Label, decimal? Value) PokemonVariantPrice(PokemonCardPrice price)
    {
        if (price.Market is not null) return ("market", price.Market);
        if (price.Mid is not null) return ("mid", price.Mid);
        if (price.Low is not null) return ("low", price.Low);
        if (price.High is not null) return ("high", price.High);
        if (price.DirectLow is not null) return ("direct low", price.DirectLow);
        return ("price", null);
    }

    private static string SummaryDetails(MagicCardSummary card)
    {
        var parts = new[] { card.SetName, card.CollectorNumber, card.Rarity };
        return string.Join(" · ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
    }

    private static string Join(params string?[] parts)
    {
        return string.Join(" · ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
    }

    private static string FoundMessage(string game, int count)
    {
        return count == 0 ? $"No {game} cards matched that search." : $"Found {count} {game} {(count == 1 ? "card" : "cards")}.";
    }

    private static string SummaryPrice(MagicCardSummary card)
    {
        return card.MarketPrice is null ? "No provider price" : ProviderMoney(card.MarketPrice.Value, card.MarketCurrency);
    }

    private static string DetailText(MagicCardDetail card)
    {
        var parts = new[] { card.TypeLine, card.ManaCost, card.OracleText };
        return string.Join(Environment.NewLine, parts.Where(part => !string.IsNullOrWhiteSpace(part)));
    }

    private static string PokemonDetailText(PokemonCardDetail card)
    {
        var kind = Join(card.Supertype, string.Join(", ", card.Subtypes));
        var stats = Join(string.IsNullOrWhiteSpace(card.Hp) ? null : $"HP {card.Hp}", string.Join(", ", card.Types));
        return string.Join(Environment.NewLine, new[] { kind, stats }.Where(part => !string.IsNullOrWhiteSpace(part)));
    }

    private static string Meta(PricePoint point)
    {
        var parts = new[] { point.PriceType, point.CardCondition, point.Grade, point.Currency };
        return string.Join(" · ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
    }

    private static string ProviderMoney(decimal amount, string? currency)
    {
        var code = BlankToNull(currency)?.ToUpperInvariant() ?? "USD";
        return code switch { "USD" => $"${amount:N2}", "EUR" => $"€{amount:N2}", _ => $"{amount:N2} {code}" };
    }

    private static string Title(string value)
    {
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(value.Trim());
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

    private static string? BlankToNull(string? value)
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

    private void SetBrowseGame(string value)
    {
        if (!SetField(ref _browseGame, value, nameof(BrowseGame)))
        {
            return;
        }
        _browseGeneration++;
        ReplaceProviderCards([]);
        ClearProviderDetail();
        BrowseMessageText = $"Search {value} cards through Regulas.Api.";
        RefreshCommands();
    }

    private bool IsCurrentBrowse(int generation)
    {
        return generation == _browseGeneration;
    }

    private void SetBrowseMessage(string value)
    {
        if (SetField(ref _browseMessageText, value, nameof(BrowseMessageText)))
        {
            OnPropertyChanged(nameof(ShowBrowseMessage));
        }
    }

    private void SetBrowseDetailName(string value)
    {
        if (SetField(ref _browseDetailName, value, nameof(BrowseDetailName)))
        {
            OnPropertyChanged(nameof(HasProviderDetail));
        }
    }


    private void RefreshCommands()
    {
        NotifyCommandStates();
        RefreshCommandExecution();
    }

    private void NotifyCommandStates()
    {
        OnPropertyChanged(nameof(CanSave));
        OnPropertyChanged(nameof(CanLoad));
        OnPropertyChanged(nameof(CanSearchCards));
        OnPropertyChanged(nameof(CanOpenCard));
        OnPropertyChanged(nameof(ShowMessage));
        OnPropertyChanged(nameof(ShowBrowseMessage));
    }

    private void RefreshCommandExecution()
    {
        _saveCommand.ChangeCanExecute();
        _loadCommand.ChangeCanExecute();
        _searchCardsCommand.ChangeCanExecute();
        _openCardCommand.ChangeCanExecute();

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
