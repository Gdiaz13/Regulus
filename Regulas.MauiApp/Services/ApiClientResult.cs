namespace Regulas.MauiApp.Services;

public sealed record ApiClientResult<T>(bool Ok, T? Data, string Message)
{
    public static ApiClientResult<T> Success(T data)
    {
        return new ApiClientResult<T>(true, data, string.Empty);
    }

    public static ApiClientResult<T> Failure(string message)
    {
        return new ApiClientResult<T>(false, default, message);
    }
}
