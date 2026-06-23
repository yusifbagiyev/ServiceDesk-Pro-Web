using FluentValidation;
using ServiceDesk.Application.Abstractions.Messaging;
using ServiceDesk.Application.Abstractions.Time;
using ServiceDesk.Tickets.Application.Abstractions;
using ServiceDesk.Tickets.Domain;
using ServiceDesk.Tickets.Domain.Enums;
using ServiceDesk.Tickets.Domain.Exceptions;
using ServiceDesk.Kernel.Results;

namespace ServiceDesk.Tickets.Application.Commands;

public sealed record CreateBackdatedClosedTicketCommand(
    Guid? CreatedByUserId,
    Guid? ReporterUserId,
    string Priority,
    DateTime ClosedAtUtc,
    string? Title,
    string Solution,
    string? InventoryCode,
    IReadOnlyList<Guid> CategoryIds,
    IReadOnlyList<Guid> AssigneeUserIds) : ICommand<Guid>;

public sealed class CreateBackdatedClosedTicketCommandValidator : AbstractValidator<CreateBackdatedClosedTicketCommand>
{
    public CreateBackdatedClosedTicketCommandValidator()
    {
        RuleFor(c => c.Priority)
            .NotEmpty()
            .Must(p => Enum.TryParse<TicketPriority>(p, true, out _))
            .WithMessage("Priority must be a valid ticket priority.");
        RuleFor(c => c.Solution).NotEmpty();
        RuleFor(c => c.ClosedAtUtc).NotEmpty();
    }
}

internal sealed class CreateBackdatedClosedTicketCommandHandler(
    ITicketRepository tickets,
    ISlaPolicyRepository slaPolicies,
    ITicketDirectoryReader directory,
    ITicketsUnitOfWork unitOfWork,
    IDateTimeProvider clock) : ICommandHandler<CreateBackdatedClosedTicketCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateBackdatedClosedTicketCommand command, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;

        var resolvedAssignees = new List<(Guid Id, string FullName)>();
        foreach (var assigneeId in command.AssigneeUserIds)
        {
            var user = await directory.FindUserAsync(assigneeId, cancellationToken);
            if (user is null || !user.IsActive)
            {
                return TicketErrors.UserNotFound(assigneeId);
            }

            resolvedAssignees.Add((user.Id, user.FullName));
        }

        var resolvedCategories = new List<(Guid Id, string Name)>();
        foreach (var categoryId in command.CategoryIds)
        {
            var category = await directory.FindCategoryAsync(categoryId, cancellationToken);
            if (category is null || !category.IsActive)
            {
                return TicketErrors.CategoryNotFound(categoryId);
            }

            resolvedCategories.Add((category.Id, category.Name));
        }

        var priority = Enum.Parse<TicketPriority>(command.Priority, ignoreCase: true);

        var ticket = Ticket.CreateBackdatedClosed(
            command.ClosedAtUtc,
            now,
            priority,
            command.CreatedByUserId,
            command.ReporterUserId ?? command.CreatedByUserId,
            command.InventoryCode?.Trim(),
            null,
            null,
            null,
            command.Title?.Trim(),
            command.Solution.Trim());

        foreach (var (id, fullName) in resolvedAssignees)
        {
            ticket.AssignUser(id, fullName, now);
        }

        foreach (var (id, name) in resolvedCategories)
        {
            ticket.AddCategory(id, name, now);
        }

        var policy = await slaPolicies.GetActiveByPriorityAsync(priority, cancellationToken);
        if (policy is not null)
        {
            ticket.ApplySla(policy, now);
        }

        tickets.Add(ticket);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ticket.Id;
    }
}
