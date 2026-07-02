using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Regulas.MauiApp.Services;

namespace Regulas.MauiApp.ViewModels;

public sealed class SettingsViewModel : INotifyPropertyChanged
{
    private readonly AuthSession _authSession;
    private string _apiBaseUrl = ApiBaseUrl.Current;
    private string _accountDetail = "Not signed in.";
    private string _statusText = $"Default: {ApiBaseUrl.Default}";

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand LogoutCommand { get; }

    public ICommand SaveCommand { get; }

    public ICommand ResetCommand { get; }

    public string AccountDetail { get => _accountDetail; private set => SetField(ref _accountDetail, value); }

    public bool IsAuthenticated => _authSession.IsAuthenticated;

    public string ApiBaseUrlText { get => _apiBaseUrl; set => SetField(ref _apiBaseUrl, value); }

    public string StatusText { get => _statusText; private set => SetField(ref _statusText, value); }

    public SettingsViewModel(AuthSession authSession)
    {
        _authSession = authSession;
        LogoutCommand = new Command(async () => await LogoutAsync());
        SaveCommand = new Command(Save);
        ResetCommand = new Command(Reset);
        _authSession.PropertyChanged += (_, _) => SyncAccount();
    }

    public async Task LoadAsync()
    {
        await _authSession.RefreshAsync(CancellationToken.None);
        SyncAccount();
    }

    private async Task LogoutAsync()
    {
        await _authSession.LogoutAsync(CancellationToken.None);
        StatusText = "Signed out.";
        SyncAccount();
    }

    private void Save()
    {
        var value = ApiBaseUrlText.Trim();
        if (!ValidBaseUrl(value))
        {
            StatusText = "Use an http or https URL.";
            return;
        }
        ApiBaseUrl.Current = value.TrimEnd('/');
        StatusText = "API URL saved.";
    }

    private void Reset()
    {
        ApiBaseUrl.Reset();
        ApiBaseUrlText = ApiBaseUrl.Current;
        StatusText = "API URL reset.";
    }

    private static bool ValidBaseUrl(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) && HttpScheme(uri);
    }

    private static bool HttpScheme(Uri uri)
    {
        return uri.Scheme is "http" or "https";
    }

    private void SyncAccount()
    {
        AccountDetail = _authSession.CurrentUser is null ? "Not signed in." : $"{_authSession.CurrentUser.DisplayName} ({_authSession.CurrentUser.Email})";
        OnPropertyChanged(nameof(IsAuthenticated));
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
