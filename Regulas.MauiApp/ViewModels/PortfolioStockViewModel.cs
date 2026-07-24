using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Regulas.MauiApp.Models;
using Regulas.MauiApp.Services;

namespace Regulas.MauiApp.ViewModels;

// Edits one saved portfolio stock and manages its notes, mirroring the web portfolio panel.
public sealed class PortfolioStockViewModel : INotifyPropertyChanged
{
    private readonly IRegulasApiClient _apiClient;
    private readonly Command _saveStockCommand;
    private readonly Command _saveNoteCommand;
    private readonly Command _cancelNoteCommand;
    private readonly Command<StockNoteRow> _editNoteCommand;
    private readonly Command<StockNoteRow> _deleteNoteCommand;
    private int _stockId;
    private int? _editingNoteId;
    private bool _isBusy;
    private string _symbol = string.Empty;
    private string _companyName = string.Empty;
    private string _purchasePrice = string.Empty;
    private string _lastDividend = string.Empty;
    private string _industry = string.Empty;
    private string _marketCap = string.Empty;
    private string _messageText = string.Empty;
    private string _noteTitle = string.Empty;
    private string _noteContent = string.Empty;
    private string _notesMessageText = string.Empty;

    public PortfolioStockViewModel(IRegulasApiClient apiClient)
    {
        _apiClient = apiClient;
        _saveStockCommand = new Command(async () => await SaveStockAsync(), () => CanSaveStock);
        _saveNoteCommand = new Command(async () => await SaveNoteAsync(), () => CanSaveNote);
        _cancelNoteCommand = new Command(ClearNoteForm);
        _editNoteCommand = new Command<StockNoteRow>(BeginEditNote);
        _deleteNoteCommand = new Command<StockNoteRow>(async note => await DeleteNoteAsync(note));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<StockNoteRow> Notes { get; } = [];
    public ICommand SaveStockCommand => _saveStockCommand;
    public ICommand SaveNoteCommand => _saveNoteCommand;
    public ICommand CancelNoteCommand => _cancelNoteCommand;
    public ICommand EditNoteCommand => _editNoteCommand;
    public ICommand DeleteNoteCommand => _deleteNoteCommand;
    public string Symbol { get => _symbol; private set => SetField(ref _symbol, value); }
    public string TitleText => string.IsNullOrWhiteSpace(Symbol) ? "Manage stock" : $"Manage {Symbol}";
    public string CompanyName { get => _companyName; set => SetInput(ref _companyName, value, nameof(CompanyName)); }
    public string PurchasePrice { get => _purchasePrice; set => SetInput(ref _purchasePrice, value, nameof(PurchasePrice)); }
    public string LastDividend { get => _lastDividend; set => SetInput(ref _lastDividend, value, nameof(LastDividend)); }
    public string Industry { get => _industry; set => SetInput(ref _industry, value, nameof(Industry)); }
    public string MarketCap { get => _marketCap; set => SetInput(ref _marketCap, value, nameof(MarketCap)); }
    public string MessageText { get => _messageText; private set => SetMessage(ref _messageText, value, nameof(MessageText), nameof(ShowMessage)); }
    public string NoteTitle { get => _noteTitle; set => SetInput(ref _noteTitle, value, nameof(NoteTitle)); }
    public string NoteContent { get => _noteContent; set => SetInput(ref _noteContent, value, nameof(NoteContent)); }
    public string NotesMessageText { get => _notesMessageText; private set => SetMessage(ref _notesMessageText, value, nameof(NotesMessageText), nameof(ShowNotesMessage)); }
    public string NoteFormLabel => _editingNoteId is null ? "Save note" : "Update note";
    public bool IsEditingNote => _editingNoteId is not null;
    public bool IsBusy { get => _isBusy; private set => SetBusy(value); }
    public bool HasStock => _stockId > 0;
    public bool HasNotes => Notes.Count > 0;
    public bool ShowMessage => !string.IsNullOrWhiteSpace(MessageText);
    public bool ShowNotesMessage => !string.IsNullOrWhiteSpace(NotesMessageText);
    public bool CanSaveStock => !IsBusy && HasStock && NumbersValid();
    public bool CanSaveNote => !IsBusy && HasStock && HasNoteText();

    public void ApplySymbol(string? symbol)
    {
        Symbol = symbol?.Trim().ToUpperInvariant() ?? string.Empty;
        OnPropertyChanged(nameof(TitleText));
    }

    public async Task LoadAsync()
    {
        if (string.IsNullOrWhiteSpace(Symbol))
        {
            return;
        }
        await RunBusyAsync(LoadCoreAsync);
    }

    private async Task LoadCoreAsync()
    {
        var result = await _apiClient.GetPortfolioStockAsync(Symbol, CancellationToken.None);
        if (!result.Ok || result.Data is null)
        {
            MessageText = result.Message;
            return;
        }
        ApplyStock(result.Data);
        await LoadNotesAsync();
    }

    private void ApplyStock(PortfolioStock stock)
    {
        _stockId = stock.Id;
        CompanyName = stock.CompanyName;
        PurchasePrice = Plain(stock.PurchasePrice);
        LastDividend = Plain(stock.LastDividend);
        Industry = stock.Industry;
        MarketCap = stock.MarketCap.ToString(CultureInfo.InvariantCulture);
        OnPropertyChanged(nameof(HasStock));
        RefreshCommands();
    }

    private async Task SaveStockAsync()
    {
        if (!CanSaveStock)
        {
            return;
        }
        await RunBusyAsync(SaveStockCoreAsync);
    }

    private async Task SaveStockCoreAsync()
    {
        var result = await _apiClient.UpdatePortfolioStockAsync(_stockId, ToUpdateRequest(), CancellationToken.None);
        if (!result.Ok || result.Data is null)
        {
            MessageText = result.Message;
            return;
        }
        ApplyStock(result.Data);
        MessageText = $"{result.Data.Symbol} updated.";
    }

    private CreatePortfolioStockRequest ToUpdateRequest()
    {
        return new CreatePortfolioStockRequest(
            Symbol, CompanyName.Trim(), ParseDecimal(PurchasePrice),
            ParseDecimal(LastDividend), Industry.Trim(), ParseLong(MarketCap)
        );
    }

    private async Task LoadNotesAsync()
    {
        var result = await _apiClient.GetStockCommentsAsync(_stockId, CancellationToken.None);
        if (!result.Ok || result.Data is null)
        {
            NotesMessageText = result.Message;
            return;
        }
        ReplaceNotes(result.Data);
        NotesMessageText = Notes.Count == 0 ? "No notes yet." : string.Empty;
    }

    private async Task SaveNoteAsync()
    {
        if (!CanSaveNote)
        {
            return;
        }
        await RunBusyAsync(SaveNoteCoreAsync);
    }

    // One form serves add and edit; the tracked note id decides which API call runs.
    private async Task SaveNoteCoreAsync()
    {
        var result = await SendNoteAsync();
        if (!result.Ok || result.Data is null)
        {
            NotesMessageText = result.Message;
            return;
        }
        ClearNoteForm();
        await LoadNotesAsync();
    }

    private Task<ApiClientResult<StockComment>> SendNoteAsync()
    {
        var request = new CreateStockCommentRequest(NoteTitle.Trim(), NoteContent.Trim());
        return _editingNoteId is int noteId
            ? _apiClient.UpdateStockCommentAsync(noteId, request, CancellationToken.None)
            : _apiClient.AddStockCommentAsync(_stockId, request, CancellationToken.None);
    }

    private void BeginEditNote(StockNoteRow? note)
    {
        if (note is null)
        {
            return;
        }
        _editingNoteId = note.Id;
        NoteTitle = note.Title;
        NoteContent = note.Content;
        NotifyNoteForm();
    }

    private async Task DeleteNoteAsync(StockNoteRow? note)
    {
        if (note is null || IsBusy)
        {
            return;
        }
        await RunBusyAsync(async () => await DeleteNoteCoreAsync(note.Id));
    }

    private async Task DeleteNoteCoreAsync(int noteId)
    {
        var result = await _apiClient.DeleteStockCommentAsync(noteId, CancellationToken.None);
        if (!result.Ok)
        {
            NotesMessageText = result.Message;
            return;
        }
        if (_editingNoteId == noteId)
        {
            ClearNoteForm();
        }
        await LoadNotesAsync();
    }

    private void ClearNoteForm()
    {
        _editingNoteId = null;
        NoteTitle = string.Empty;
        NoteContent = string.Empty;
        NotifyNoteForm();
    }

    private void ReplaceNotes(IReadOnlyList<StockComment> comments)
    {
        Notes.Clear();
        foreach (var comment in comments)
        {
            Notes.Add(Row(comment));
        }
        OnPropertyChanged(nameof(HasNotes));
    }

    private static StockNoteRow Row(StockComment comment)
    {
        return new StockNoteRow(comment.Id, comment.Title, comment.Content, comment.CreatedOn.ToString("yyyy-MM-dd"));
    }

    private bool HasNoteText()
    {
        return !string.IsNullOrWhiteSpace(NoteTitle) && !string.IsNullOrWhiteSpace(NoteContent);
    }

    // Blank number fields fall back to the backend defaults; typos must not save as zero.
    private bool NumbersValid()
    {
        return NumberOk(PurchasePrice) && NumberOk(LastDividend) && WholeNumberOk(MarketCap);
    }

    private static bool NumberOk(string value)
    {
        return string.IsNullOrWhiteSpace(value) || ParseDecimal(value) >= 0;
    }

    private static bool WholeNumberOk(string value)
    {
        return string.IsNullOrWhiteSpace(value) || ParseLong(value) >= 0;
    }

    private static decimal? ParseDecimal(string value)
    {
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
    }

    private static long? ParseLong(string value)
    {
        return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
    }

    private static string Plain(decimal value)
    {
        return value.ToString("0.##", CultureInfo.InvariantCulture);
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

    private void NotifyNoteForm()
    {
        OnPropertyChanged(nameof(NoteFormLabel));
        OnPropertyChanged(nameof(IsEditingNote));
        RefreshCommands();
    }

    private void SetBusy(bool value)
    {
        if (SetField(ref _isBusy, value, nameof(IsBusy)))
        {
            RefreshCommands();
        }
    }

    private void SetInput<T>(ref T field, T value, string name)
    {
        if (SetField(ref field, value, name))
        {
            RefreshCommands();
        }
    }

    private void SetMessage(ref string field, string value, string name, string showName)
    {
        if (SetField(ref field, value, name))
        {
            OnPropertyChanged(showName);
        }
    }

    private void RefreshCommands()
    {
        OnPropertyChanged(nameof(CanSaveStock));
        OnPropertyChanged(nameof(CanSaveNote));
        _saveStockCommand.ChangeCanExecute();
        _saveNoteCommand.ChangeCanExecute();
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
