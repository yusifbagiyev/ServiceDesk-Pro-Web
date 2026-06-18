using FluentValidation;
using ServiceDesk.Application.Abstractions.Messaging;
using ServiceDesk.Application.Abstractions.Time;
using ServiceDesk.Catalog.Application.Abstractions;
using ServiceDesk.Catalog.Domain.Entity;
using ServiceDesk.Catalog.Domain.Exceptions;
using ServiceDesk.Kernel.Results;

namespace ServiceDesk.Catalog.Application.Commands;

public sealed record RenameCategoryCommand(Guid CategoryId, string Name) : ICommand;

public sealed class RenameCategoryCommandValidator : AbstractValidator<RenameCategoryCommand>
{
    public RenameCategoryCommandValidator()
    {
        RuleFor(c => c.CategoryId).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
    }
}

internal sealed class RenameCategoryCommandHandler(
    ICategoryRepository categories,
    ICatalogUnitOfWork unitOfWork,
    IDateTimeProvider clock) : ICommandHandler<RenameCategoryCommand>
{
    public async Task<Result> Handle(RenameCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await categories.GetByIdAsync(command.CategoryId, cancellationToken);
        if (category is null)
        {
            return CategoryErrors.NotFound(command.CategoryId);
        }

        var normalized = Category.NormalizeName(command.Name);

        if (normalized != category.NameNormalized
            && await categories.ExistsByNormalizedNameAsync(normalized, cancellationToken))
        {
            return CategoryErrors.NameTaken;
        }

        category.Rename(command.Name.Trim(), clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
