using FluentValidation;
using ServiceDesk.Application.Abstractions.Messaging;
using ServiceDesk.Application.Abstractions.Time;
using ServiceDesk.Identity.Application.Abstractions;
using ServiceDesk.Identity.Domain.Entity;
using ServiceDesk.Identity.Domain.Exceptions;
using ServiceDesk.Kernel.Results;

namespace ServiceDesk.Identity.Application.Commands;

/// <summary>Admin update of a user's identity and contact details (email, display name, phone, WhatsApp opt-in).</summary>
public sealed record UpdateUserCommand(
    Guid UserId,
    string Email,
    string FullName,
    string? PhoneNumber,
    bool WhatsAppOptIn) : ICommand;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(c => c.FullName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{6,14}$")
            .When(c => !string.IsNullOrWhiteSpace(c.PhoneNumber))
            .WithMessage("Phone number must be in E.164 format (e.g. +994501234567).");
    }
}

internal sealed class UpdateUserCommandHandler(
    IUserRepository users,
    IIdentityUnitOfWork unitOfWork,
    IDateTimeProvider clock) : ICommandHandler<UpdateUserCommand>
{
    public async Task<Result> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            return UserErrors.NotFound(command.UserId);
        }

        // Only reject the new email if it belongs to a *different* user.
        var newEmail = User.NormalizeEmail(command.Email);
        if (newEmail != user.Email && await users.ExistsByEmailAsync(newEmail, cancellationToken))
        {
            return UserErrors.EmailTaken;
        }

        var now = clock.UtcNow;
        user.ChangeEmail(command.Email.Trim(), now);
        user.Rename(command.FullName.Trim(), now);
        user.UpdateContact(command.PhoneNumber?.Trim(), command.WhatsAppOptIn, now);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
