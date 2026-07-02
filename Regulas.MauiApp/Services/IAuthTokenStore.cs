namespace Regulas.MauiApp.Services;

public interface IAuthTokenStore
{
    Task<string?> GetAsync();

    Task SaveAsync(string token);

    Task ClearAsync();
}
