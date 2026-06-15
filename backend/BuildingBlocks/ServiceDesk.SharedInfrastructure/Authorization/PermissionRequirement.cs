using Microsoft.AspNetCore.Authorization;

namespace ServiceDesk.SharedInfrastructure.Authorization;

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
