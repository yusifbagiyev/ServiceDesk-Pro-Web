using FluentValidation;
using ServiceDesk.Application.Abstractions.Messaging;
using ServiceDesk.Application.Abstractions.Security;
using ServiceDesk.Application.Abstractions.Time;
using ServiceDesk.Identity.Application.Abstractions;
using ServiceDesk.Identity.Domain.Entity;
using ServiceDesk.Identity.Domain.Enums;
using ServiceDesk.Identity.Domain.Exceptions;
using ServiceDesk.Kernel.Results;

namespace ServiceDesk.Identity.Application.Commands;

public sealed record CreateUserCommand(
    string Email,
    string FullName,
    string Password,
    string Role,
    string? PhoneNumber) : ICommand<Guid>;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(c => c.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(c => c.FullName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
        RuleFor(c => c.Role)
            .Must(role => Enum.TryParse<UserRole>(role, ignoreCase: true, out _))
            .WithMessage("Role must be one of: User, Admin.");
        RuleFor(c => c.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{6,14}$")
            .When(c => !string.IsNullOrWhiteSpace(c.PhoneNumber))
            .WithMessage("Phone number must be in E.164 format (e.g. +994501234567).");
    }
}

internal sealed class CreateUserCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IIdentityUnitOfWork unitOfWork,
    IDateTimeProvider clock) : ICommandHandler<CreateUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        if (await users.ExistsByEmailAsync(command.Email, cancellationToken))
        {
            return UserErrors.EmailTaken;
        }

        var role = Enum.Parse<UserRole>(command.Role, ignoreCase: true);
        var passwordHash = passwordHasher.Hash(command.Password);

        var user = User.Create(
            command.Email.Trim(),
            command.FullName.Trim(),
            passwordHash,
            role,
            clock.UtcNow,
            command.PhoneNumber?.Trim());

        users.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
