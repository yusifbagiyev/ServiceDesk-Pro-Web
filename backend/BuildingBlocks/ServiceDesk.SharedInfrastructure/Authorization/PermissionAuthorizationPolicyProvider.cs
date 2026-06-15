using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using ServiceDesk.SharedInfrastructure.Authentication;

namespace ServiceDesk.SharedInfrastructure.Authorization;

/// <summary>Builds an authorization policy on the fly for each <c>perm:{permission}</c> policy name.</summary>
internal sealed class PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options)
{
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(RequirePermissionAttribute.PolicyPrefix, StringComparison.Ordinal))
        {
            var permission = policyName[RequirePermissionAttribute.PolicyPrefix.Length..];

            return new AuthorizationPolicyBuilder(SessionConstants.Scheme)
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(permission))
                .Build();
        }

        return await base.GetPolicyAsync(policyName);
    }
}
