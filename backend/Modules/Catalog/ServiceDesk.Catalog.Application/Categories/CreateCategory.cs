using FluentValidation;
using ServiceDesk.Application.Abstractions.Messaging;
using ServiceDesk.Application.Abstractions.Time;
using ServiceDesk.Catalog.Application.Abstractions;
using ServiceDesk.Catalog.Domain;
using ServiceDesk.Kernel.Results;

namespace ServiceDesk.Catalog.Application.Categories;

public sealed record CreateCategoryCommand(string Name) : ICommand<Guid>;

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator() =>
        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
}

internal sealed class CreateCategoryCommandHandler(
    ICategoryRepository categories,
    ICatalogUnitOfWork unitOfWork,
    IDateTimeProvider clock) : ICommandHandler<CreateCategoryCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var normalized = Category.NormalizeName(command.Name);

        if (await categories.ExistsByNormalizedNameAsync(normalized, cancellationToken))
        {
            return CategoryErrors.NameTaken;
        }

        var category = Category.Create(command.Name.Trim(), clock.UtcNow);
        categories.Add(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}
