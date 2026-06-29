using System.Data.Common;

namespace api.Services;

public interface IDatabaseConnectionFactory
{
    Task<DbConnection> OpenDatabaseConnectionAsync(CancellationToken cancellationToken = default);
}
