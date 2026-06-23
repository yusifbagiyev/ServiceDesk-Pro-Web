using FluentValidation;
using ServiceDesk.Application.Abstractions.Messaging;
using ServiceDesk.Application.Abstractions.Time;
using ServiceDesk.Tickets.Application.Abstractions;
using ServiceDesk.Tickets.Domain.Exceptions;
using ServiceDesk.Kernel.Results;

namespace ServiceDesk.Tickets.Application.Commands;

public sealed record UpdateTicketDetailsCommand(
    Guid TicketId,
    string? Title,
    string? Solution,
    string? Worker,
    string? DeviceName,
    string? DepartmentName) : ICommand;

public sealed class UpdateTicketDetailsCommandValidator : AbstractValidator<UpdateTicketDetailsCommand>
{
    public UpdateTicketDetailsCommandValidator()
    {
        RuleFor(c => c.TicketId).NotEmpty();
        RuleFor(c => c.Title).MaximumLength(200).When(c => c.Title is not null);
    }
}

internal sealed class UpdateTicketDetailsCommandHandler(
    ITicketRepository tickets,
    ITicketsUnitOfWork unitOfWork,
    IDateTimeProvider clock) : ICommandHandler<UpdateTicketDetailsCommand>
{
    public async Task<Result> Handle(UpdateTicketDetailsCommand command, CancellationToken cancellationToken)
    {
        var ticket = await tickets.GetByIdAsync(command.TicketId, cancellationToken);
        if (ticket is null)
        {
            return TicketErrors.NotFound(command.TicketId);
        }

        ticket.UpdateDetails(
            command.Title?.Trim(),
            command.Solution?.Trim(),
            command.Worker?.Trim(),
            command.DeviceName?.Trim(),
            command.DepartmentName?.Trim(),
            clock.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
