using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace api.Services;

public sealed class RegulasBearerAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly AuthStore _store;

    public RegulasBearerAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        AuthStore store
    ) : base(options, logger, encoder)
    {
        _store = store;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var token = BearerToken();
        if (string.IsNullOrWhiteSpace(token))
        {
            return AuthenticateResult.NoResult();
        }
        return await AuthenticateToken(token);
    }

    private async Task<AuthenticateResult> AuthenticateToken(string token)
    {
        var tokenHash = AuthTokenService.Hash(token);
        var user = await _store.FindUserByTokenHashAsync(tokenHash);
        return user is null ? AuthenticateResult.Fail("Invalid bearer token.") : Success(user.Id, user.Email, user.DisplayName, tokenHash);
    }

    private AuthenticateResult Success(Guid userId, string email, string displayName, string tokenHash)
    {
        var identity = new ClaimsIdentity(Claims(userId, email, displayName, tokenHash), Scheme.Name);
        return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name));
    }

    private static List<Claim> Claims(Guid userId, string email, string displayName, string tokenHash)
    {
        return
        [
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, displayName),
            new Claim(RegulasAuthDefaults.TokenHashClaim, tokenHash),
        ];
    }

    private string? BearerToken()
    {
        var header = Request.Headers.Authorization.ToString();
        return header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? header[7..].Trim() : null;
    }
}
