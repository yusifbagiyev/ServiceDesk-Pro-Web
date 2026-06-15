using System.Security.Cryptography;
using System.Text.Json;
using StackExchange.Redis;

namespace ServiceDesk.SharedInfrastructure.Authentication;

internal sealed class RedisSessionStore(IConnectionMultiplexer redis) : ISessionStore
{
    private IDatabase Database => redis.GetDatabase();

    public async Task<string> CreateAsync(SessionData data, CancellationToken cancellationToken = default)
    {
        var sessionId = Base64Url(RandomNumberGenerator.GetBytes(32));
        var json = JsonSerializer.Serialize(data);

        await Database.StringSetAsync(SessionKey(sessionId), json, SessionConstants.IdleTimeout);
        await Database.SetAddAsync(UserKey(data.UserId), sessionId);

        return sessionId;
    }

    public async Task<SessionData?> GetAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var value = await Database.StringGetAsync(SessionKey(sessionId));
        if (value.IsNullOrEmpty)
        {
            return null;
        }

        var data = JsonSerializer.Deserialize<SessionData>((string)value!);
        if (data is null)
        {
            return null;
        }

        // Sliding expiration.
        await Database.KeyExpireAsync(SessionKey(sessionId), SessionConstants.IdleTimeout);
        return data;
    }

    public async Task RemoveAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var value = await Database.StringGetAsync(SessionKey(sessionId));
        if (!value.IsNullOrEmpty)
        {
            var data = JsonSerializer.Deserialize<SessionData>((string)value!);
            if (data is not null)
            {
                await Database.SetRemoveAsync(UserKey(data.UserId), sessionId);
            }
        }

        await Database.KeyDeleteAsync(SessionKey(sessionId));
    }

    public async Task RemoveAllForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var sessionIds = await Database.SetMembersAsync(UserKey(userId));
        foreach (var sessionId in sessionIds)
        {
            await Database.KeyDeleteAsync(SessionKey(sessionId.ToString()));
        }

        await Database.KeyDeleteAsync(UserKey(userId));
    }

    public async Task<string?> RotateAsync(
        string oldSessionId,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        var value = await Database.StringGetAsync(SessionKey(oldSessionId));
        if (value.IsNullOrEmpty)
        {
            return null;
        }

        var data = JsonSerializer.Deserialize<SessionData>((string)value!);
        if (data is null)
        {
            return null;
        }

        var newSessionId = await CreateAsync(data with { LastSeenAtUtc = nowUtc }, cancellationToken);
        await RemoveAsync(oldSessionId, cancellationToken);
        return newSessionId;
    }

    private static string SessionKey(string sessionId) => $"sess:{sessionId}";

    private static string UserKey(Guid userId) => $"usess:{userId}";

    private static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
}
