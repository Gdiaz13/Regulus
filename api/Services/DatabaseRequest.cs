using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public static class DatabaseRequest
{
    // Endpoint handlers use this so database outages become one clear API response.
    public static async Task<IResult> Run(Func<Task<IResult>> action)
    {
        try
        {
            return await action();
        }
        catch (Exception exception) when (IsDatabaseException(exception))
        {
            return Unavailable();
        }
    }

    private static bool IsDatabaseException(Exception exception)
    {
        return exception is DbException or DbUpdateException or InvalidOperationException;
    }

    private static IResult Unavailable()
    {
        return Results.Problem("Database is unavailable.", statusCode: StatusCodes.Status503ServiceUnavailable);
    }
}
