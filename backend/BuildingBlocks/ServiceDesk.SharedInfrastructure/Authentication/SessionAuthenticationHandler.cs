using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ServiceDesk.SharedInfrastructure.Authentication;

/// <summary>
/// Reads the opaque <c>_sid</c> cookie, loads the server-side session from Redis, and builds the
/// <see cref="ClaimsPrincipal"/> (id, name, email, role, permission claims). No token in the browser.
/// </summary>
internal sealed class SessionAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    ISessionStore sessionStore) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Cookies.TryGetValue(SessionConstants.CookieName, out var sessionId)
            || string.IsNullOrEmpty(sessionId))
        {
            return AuthenticateResult.NoResult();
        }

        var session = await sessionStore.GetAsync(sessionId);
        if (session is null)
        {
            return AuthenticateResult.Fail("Invalid or expired session.");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, session.UserId.ToString()),
            new(ClaimTypes.Name, session.FullName),
            new(ClaimTypes.Email, session.Email),
            new(ClaimTypes.Role, session.Role),
        };
        claims.AddRange(session.Permissions.Select(permission =>
            new Claim(SessionConstants.PermissionClaimType, permission)));

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }
}
