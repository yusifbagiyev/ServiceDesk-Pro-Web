using Microsoft.EntityFrameworkCore;
using ServiceDesk.Catalog.Application.Abstractions;
using ServiceDesk.Catalog.Domain;

namespace ServiceDesk.Catalog.Infrastructure.Persistence;

internal sealed class CategoryRepository(CatalogDbContext dbContext) : ICategoryRepository
{
    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Categories.FirstOrDefaultAsync(category => category.Id == id, cancellationToken);

    public Task<bool> ExistsByNormalizedNameAsync(
        string normalizedName,
        CancellationToken cancellationToken = default) =>
        dbContext.Categories.AnyAsync(category => category.NameNormalized == normalizedName, cancellationToken);

    public async Task<IReadOnlyList<Category>> ListAsync(bool activeOnly, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Categories.AsNoTracking();

        if (activeOnly)
        {
            query = query.Where(category => category.IsActive);
        }

        return await query.OrderBy(category => category.NameNormalized).ToListAsync(cancellationToken);
    }

    public void Add(Category category) => dbContext.Categories.Add(category);
}
