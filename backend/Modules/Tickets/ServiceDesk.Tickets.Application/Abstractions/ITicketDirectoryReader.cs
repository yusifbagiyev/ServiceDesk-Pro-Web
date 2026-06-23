namespace ServiceDesk.Tickets.Application.Abstractions;

/// <summary>
/// Cross-module reads of users (Identity) and categories (Catalog) via the Tickets module's own
/// read-only model mappings (ExcludeFromMigrations) in TicketsDbContext - NOT a shared service.
/// Used to resolve and snapshot a name when assigning a user or adding a category to a ticket.
/// </summary>
public interface ITicketDirectoryReader
{
    Task<UserLookup?> FindUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<CategoryLookup?> FindCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
}

public sealed record UserLookup(Guid Id, string FullName, bool IsActive);

public sealed record CategoryLookup(Guid Id, string Name, bool IsActive);
