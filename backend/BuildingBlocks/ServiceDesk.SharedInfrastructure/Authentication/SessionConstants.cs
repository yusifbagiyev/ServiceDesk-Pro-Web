namespace ServiceDesk.SharedInfrastructure.Authentication;

public static class SessionConstants
{
    public const string Scheme = "ServiceDeskSession";

    public const string CookieName = "_sid";

    public const string PermissionClaimType = "permission";

    /// <summary>Sliding idle timeout; refreshed on each authenticated request.</summary>
    public static readonly TimeSpan IdleTimeout = TimeSpan.FromMinutes(30);
}
