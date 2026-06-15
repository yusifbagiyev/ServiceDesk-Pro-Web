using Microsoft.EntityFrameworkCore;
using ServiceDesk.Identity.Application.Abstractions;
using ServiceDesk.Identity.Domain.Entity;
using ServiceDesk.Identity.Domain.Enums;

namespace ServiceDesk.Identity.Infrastructure.Persistence;

internal sealed class UserRepository(IdentityDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Users.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        email = User.NormalizeEmail(email);
        return dbContext.Users.FirstOrDefaultAsync(user => user.Email == email, cancellationToken);
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        email = User.NormalizeEmail(email);
        return dbContext.Users.AnyAsync(user => user.Email == email, cancellationToken);
    }

    public Task<int> CountActiveAdminsAsync(CancellationToken cancellationToken = default) =>
        dbContext.Users.CountAsync(user => user.IsActive && user.Role == UserRole.Admin, cancellationToken);

    public async Task<IReadOnlyList<User>> ListAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Users
            .AsNoTracking()
            .OrderBy(user => user.FullName)
            .ToListAsync(cancellationToken);

    public void Add(User user) => dbContext.Users.Add(user);
}
