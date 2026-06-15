using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ServiceDesk.SharedInfrastructure.Authentication;

namespace ServiceDesk.SharedInfrastructure.Middleware;

/// <summary>Allowed browser origins for cookie-authenticated state-changing requests.</summary>
public sealed class CsrfProtectionOptions
{
    public HashSet<string> AllowedOrigins { get; } = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// For unsafe methods carrying the session cookie, validates Origin (then Referer) against the
/// allowlist. Bearer/websocket and non-browser clients (no Origin/Referer) are unaffected.
/// </summary>
public sealed class CsrfProtectionMiddleware(RequestDelegate next, CsrfProtectionOptions options)
{
    private static readonly HashSet<string> SafeMethods =
        new(StringComparer.OrdinalIgnoreCase) { "GET", "HEAD", "OPTIONS", "TRACE" };

    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;

        if (!SafeMethods.Contains(request.Method)
            && request.Cookies.ContainsKey(SessionConstants.CookieName)
            && !IsTrustedOrigin(request))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("CSRF validation failed.");
            return;
        }

        await next(context);
    }

    private bool IsTrustedOrigin(HttpRequest request)
    {
        var origin = request.Headers.Origin.ToString();
        if (!string.IsNullOrEmpty(origin))
        {
            return IsAllowed(origin);
        }

        var referer = request.Headers.Referer.ToString();
        if (!string.IsNullOrEmpty(referer) && Uri.TryCreate(referer, UriKind.Absolute, out var refererUri))
        {
            return IsAllowed($"{refererUri.Scheme}://{refererUri.Authority}");
        }

        // No Origin/Referer: not a browser cross-site form post, allow (API clients).
        return true;
    }

    private bool IsAllowed(string origin) =>
        options.AllowedOrigins.Count == 0 || options.AllowedOrigins.Contains(origin);
}

public static class CsrfProtectionMiddlewareExtensions
{
    public static IApplicationBuilder UseCsrfProtection(this IApplicationBuilder app) =>
        app.UseMiddleware<CsrfProtectionMiddleware>();
}
