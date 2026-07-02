using System.ComponentModel;
using System.Runtime.CompilerServices;
using Regulas.MauiApp.Models;

namespace Regulas.MauiApp.Services;

public sealed class AuthSession : INotifyPropertyChanged
{
    private readonly IRegulasApiClient _apiClient;
    private readonly IAuthTokenStore _tokens;
    private CurrentUser? _currentUser;

    public AuthSession(IRegulasApiClient apiClient, IAuthTokenStore tokens)
    {
        _apiClient = apiClient;
        _tokens = tokens;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public CurrentUser? CurrentUser { get => _currentUser; private set => SetUser(value); }

    public bool IsAuthenticated => CurrentUser is not null;

    public async Task RefreshAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(await _tokens.GetAsync()))
        {
            CurrentUser = null;
            return;
        }
        await ApplyCurrentUserAsync(cancellationToken);
    }

    public Task<ApiClientResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken token)
    {
        return AuthenticateAsync(() => _apiClient.LoginAsync(request, token));
    }

    public Task<ApiClientResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken token)
    {
        return AuthenticateAsync(() => _apiClient.RegisterAsync(request, token));
    }

    public async Task LogoutAsync(CancellationToken cancellationToken)
    {
        await _apiClient.LogoutAsync(cancellationToken);
        await _tokens.ClearAsync();
        CurrentUser = null;
    }

    private async Task ApplyCurrentUserAsync(CancellationToken cancellationToken)
    {
        var result = await _apiClient.GetCurrentUserAsync(cancellationToken);
        CurrentUser = result.Ok ? result.Data : null;
        if (!result.Ok)
        {
            await _tokens.ClearAsync();
        }
    }

    private async Task<ApiClientResult<AuthResponse>> AuthenticateAsync(AuthRequest request)
    {
        var result = await request();
        if (result.Ok && result.Data is not null)
        {
            await SignInAsync(result.Data);
        }
        return result;
    }

    private async Task SignInAsync(AuthResponse response)
    {
        await _tokens.SaveAsync(response.Token);
        CurrentUser = response.User;
    }

    private void SetUser(CurrentUser? user)
    {
        if (_currentUser == user)
        {
            return;
        }
        _currentUser = user;
        OnPropertyChanged(nameof(CurrentUser));
        OnPropertyChanged(nameof(IsAuthenticated));
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private delegate Task<ApiClientResult<AuthResponse>> AuthRequest();
}
