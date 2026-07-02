using System.Security.Claims;

namespace api.Services;

public static class CurrentUser
{
    public static Guid Id(ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : Guid.Empty;
    }

    public static string TokenHash(ClaimsPrincipal user)
    {
        return user.FindFirstValue(RegulasAuthDefaults.TokenHashClaim) ?? string.Empty;
    }
}
