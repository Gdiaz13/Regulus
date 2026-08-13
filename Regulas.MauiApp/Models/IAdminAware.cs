namespace Regulas.MauiApp.Models;

// Mirrors the backend's IAdminAware so the app asks the admin question the same
// way the API answers it. The flag is only ever read from the API, never set
// here: the server decides who is an admin.
public interface IAdminAware
{
    bool IsAdmin { get; }
}
