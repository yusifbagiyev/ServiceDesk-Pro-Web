using ServiceDesk.Application.Abstractions.Persistence;
using ServiceDesk.Catalog.Domain;

namespace ServiceDesk.Catalog.Application.Abstractions;

/// <summary>Persistence gateway for the <see cref="Category"/> aggregate.</summary>
public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Category>> ListAsync(bool activeOnly, CancellationToken cancellationToken = default);

    void Add(Category category);
}

/// <summary>The Catalog module's unit of work (its DbContext), registered under this module-specific interface.</summary>
public interface ICatalogUnitOfWork : IUnitOfWork;
