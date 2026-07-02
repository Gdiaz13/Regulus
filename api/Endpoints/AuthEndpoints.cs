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

    private static Task<IResult> Register(RegisterRequest request, AuthService service)
    {
        return DatabaseRequest.Run(() => RegisterCore(request, service));
    }

    private static async Task<IResult> RegisterCore(RegisterRequest request, AuthService service)
    {
        var validation = ValidateRegister(request);
        if (validation is not null)
        {
            return validation;
        }
        return RegisterResult(await service.RegisterAsync(request));
    }

    private static Task<IResult> Login(LoginRequest request, AuthService service)
    {
        return DatabaseRequest.Run(() => LoginCore(request, service));
    }

    private static async Task<IResult> LoginCore(LoginRequest request, AuthService service)
    {
        var validation = ValidateLogin(request);
        if (validation is not null)
        {
            return validation;
        }
        return LoginResult(await service.LoginAsync(request));
    }

    private static Task<IResult> Logout(HttpContext context, AuthService service)
    {
        return DatabaseRequest.Run(async () =>
        {
            await service.LogoutAsync(CurrentUser.TokenHash(context.User));
            return Results.NoContent();
        });
    }

    private static Task<IResult> Me(HttpContext context, AuthService service)
    {
        return DatabaseRequest.Run(async () =>
        {
            var user = await service.CurrentUserAsync(CurrentUser.Id(context.User));
            return user is null ? Results.Unauthorized() : Results.Ok(user);
        });
    }

    private static IResult RegisterResult(AuthResult result)
    {
        return result.Status is AuthResultStatus.EmailTaken
            ? Results.Conflict("An account already exists for that email.")
            : Results.Created("/api/v1/auth/me", result.Response);
    }

    private static IResult LoginResult(AuthResult result)
    {
        return result.Status is AuthResultStatus.InvalidLogin ? Results.Unauthorized() : Results.Ok(result.Response);
    }

    private static IResult? ValidateRegister(RegisterRequest request)
    {
        return ValidateEmail(request.Email) ?? ValidatePassword(request.Password) ?? ValidateDisplayName(request.DisplayName);
    }

    private static IResult? ValidateLogin(LoginRequest request)
    {
        return ValidateEmail(request.Email) ?? ValidatePassword(request.Password);
    }

    private static IResult? ValidateEmail(string? email)
    {
        return string.IsNullOrWhiteSpace(email) || !email.Contains('@')
            ? Results.BadRequest("A valid email is required.")
            : null;
    }

    private static IResult? ValidatePassword(string? password)
    {
        return string.IsNullOrWhiteSpace(password) || password.Length < 8
            ? Results.BadRequest("Password must be at least 8 characters.")
            : null;
    }

    private static IResult? ValidateDisplayName(string? displayName)
    {
        return string.IsNullOrWhiteSpace(displayName) ? Results.BadRequest("A display name is required.") : null;
    }
}
