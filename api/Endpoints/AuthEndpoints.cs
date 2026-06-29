using api.Contracts;
using api.Services;

namespace api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var auth = app.MapGroup("/api/v1/auth");
        auth.MapPost("register", Register);
        auth.MapPost("login", Login);
        auth.MapPost("logout", Logout).RequireAuthorization();
        auth.MapGet("me", Me).RequireAuthorization();
    }

    private static Task<IResult> Register(AuthRegisterRequest request, AuthService service)
    {
        return RunAuth(() => RegisterCore(request, service));
    }

    private static async Task<IResult> RegisterCore(AuthRegisterRequest request, AuthService service)
    {
        var validation = ValidateRegister(request);
        if (validation is not null)
        {
            return validation;
        }
        return ToRegisterResult(await service.RegisterAsync(Clean(request.Email), Clean(request.Password), DisplayName(request)));
    }

    private static Task<IResult> Login(AuthLoginRequest request, AuthService service)
    {
        return RunAuth(() => LoginCore(request, service));
    }

    private static async Task<IResult> LoginCore(AuthLoginRequest request, AuthService service)
    {
        var validation = ValidateLogin(request);
        if (validation is not null)
        {
            return validation;
        }
        return ToLoginResult(await service.LoginAsync(Clean(request.Email), Clean(request.Password)));
    }

    private static Task<IResult> Logout(HttpContext context, AuthService service)
    {
        return RunAuth(async () =>
        {
            await service.LogoutAsync(CurrentUser.TokenHash(context.User));
            return Results.NoContent();
        });
    }

    private static Task<IResult> Me(HttpContext context, AuthService service)
    {
        return RunAuth(() => MeCore(context, service));
    }

    private static async Task<IResult> MeCore(HttpContext context, AuthService service)
    {
        var user = await service.CurrentUserAsync(CurrentUser.Id(context.User));
        return user is null ? Results.Unauthorized() : Results.Ok(user);
    }

    private static async Task<IResult> RunAuth(Func<Task<IResult>> action)
    {
        try
        {
            return await action();
        }
        catch (Exception exception) when (DatabaseRequest.IsDatabaseException(exception))
        {
            return Results.Problem("Database is unavailable.", statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }

    private static IResult ToRegisterResult(AuthServiceResult result)
    {
        return result.Status is AuthServiceStatus.EmailTaken
            ? Results.Conflict("An account already exists for that email.")
            : Results.Created("/api/v1/auth/me", result.Response);
    }

    private static IResult ToLoginResult(AuthServiceResult result)
    {
        return result.Status is AuthServiceStatus.InvalidLogin
            ? Results.Unauthorized()
            : Results.Ok(result.Response);
    }

    private static IResult? ValidateRegister(AuthRegisterRequest request)
    {
        return ValidateEmail(request.Email) ?? ValidatePassword(request.Password) ?? ValidateDisplayName(request.DisplayName);
    }

    private static IResult? ValidateLogin(AuthLoginRequest request)
    {
        return ValidateEmail(request.Email) ?? ValidatePassword(request.Password);
    }

    private static IResult? ValidateEmail(string? email)
    {
        return IsBlank(email) || !email.Contains('@') ? Results.BadRequest("A valid email is required.") : null;
    }

    private static IResult? ValidatePassword(string? password)
    {
        return string.IsNullOrWhiteSpace(password) || password.Length < 8
            ? Results.BadRequest("Password must be at least 8 characters.")
            : null;
    }

    private static IResult? ValidateDisplayName(string? displayName)
    {
        return IsBlank(displayName) ? Results.BadRequest("A display name is required.") : null;
    }

    private static string DisplayName(AuthRegisterRequest request)
    {
        return Clean(request.DisplayName);
    }

    private static string Clean(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }

    private static bool IsBlank(string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }
}
