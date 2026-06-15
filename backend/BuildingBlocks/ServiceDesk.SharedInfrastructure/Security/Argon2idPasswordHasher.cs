using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using ServiceDesk.Application.Abstractions.Security;

namespace ServiceDesk.SharedInfrastructure.Security;

/// <summary>
/// Argon2id password hashing. Produces a self-describing PHC-style string
/// (<c>$argon2id$v=19$m=..,t=..,p=..$salt$hash</c>) so parameters can evolve, and verifies
/// in constant time.
/// </summary>
public sealed class Argon2idPasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int MemoryKib = 19456;
    private const int Iterations = 2;
    private const int Parallelism = 1;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Compute(password, salt, MemoryKib, Iterations, Parallelism, HashSize);

        return string.Create(
            CultureInfo.InvariantCulture,
            $"$argon2id$v=19$m={MemoryKib},t={Iterations},p={Parallelism}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}");
    }

    public bool Verify(string password, string hash)
    {
        try
        {
            var parts = hash.Split('$', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 5 || parts[0] != "argon2id")
            {
                return false;
            }

            var perf = parts[2].Split(',');
            var memoryKib = int.Parse(perf[0].AsSpan(2), CultureInfo.InvariantCulture);
            var iterations = int.Parse(perf[1].AsSpan(2), CultureInfo.InvariantCulture);
            var parallelism = int.Parse(perf[2].AsSpan(2), CultureInfo.InvariantCulture);

            var salt = Convert.FromBase64String(parts[3]);
            var expected = Convert.FromBase64String(parts[4]);
            var actual = Compute(password, salt, memoryKib, iterations, parallelism, expected.Length);

            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (Exception ex) when (ex is FormatException or IndexOutOfRangeException or ArgumentException)
        {
            return false;
        }
    }

    private static byte[] Compute(string password, byte[] salt, int memoryKib, int iterations, int parallelism, int hashSize)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            MemorySize = memoryKib,
            Iterations = iterations,
            DegreeOfParallelism = parallelism,
        };

        return argon2.GetBytes(hashSize);
    }
}
