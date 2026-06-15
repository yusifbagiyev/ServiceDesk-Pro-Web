using Microsoft.AspNetCore.Authorization;
using ServiceDesk.SharedInfrastructure.Authentication;

namespace ServiceDesk.SharedInfrastructure.Authorization;

/// <summary>Grants a permission requirement if the caller holds the permission claim or is an Admin (role bypass).</summary>
internal sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.IsInRole(RolePermissions.AdminRole)
            || context.User.HasClaim(SessionConstants.PermissionClaimType, requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
