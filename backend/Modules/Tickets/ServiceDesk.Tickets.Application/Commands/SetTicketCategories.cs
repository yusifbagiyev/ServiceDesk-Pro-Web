using FluentValidation;
using ServiceDesk.Application.Abstractions.Messaging;
using ServiceDesk.Application.Abstractions.Time;
using ServiceDesk.Tickets.Application.Abstractions;
using ServiceDesk.Tickets.Domain.Exceptions;
using ServiceDesk.Kernel.Results;

namespace ServiceDesk.Tickets.Application.Commands;

public sealed record SetTicketCategoriesCommand(Guid TicketId, IReadOnlyList<Guid> CategoryIds) : ICommand;

public sealed class SetTicketCategoriesCommandValidator : AbstractValidator<SetTicketCategoriesCommand>
{
    public SetTicketCategoriesCommandValidator()
    {
        RuleFor(c => c.TicketId).NotEmpty();
        RuleFor(c => c.CategoryIds).NotEmpty();
    }
}

internal sealed class SetTicketCategoriesCommandHandler(
    ITicketRepository tickets,
    ITicketDirectoryReader directory,
    ITicketsUnitOfWork unitOfWork,
    IDateTimeProvider clock) : ICommandHandler<SetTicketCategoriesCommand>
{
    public async Task<Result> Handle(SetTicketCategoriesCommand command, CancellationToken cancellationToken)
    {
        var ticket = await tickets.GetByIdAsync(command.TicketId, cancellationToken);
        if (ticket is null)
        {
            return TicketErrors.NotFound(command.TicketId);
        }

        var resolved = new List<(Guid Id, string Name)>();
        foreach (var id in command.CategoryIds)
        {
            var cat = await directory.FindCategoryAsync(id, cancellationToken);
            if (cat is null || !cat.IsActive)
            {
                return TicketErrors.CategoryNotFound(id);
            }

            resolved.Add((cat.Id, cat.Name));
        }

        var now = clock.UtcNow;
        ticket.ClearCategories(now);
        foreach (var (id, name) in resolved)
        {
            ticket.AddCategory(id, name, now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
