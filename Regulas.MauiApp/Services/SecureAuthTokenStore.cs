namespace Regulas.MauiApp.Services;

public sealed class SecureAuthTokenStore : IAuthTokenStore
{
    private const string TokenKey = "RegulasAuthToken";

    public Task<string?> GetAsync()
    {
        return SecureStorage.Default.GetAsync(TokenKey);
    }

    public Task SaveAsync(string token)
    {
        return SecureStorage.Default.SetAsync(TokenKey, token);
    }

    public Task ClearAsync()
    {
        SecureStorage.Default.Remove(TokenKey);
        return Task.CompletedTask;
    }
}
