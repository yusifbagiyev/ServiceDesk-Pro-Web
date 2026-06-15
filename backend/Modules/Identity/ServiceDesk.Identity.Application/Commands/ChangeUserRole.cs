using FluentValidation;
using ServiceDesk.Application.Abstractions.Messaging;
using ServiceDesk.Application.Abstractions.Time;
using ServiceDesk.Identity.Application.Abstractions;
using ServiceDesk.Identity.Domain.Enums;
using ServiceDesk.Identity.Domain.Exceptions;
using ServiceDesk.Kernel.Results;

namespace ServiceDesk.Identity.Application.Commands;

public sealed record ChangeUserRoleCommand(Guid UserId, string Role) : ICommand;

public sealed class ChangeUserRoleCommandValidator : AbstractValidator<ChangeUserRoleCommand>
{
    public ChangeUserRoleCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.Role)
            .Must(role => Enum.TryParse<UserRole>(role, ignoreCase: true, out _))
            .WithMessage("Role must be one of: User, Admin.");
    }
}

internal sealed class ChangeUserRoleCommandHandler(
    IUserRepository users,
    IIdentityUnitOfWork unitOfWork,
    IDateTimeProvider clock) : ICommandHandler<ChangeUserRoleCommand>
{
    public async Task<Result> Handle(ChangeUserRoleCommand command, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            return UserErrors.NotFound(command.UserId);
        }

        var newRole = Enum.Parse<UserRole>(command.Role, ignoreCase: true);

        if (user.Role == UserRole.Admin && newRole == UserRole.User
            && await users.CountActiveAdminsAsync(cancellationToken) <= 1)
        {
            return UserErrors.CannotDemoteLastAdmin;
        }

        user.ChangeRole(newRole, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
