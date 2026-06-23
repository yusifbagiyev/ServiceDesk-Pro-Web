using FluentValidation;
using ServiceDesk.Application.Abstractions.Messaging;
using ServiceDesk.Application.Abstractions.Time;
using ServiceDesk.Tickets.Application.Abstractions;
using ServiceDesk.Tickets.Domain.Exceptions;
using ServiceDesk.Kernel.Results;

namespace ServiceDesk.Tickets.Application.Commands;

public sealed record UpdateSlaPolicyCommand(Guid Id, string Name, int ResponseMinutes, int ResolutionMinutes) : ICommand;

public sealed class UpdateSlaPolicyCommandValidator : AbstractValidator<UpdateSlaPolicyCommand>
{
    public UpdateSlaPolicyCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
        RuleFor(c => c.ResponseMinutes).GreaterThan(0);
        RuleFor(c => c.ResolutionMinutes).GreaterThan(0);
    }
}

internal sealed class UpdateSlaPolicyCommandHandler(
    ISlaPolicyRepository slaPolicies,
    ITicketsUnitOfWork unitOfWork,
    IDateTimeProvider clock) : ICommandHandler<UpdateSlaPolicyCommand>
{
    public async Task<Result> Handle(UpdateSlaPolicyCommand command, CancellationToken cancellationToken)
    {
        var policy = await slaPolicies.GetByIdAsync(command.Id, cancellationToken);
        if (policy is null)
        {
            return TicketErrors.SlaPolicyNotFound(command.Id);
        }

        policy.Update(command.Name.Trim(), command.ResponseMinutes, command.ResolutionMinutes, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
