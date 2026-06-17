using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Application.Abstractions.Time;
using ServiceDesk.Identity.Application.Commands;
using ServiceDesk.Identity.Application.DTOs;
using ServiceDesk.SharedInfrastructure.Authentication;
using ServiceDesk.SharedInfrastructure.Authorization;
using ServiceDesk.SharedInfrastructure.Web;

namespace ServiceDesk.Identity.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed partial class AuthController(ISender sender, ISessionStore sessionStore, IDateTimeProvider clock) : ControllerBase
{
    /// <summary>Authenticate by email + password; on success starts a BFF session and sets the _sid cookie.</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new LoginCommand(request.Email, request.Password), cancellationToken);
        if (result.IsFailure)
        {
            return result.ToActionResult(this);
        }

        var user = result.Value;
        var permissions = RolePermissions.For(user.Role);
        var now = clock.UtcNow;
        var session = new SessionData(user.UserId, user.Email, user.FullName, user.Role, permissions, now, now);
        var sessionId = await sessionStore.CreateAsync(session, cancellationToken);
        SetSessionCookie(sessionId);

        return Ok(new
        {
            user.UserId,
            user.Email,
            user.FullName,
            user.Role,
            Permissions = permissions,
        });
    }

    /// <summary>Revoke the current session and clear the cookie.</summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (Request.Cookies.TryGetValue(SessionConstants.CookieName, out var sessionId)
            && !string.IsNullOrEmpty(sessionId))
        {
            await sessionStore.RemoveAsync(sessionId, cancellationToken);
        }

        Response.Cookies.Delete(SessionConstants.CookieName);
        return NoContent();
    }

    /// <summary>Rotate the session id (anti-fixation) and extend it; 401 if there is no live session.</summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue(SessionConstants.CookieName, out var sessionId)
            || string.IsNullOrEmpty(sessionId))
        {
            return Unauthorized();
        }

        var newSessionId = await sessionStore.RotateAsync(sessionId, clock.UtcNow, cancellationToken);
        if (newSessionId is null)
        {
            return Unauthorized();
        }

        SetSessionCookie(newSessionId);
        return NoContent();
    }

    private void SetSessionCookie(string sessionId) =>
        Response.Cookies.Append(SessionConstants.CookieName, sessionId, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            MaxAge = SessionConstants.IdleTimeout,
        });
}
