namespace ServiceDesk.SharedInfrastructure.Authentication;

/// <summary>Redis-backed BFF session store. The cookie carries only the opaque session id.</summary>
public interface ISessionStore
{
    /// <summary>Create a session and return its opaque id (to be set as the <c>_sid</c> cookie).</summary>
    Task<string> CreateAsync(SessionData data, CancellationToken cancellationToken = default);

    /// <summary>Load a session by id, refreshing its sliding TTL; null if missing/expired.</summary>
    Task<SessionData?> GetAsync(string sessionId, CancellationToken cancellationToken = default);

    Task RemoveAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>Revoke every session for a user (admin force-logout / claims change).</summary>
    Task RemoveAllForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Issue a fresh session id for an existing session (anti-fixation); null if not found.</summary>
    Task<string?> RotateAsync(string oldSessionId, DateTime nowUtc, CancellationToken cancellationToken = default);
}
