using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ServiceDesk.Application.Abstractions.Security;

namespace ServiceDesk.SharedInfrastructure.Authentication;

/// <summary>Resolves the authenticated caller from the request's <see cref="ClaimsPrincipal"/>.</summary>
internal sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public Guid UserId => Guid.Parse(
        Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("The request is not authenticated."));

    public string FullName => Principal?.FindFirstValue(ClaimTypes.Name)
        ?? throw new InvalidOperationException("The request is not authenticated.");

    public string? Role => Principal?.FindFirstValue(ClaimTypes.Role);

    public IReadOnlyCollection<string> Permissions =>
        Principal?.FindAll(SessionConstants.PermissionClaimType).Select(claim => claim.Value).ToArray() ?? [];

    public bool HasPermission(string permission) =>
        Principal?.HasClaim(SessionConstants.PermissionClaimType, permission) ?? false;

    public bool IsInRole(string role) => Principal?.IsInRole(role) ?? false;
}
