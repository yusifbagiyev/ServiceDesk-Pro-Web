using FluentValidation;
using ServiceDesk.Application.Abstractions.Messaging;
using ServiceDesk.Application.Abstractions.Time;
using ServiceDesk.Tickets.Application.Abstractions;
using ServiceDesk.Tickets.Domain.Exceptions;
using ServiceDesk.Kernel.Results;

namespace ServiceDesk.Tickets.Application.Commands;

public sealed record SetSlaPolicyActivationCommand(Guid Id, bool IsActive) : ICommand;

public sealed class SetSlaPolicyActivationCommandValidator : AbstractValidator<SetSlaPolicyActivationCommand>
{
    public SetSlaPolicyActivationCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}

internal sealed class SetSlaPolicyActivationCommandHandler(
    ISlaPolicyRepository slaPolicies,
    ITicketsUnitOfWork unitOfWork,
    IDateTimeProvider clock) : ICommandHandler<SetSlaPolicyActivationCommand>
{
    public async Task<Result> Handle(SetSlaPolicyActivationCommand command, CancellationToken cancellationToken)
    {
        var policy = await slaPolicies.GetByIdAsync(command.Id, cancellationToken);
        if (policy is null)
        {
            return TicketErrors.SlaPolicyNotFound(command.Id);
        }

        policy.SetActive(command.IsActive, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
