using FluentValidation;
using ServiceDesk.Application.Abstractions.Messaging;
using ServiceDesk.Application.Abstractions.Time;
using ServiceDesk.Kernel.Results;
using ServiceDesk.Tickets.Application.Abstractions;
using ServiceDesk.Tickets.Domain;
using ServiceDesk.Tickets.Domain.Enums;

namespace ServiceDesk.Tickets.Application.Commands;

public sealed record CreateSlaPolicyCommand(
    string Name,
    string Priority,
    int ResponseMinutes,
    int ResolutionMinutes) : ICommand<Guid>;

public sealed class CreateSlaPolicyCommandValidator : AbstractValidator<CreateSlaPolicyCommand>
{
    public CreateSlaPolicyCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Priority)
            .Must(value => Enum.TryParse<TicketPriority>(value, true, out _))
            .WithMessage("Priority must be a valid ticket priority.");
        RuleFor(c => c.ResponseMinutes).GreaterThan(0);
        RuleFor(c => c.ResolutionMinutes).GreaterThan(0);
    }
}

internal sealed class CreateSlaPolicyCommandHandler(
    ISlaPolicyRepository slaPolicies,
    ITicketsUnitOfWork unitOfWork,
    IDateTimeProvider clock) : ICommandHandler<CreateSlaPolicyCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateSlaPolicyCommand command, CancellationToken cancellationToken)
    {
        var priority = Enum.Parse<TicketPriority>(command.Priority, ignoreCase: true);

        var policy = SlaPolicy.Create(
            command.Name.Trim(),
            priority,
            command.ResponseMinutes,
            command.ResolutionMinutes,
            clock.UtcNow);

        slaPolicies.Add(policy);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return policy.Id;
    }
}
