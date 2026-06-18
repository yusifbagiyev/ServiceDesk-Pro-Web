using FluentValidation;
using ServiceDesk.Application.Abstractions.Messaging;
using ServiceDesk.Application.Abstractions.Time;
using ServiceDesk.Catalog.Application.Abstractions;
using ServiceDesk.Catalog.Domain.Exceptions;
using ServiceDesk.Kernel.Results;

namespace ServiceDesk.Catalog.Application.Commands;

public sealed record SetCategoryActivationCommand(Guid CategoryId, bool IsActive) : ICommand;

public sealed class SetCategoryActivationCommandValidator : AbstractValidator<SetCategoryActivationCommand>
{
    public SetCategoryActivationCommandValidator() => RuleFor(c => c.CategoryId).NotEmpty();
}

internal sealed class SetCategoryActivationCommandHandler(
    ICategoryRepository categories,
    ICatalogUnitOfWork unitOfWork,
    IDateTimeProvider clock) : ICommandHandler<SetCategoryActivationCommand>
{
    public async Task<Result> Handle(SetCategoryActivationCommand command, CancellationToken cancellationToken)
    {
        var category = await categories.GetByIdAsync(command.CategoryId, cancellationToken);
        if (category is null)
        {
            return CategoryErrors.NotFound(command.CategoryId);
        }

        category.SetActive(command.IsActive, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
