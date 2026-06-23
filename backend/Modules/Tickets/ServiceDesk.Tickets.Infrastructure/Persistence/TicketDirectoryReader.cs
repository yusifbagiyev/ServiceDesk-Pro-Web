using Microsoft.EntityFrameworkCore;
using ServiceDesk.Tickets.Application.Abstractions;

namespace ServiceDesk.Tickets.Infrastructure.Persistence;

internal sealed class TicketDirectoryReader(TicketsDbContext dbContext) : ITicketDirectoryReader
{
    public Task<UserLookup?> FindUserAsync(Guid userId, CancellationToken ct = default) =>
        dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new UserLookup(u.Id, u.FullName, u.IsActive))
            .FirstOrDefaultAsync(ct);

    public Task<CategoryLookup?> FindCategoryAsync(Guid categoryId, CancellationToken ct = default) =>
        dbContext.Categories
            .AsNoTracking()
            .Where(c => c.Id == categoryId)
            .Select(c => new CategoryLookup(c.Id, c.Name, c.IsActive))
            .FirstOrDefaultAsync(ct);
}
