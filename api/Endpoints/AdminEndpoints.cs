using api.Services;

namespace api.Endpoints;

// Admin-only routes. Granting admin rights is deliberate and never something a
// user can do to themselves through registration, so it lives here behind the
// admin policy rather than anywhere on the public auth surface.
public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        var admin = app.MapGroup("/api/v1/admin").RequireAuthorization(RegulasAuthDefaults.AdminPolicy);
        admin.MapPut("users/{id:guid}/admin", SetAdmin);
    }

    private static Task<IResult> SetAdmin(Guid id, SetAdminRequest request, HttpContext context, AuthStore store)
    {
        return DatabaseRequest.Run(() => SetAdminCore(id, request, context, store));
    }

    private static async Task<IResult> SetAdminCore(Guid id, SetAdminRequest request, HttpContext context, AuthStore store)
    {
        var validation = ValidateTarget(id, context);
        if (validation is not null)
        {
            return validation;
        }
        var changed = await store.SetAdminAsync(id, request.IsAdmin ?? false);
        return changed ? Results.Ok(new AdminUserResponse(id, request.IsAdmin ?? false)) : UserMissing(id);
    }

    // An admin who demotes themselves could lock every admin out of the system,
    // so the last thing they can change is their own flag.
    private static IResult? ValidateTarget(Guid id, HttpContext context)
    {
        return id == CurrentUser.Id(context.User)
            ? Results.BadRequest("Admins cannot change their own admin rights.")
            : null;
    }

    private static IResult UserMissing(Guid id)
    {
        return Results.NotFound($"User with id {id} was not found.");
    }
}

public sealed record SetAdminRequest(bool? IsAdmin);

public sealed record AdminUserResponse(Guid Id, bool IsAdmin);
