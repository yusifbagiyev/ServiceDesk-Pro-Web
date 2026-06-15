using FluentValidation;
using ServiceDesk.Application.Abstractions.Messaging;
using ServiceDesk.Application.Abstractions.Security;
using ServiceDesk.Identity.Application.Abstractions;
using ServiceDesk.Identity.Domain.Exceptions;
using ServiceDesk.Kernel.Results;

namespace ServiceDesk.Identity.Application.Commands;

public sealed record LoginCommand(string Email, string Password) : ICommand<AuthenticatedUser>;

/// <summary>The authenticated identity returned on a successful login (the API builds the session from it).</summary>
public sealed record AuthenticatedUser(Guid UserId, string Email, string FullName, string Role);

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(c => c.Email).NotEmpty();
        RuleFor(c => c.Password).NotEmpty();
    }
}

internal sealed class LoginCommandHandler(IUserRepository users, IPasswordHasher passwordHasher)
    : ICommandHandler<LoginCommand, AuthenticatedUser>
{
    public async Task<Result<AuthenticatedUser>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await users.GetByEmailAsync(command.Email, cancellationToken);

        if (user is null)
        {
            // Run the hash anyway so an unknown email costs the same time as a known one (no enumeration).
            _ = passwordHasher.Hash(command.Password);
            return UserErrors.InvalidCredentials;
        }

        if (!passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            return UserErrors.InvalidCredentials;
        }

        // Activation is checked AFTER the verify so there is no timing oracle.
        if (!user.IsActive)
        {
            return UserErrors.Inactive;
        }

        return new AuthenticatedUser(user.Id, user.Email, user.FullName, user.Role.ToString());
    }
}
