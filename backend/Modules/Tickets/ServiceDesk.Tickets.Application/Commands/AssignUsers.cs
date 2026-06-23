using FluentValidation;
using ServiceDesk.Application.Abstractions.Messaging;
using ServiceDesk.Application.Abstractions.Time;
using ServiceDesk.Tickets.Application.Abstractions;
using ServiceDesk.Tickets.Domain.Exceptions;
using ServiceDesk.Kernel.Results;

namespace ServiceDesk.Tickets.Application.Commands;

public sealed record AssignUsersCommand(Guid TicketId, IReadOnlyList<Guid> UserIds) : ICommand;

public sealed class AssignUsersCommandValidator : AbstractValidator<AssignUsersCommand>
{
    public AssignUsersCommandValidator()
    {
        RuleFor(c => c.TicketId).NotEmpty();
        RuleFor(c => c.UserIds).NotEmpty();
    }
}

internal sealed class AssignUsersCommandHandler(
    ITicketRepository tickets,
    ITicketDirectoryReader directory,
    ITicketsUnitOfWork unitOfWork,
    IDateTimeProvider clock) : ICommandHandler<AssignUsersCommand>
{
    public async Task<Result> Handle(AssignUsersCommand command, CancellationToken cancellationToken)
    {
        var ticket = await tickets.GetByIdAsync(command.TicketId, cancellationToken);
        if (ticket is null)
        {
            return TicketErrors.NotFound(command.TicketId);
        }

        var now = clock.UtcNow;

        foreach (var userId in command.UserIds)
        {
            var user = await directory.FindUserAsync(userId, cancellationToken);
            if (user is null || !user.IsActive)
            {
                return TicketErrors.UserNotFound(userId);
            }

            ticket.AssignUser(userId, user.FullName, now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
