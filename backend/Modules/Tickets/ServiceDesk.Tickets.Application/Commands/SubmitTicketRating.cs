using FluentValidation;
using ServiceDesk.Application.Abstractions.Messaging;
using ServiceDesk.Application.Abstractions.Time;
using ServiceDesk.Tickets.Application.Abstractions;
using ServiceDesk.Tickets.Domain.Exceptions;
using ServiceDesk.Kernel.Results;

namespace ServiceDesk.Tickets.Application.Commands;

public sealed record SubmitTicketRatingCommand(Guid TicketId, int Score, string? Message, Guid? RaterUserId) : ICommand;

public sealed class SubmitTicketRatingCommandValidator : AbstractValidator<SubmitTicketRatingCommand>
{
    public SubmitTicketRatingCommandValidator()
    {
        RuleFor(c => c.TicketId).NotEmpty();
        RuleFor(c => c.Score).InclusiveBetween(0, 5);
        RuleFor(c => c.Message).MaximumLength(2000).When(c => c.Message is not null);
    }
}

internal sealed class SubmitTicketRatingCommandHandler(
    ITicketRepository tickets,
    ITicketsUnitOfWork unitOfWork,
    IDateTimeProvider clock) : ICommandHandler<SubmitTicketRatingCommand>
{
    public async Task<Result> Handle(SubmitTicketRatingCommand command, CancellationToken cancellationToken)
    {
        var ticket = await tickets.GetByIdAsync(command.TicketId, cancellationToken);
        if (ticket is null)
        {
            return TicketErrors.NotFound(command.TicketId);
        }

        ticket.SubmitRating(command.Score, command.Message?.Trim(), command.RaterUserId, clock.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
