using FluentValidation;
using ServiceDesk.Application.Abstractions.Messaging;
using ServiceDesk.Application.Abstractions.Time;
using ServiceDesk.Tickets.Application.Abstractions;
using ServiceDesk.Tickets.Domain.Enums;
using ServiceDesk.Tickets.Domain.Exceptions;
using ServiceDesk.Kernel.Results;

namespace ServiceDesk.Tickets.Application.Commands;

public sealed record SetTicketPriorityCommand(Guid TicketId, string Priority) : ICommand;

public sealed class SetTicketPriorityCommandValidator : AbstractValidator<SetTicketPriorityCommand>
{
    public SetTicketPriorityCommandValidator()
    {
        RuleFor(c => c.TicketId).NotEmpty();
        RuleFor(c => c.Priority)
            .Must(value => Enum.TryParse<TicketPriority>(value, true, out _))
            .WithMessage("Priority must be one of: Low, Normal, High, Urgent.");
    }
}

internal sealed class SetTicketPriorityCommandHandler(
    ITicketRepository tickets,
    ISlaPolicyRepository slaPolicies,
    ITicketsUnitOfWork unitOfWork,
    IDateTimeProvider clock) : ICommandHandler<SetTicketPriorityCommand>
{
    public async Task<Result> Handle(SetTicketPriorityCommand command, CancellationToken cancellationToken)
    {
        var ticket = await tickets.GetByIdAsync(command.TicketId, cancellationToken);
        if (ticket is null)
        {
            return TicketErrors.NotFound(command.TicketId);
        }

        var now = clock.UtcNow;
        var priority = Enum.Parse<TicketPriority>(command.Priority, true);

        ticket.SetPriority(priority, now);

        var policy = await slaPolicies.GetActiveByPriorityAsync(priority, cancellationToken);
        if (policy is not null)
        {
            ticket.ApplySla(policy, now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
