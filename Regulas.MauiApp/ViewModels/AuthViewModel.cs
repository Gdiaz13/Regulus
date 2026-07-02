using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Regulas.MauiApp.Models;
using Regulas.MauiApp.Services;

namespace Regulas.MauiApp.ViewModels;

public sealed class AuthViewModel : INotifyPropertyChanged
{
    private readonly AuthSession _session;
    private readonly Command _submitCommand;
    private string _displayName = string.Empty;
    private string _email = string.Empty;
    private bool _isBusy;
    private bool _isRegisterMode;
    private string _password = string.Empty;
    private string _statusText = "Sign in to sync your portfolio, notes, and predictions.";

    public AuthViewModel(AuthSession session)
    {
        _session = session;
        _submitCommand = new Command(async () => await SubmitAsync(), () => CanSubmit);
        SwitchModeCommand = new Command(SwitchMode);
        LogoutCommand = new Command(async () => await LogoutAsync());
        _session.PropertyChanged += (_, _) => SyncUser();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand SubmitCommand => _submitCommand;
    public ICommand SwitchModeCommand { get; }
    public ICommand LogoutCommand { get; }
    public string Email { get => _email; set => SetFormField(ref _email, value, nameof(Email)); }
    public string Password { get => _password; set => SetFormField(ref _password, value, nameof(Password)); }
    public string DisplayName { get => _displayName; set => SetFormField(ref _displayName, value, nameof(DisplayName)); }
    public string StatusText { get => _statusText; private set => SetField(ref _statusText, value); }
    public bool IsBusy { get => _isBusy; private set => SetBusy(value); }
    public bool IsRegisterMode { get => _isRegisterMode; private set => SetRegisterMode(value); }
    public bool IsAuthenticated => _session.IsAuthenticated;
    public bool IsAnonymous => !IsAuthenticated;
    public bool CanSubmit => !IsBusy && HasValidCredentials();
    public string ModeTitle => IsRegisterMode ? "Create your Regulas account." : "Sign in to Regulas.";
    public string SubmitText => IsRegisterMode ? "Create account" : "Sign in";
    public string SwitchText => IsRegisterMode ? "Use an existing account" : "Create an account";
    public string CurrentUserText => _session.CurrentUser is null ? "Not signed in." : $"Signed in as {_session.CurrentUser.DisplayName}";

    public async Task LoadAsync()
    {
        await _session.RefreshAsync(CancellationToken.None);
        SyncUser();
    }

    private async Task SubmitAsync()
    {
        if (IsBusy || !ValidForm())
        {
            return;
        }
        await RunSubmitAsync();
    }

    private async Task RunSubmitAsync()
    {
        IsBusy = true;
        try
        {
            await ApplyResultAsync(await SendAsync());
        }
        finally
        {
            IsBusy = false;
        }
    }

    private Task<ApiClientResult<AuthResponse>> SendAsync()
    {
        return IsRegisterMode
            ? _session.RegisterAsync(new RegisterRequest(Email.Trim(), Password, DisplayName.Trim()), CancellationToken.None)
            : _session.LoginAsync(new LoginRequest(Email.Trim(), Password), CancellationToken.None);
    }

    private Task ApplyResultAsync(ApiClientResult<AuthResponse> result)
    {
        StatusText = result.Ok ? "Signed in." : result.Message;
        Password = result.Ok ? string.Empty : Password;
        SyncUser();
        return Task.CompletedTask;
    }

    private async Task LogoutAsync()
    {
        await _session.LogoutAsync(CancellationToken.None);
        StatusText = "Signed out.";
        SyncUser();
    }

    private bool ValidForm()
    {
        StatusText = ValidationMessage();
        return string.IsNullOrEmpty(StatusText);
    }

    private string ValidationMessage()
    {
        if (!Email.Contains('@', StringComparison.Ordinal) || Password.Length < 8)
        {
            return "Use a valid email and an 8 character password.";
        }
        return IsRegisterMode && string.IsNullOrWhiteSpace(DisplayName) ? "Display name is required." : string.Empty;
    }

    private bool HasValidCredentials()
    {
        return Email.Contains('@', StringComparison.Ordinal)
            && Password.Length >= 8
            && (!IsRegisterMode || !string.IsNullOrWhiteSpace(DisplayName));
    }

    private void SwitchMode()
    {
        IsRegisterMode = !IsRegisterMode;
        StatusText = IsRegisterMode ? "Create an account to save your work." : "Sign in to your saved workspace.";
    }

    private void SyncUser()
    {
        OnPropertyChanged(nameof(IsAuthenticated));
        OnPropertyChanged(nameof(IsAnonymous));
        OnPropertyChanged(nameof(CurrentUserText));
    }

    private void SetBusy(bool value)
    {
        if (SetField(ref _isBusy, value, nameof(IsBusy)))
        {
            OnPropertyChanged(nameof(CanSubmit));
            _submitCommand.ChangeCanExecute();
        }
    }

    private void SetRegisterMode(bool value)
    {
        if (!SetField(ref _isRegisterMode, value, nameof(IsRegisterMode)))
        {
            return;
        }
        OnPropertyChanged(nameof(ModeTitle));
        OnPropertyChanged(nameof(SubmitText));
        OnPropertyChanged(nameof(SwitchText));
        OnPropertyChanged(nameof(CanSubmit));
        _submitCommand.ChangeCanExecute();
    }

    private void SetFormField(ref string field, string value, string name)
    {
        if (!SetField(ref field, value, name))
        {
            return;
        }
        OnPropertyChanged(nameof(CanSubmit));
        _submitCommand.ChangeCanExecute();
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
