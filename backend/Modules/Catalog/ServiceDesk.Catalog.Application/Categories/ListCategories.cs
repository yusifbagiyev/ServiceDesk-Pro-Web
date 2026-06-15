using ServiceDesk.Application.Abstractions.Messaging;
using ServiceDesk.Catalog.Application.Abstractions;
using ServiceDesk.Kernel.Results;

namespace ServiceDesk.Catalog.Application.Categories;

public sealed record ListCategoriesQuery(bool ActiveOnly) : IQuery<IReadOnlyList<CategoryListItem>>;

public sealed record CategoryListItem(Guid Id, string Name, bool IsActive);

internal sealed class ListCategoriesQueryHandler(ICategoryRepository categories)
    : IQueryHandler<ListCategoriesQuery, IReadOnlyList<CategoryListItem>>
{
    public async Task<Result<IReadOnlyList<CategoryListItem>>> Handle(
        ListCategoriesQuery query,
        CancellationToken cancellationToken)
    {
        var all = await categories.ListAsync(query.ActiveOnly, cancellationToken);

        IReadOnlyList<CategoryListItem> items = all
            .Select(category => new CategoryListItem(category.Id, category.Name, category.IsActive))
            .ToList();

        return Result.Success(items);
    }
}
