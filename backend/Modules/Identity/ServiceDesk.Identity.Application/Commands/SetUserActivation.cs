using FluentValidation;
using ServiceDesk.Application.Abstractions.Messaging;
using ServiceDesk.Application.Abstractions.Time;
using ServiceDesk.Identity.Application.Abstractions;
using ServiceDesk.Identity.Domain.Enums;
using ServiceDesk.Identity.Domain.Exceptions;
using ServiceDesk.Kernel.Results;

namespace ServiceDesk.Identity.Application.Commands;

/// <summary>Deactivate (disable-only lifecycle) or reactivate a user.</summary>
public sealed record SetUserActivationCommand(Guid UserId, bool IsActive) : ICommand;

public sealed class SetUserActivationCommandValidator : AbstractValidator<SetUserActivationCommand>
{
    public SetUserActivationCommandValidator() => RuleFor(c => c.UserId).NotEmpty();
}

internal sealed class SetUserActivationCommandHandler(
    IUserRepository users,
    IIdentityUnitOfWork unitOfWork,
    IDateTimeProvider clock) : ICommandHandler<SetUserActivationCommand>
{
    public async Task<Result> Handle(SetUserActivationCommand command, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            return UserErrors.NotFound(command.UserId);
        }

        if (!command.IsActive)
        {
            if (user.Role == UserRole.Admin && await users.CountActiveAdminsAsync(cancellationToken) <= 1)
            {
                return UserErrors.CannotDemoteLastAdmin;
            }

            user.Deactivate(clock.UtcNow);
        }
        else
        {
            user.Reactivate(clock.UtcNow);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
