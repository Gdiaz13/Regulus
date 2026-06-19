using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public static class DatabaseRequest
{
    public static async Task<IResult> Run(Func<Task<IResult>> action)
    {
        try
        {
            return await action();
        }
        catch (DbException)
        {
            return Unavailable();
        }
        catch (DbUpdateException)
        {
            return Unavailable();
        }
    }

    private static IResult Unavailable()
    {
        return Results.Problem("Database is unavailable.", statusCode: StatusCodes.Status503ServiceUnavailable);
    }
}
