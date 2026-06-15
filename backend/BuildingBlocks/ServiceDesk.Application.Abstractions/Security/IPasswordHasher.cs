namespace ServiceDesk.Application.Abstractions.Security;

/// <summary>
/// Hashes and verifies passwords. The Argon2id implementation lives in the auth infrastructure;
/// the Identity module depends only on this abstraction.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Produce a self-describing PHC-format hash (algorithm + params + salt embedded).</summary>
    string Hash(string password);

    /// <summary>Constant-time verify of a password against a stored hash.</summary>
    bool Verify(string password, string hash);
}
