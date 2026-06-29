using Regulas.MauiApp.Models;

namespace Regulas.MauiApp.Services;

public interface IRegulasApiClient
{
    Task<ApiClientResult<ApiHealth>> GetHealthAsync(CancellationToken cancellationToken);

    Task<ApiClientResult<IReadOnlyList<PortfolioStock>>> GetPortfolioStocksAsync(CancellationToken cancellationToken);

    Task<ApiClientResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    Task<ApiClientResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);

    Task<ApiClientResult<CurrentUser>> GetCurrentUserAsync(CancellationToken cancellationToken);

    Task<ApiClientResult<bool>> LogoutAsync(CancellationToken cancellationToken);
}
