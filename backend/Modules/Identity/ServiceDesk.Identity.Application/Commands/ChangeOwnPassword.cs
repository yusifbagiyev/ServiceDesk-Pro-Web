using FluentValidation;
using ServiceDesk.Application.Abstractions.Messaging;
using ServiceDesk.Application.Abstractions.Security;
using ServiceDesk.Application.Abstractions.Time;
using ServiceDesk.Identity.Application.Abstractions;
using ServiceDesk.Identity.Domain.Exceptions;
using ServiceDesk.Kernel.Results;

namespace ServiceDesk.Identity.Application.Commands;

public sealed record ChangeOwnPasswordCommand(string CurrentPassword, string NewPassword) : ICommand;

public sealed class ChangeOwnPasswordCommandValidator : AbstractValidator<ChangeOwnPasswordCommand>
{
    public ChangeOwnPasswordCommandValidator()
    {
        RuleFor(c => c.CurrentPassword).NotEmpty();
        RuleFor(c => c.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(128);
    }
}

internal sealed class ChangeOwnPasswordCommandHandler(
    IUserRepository users,
    ICurrentUser currentUser,
    IPasswordHasher passwordHasher,
    IIdentityUnitOfWork unitOfWork,
    IDateTimeProvider clock) : ICommandHandler<ChangeOwnPasswordCommand>
{
    public async Task<Result> Handle(ChangeOwnPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(currentUser.UserId, cancellationToken);
        if (user is null)
        {
            return UserErrors.NotFound(currentUser.UserId);
        }

        if (!passwordHasher.Verify(command.CurrentPassword, user.PasswordHash))
        {
            return UserErrors.InvalidCredentials;
        }

        user.ChangePassword(passwordHasher.Hash(command.NewPassword), clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
