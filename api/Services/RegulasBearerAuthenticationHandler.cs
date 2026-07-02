using System.Security.Claims;
using System.Text.Encodings.Web;
using api.Models;
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
        return await AuthenticateTokenAsync(token);
    }

    private async Task<AuthenticateResult> AuthenticateTokenAsync(string token)
    {
        var tokenHash = AuthTokenService.Hash(token);
        var user = await _store.FindUserByTokenHashAsync(tokenHash);
        return user is null ? AuthenticateResult.Fail("Invalid bearer token.") : Success(user, tokenHash);
    }

    private AuthenticateResult Success(RegulasUser user, string tokenHash)
    {
        var identity = new ClaimsIdentity(Claims(user, tokenHash), Scheme.Name);
        return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name));
    }

    private static List<Claim> Claims(RegulasUser user, string tokenHash)
    {
        return
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.DisplayName),
            new Claim(RegulasAuthDefaults.TokenHashClaim, tokenHash),
        ];
    }

    private string? BearerToken()
    {
        var header = Request.Headers.Authorization.ToString();
        return header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? header[7..].Trim() : null;
    }
}
