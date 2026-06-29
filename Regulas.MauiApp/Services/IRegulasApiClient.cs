using Regulas.MauiApp.Models;

namespace Regulas.MauiApp.Services;

public interface IRegulasApiClient
{
    Task<ApiClientResult<ApiHealth>> GetHealthAsync(CancellationToken cancellationToken);

    Task<ApiClientResult<IReadOnlyList<PortfolioStock>>> GetPortfolioStocksAsync(CancellationToken cancellationToken);
}
