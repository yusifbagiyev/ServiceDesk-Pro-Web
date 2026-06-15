namespace ServiceDesk.Application.Abstractions.Security;

/// <summary>
/// The authenticated caller for the current request, hydrated from the BFF session by middleware.
/// Handlers use this to scope queries to the caller and to read permissions.
/// </summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    /// <summary>The caller's user id. Throws if the request is unauthenticated.</summary>
    Guid UserId { get; }

    /// <summary>The caller's login handle (fullname). Throws if unauthenticated.</summary>
    string FullName { get; }

    /// <summary>The caller's role name (e.g. "User", "Admin").</summary>
    string? Role { get; }

    IReadOnlyCollection<string> Permissions { get; }

    bool HasPermission(string permission);

    bool IsInRole(string role);
}
