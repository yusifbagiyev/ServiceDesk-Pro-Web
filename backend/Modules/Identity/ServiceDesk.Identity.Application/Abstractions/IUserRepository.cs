using ServiceDesk.Identity.Domain.Entity;

namespace ServiceDesk.Identity.Application.Abstractions;

/// <summary>Persistence gateway for the <see cref="User"/> aggregate (Identity module only).</summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<int> CountActiveAdminsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> ListAsync(CancellationToken cancellationToken = default);

    void Add(User user);
}
