namespace ServiceDesk.SharedInfrastructure.Authentication;

/// <summary>The server-side session record, held in Redis. Only the opaque session id is in the cookie.</summary>
public sealed record SessionData(
    Guid UserId,
    string Email,
    string FullName,
    string Role,
    IReadOnlyList<string> Permissions,
    DateTime CreatedAtUtc,
    DateTime LastSeenAtUtc);
