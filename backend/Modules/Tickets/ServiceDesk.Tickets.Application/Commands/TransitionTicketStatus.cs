using FluentValidation;
using ServiceDesk.Application.Abstractions.Messaging;
using ServiceDesk.Application.Abstractions.Time;
using ServiceDesk.Tickets.Application.Abstractions;
using ServiceDesk.Tickets.Domain.Enums;
using ServiceDesk.Tickets.Domain.Exceptions;
using ServiceDesk.Kernel.Results;

namespace ServiceDesk.Tickets.Application.Commands;

public sealed record TransitionTicketStatusCommand(Guid TicketId, string Status, Guid? ByUserId, string? Note) : ICommand;

public sealed class TransitionTicketStatusCommandValidator : AbstractValidator<TransitionTicketStatusCommand>
{
    public TransitionTicketStatusCommandValidator()
    {
        RuleFor(c => c.TicketId).NotEmpty();
        RuleFor(c => c.Status)
            .Must(status => Enum.TryParse<TicketStatus>(status, true, out _))
            .WithMessage("Status must be a valid ticket status.");
    }
}

internal sealed class TransitionTicketStatusCommandHandler(
    ITicketRepository tickets,
    ITicketsUnitOfWork unitOfWork,
    IDateTimeProvider clock) : ICommandHandler<TransitionTicketStatusCommand>
{
    public async Task<Result> Handle(TransitionTicketStatusCommand command, CancellationToken cancellationToken)
    {
        var ticket = await tickets.GetByIdAsync(command.TicketId, cancellationToken);
        if (ticket is null)
        {
            return TicketErrors.NotFound(command.TicketId);
        }

        var to = Enum.Parse<TicketStatus>(command.Status, ignoreCase: true);
        var result = ticket.Transition(to, command.ByUserId, clock.UtcNow, command.Note?.Trim());
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
