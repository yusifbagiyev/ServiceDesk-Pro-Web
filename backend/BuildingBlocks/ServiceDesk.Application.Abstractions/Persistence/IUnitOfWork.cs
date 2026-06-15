namespace ServiceDesk.Application.Abstractions.Persistence;

/// <summary>
/// Per-module unit of work. The transaction pipeline behavior calls
/// <see cref="SaveChangesAsync"/> once after a successful command handler;
/// the SaveChanges interceptor then dispatches domain events after commit.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
