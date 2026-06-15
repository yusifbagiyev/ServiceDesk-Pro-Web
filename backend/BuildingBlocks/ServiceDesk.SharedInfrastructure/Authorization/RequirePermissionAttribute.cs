using Microsoft.AspNetCore.Authorization;

namespace ServiceDesk.SharedInfrastructure.Authorization;

/// <summary>
/// Requires the caller to hold a specific permission. Encodes the permission into a dynamic policy
/// name resolved by <c>PermissionAuthorizationPolicyProvider</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequirePermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "perm:";

    public RequirePermissionAttribute(string permission) => Policy = PolicyPrefix + permission;
}
