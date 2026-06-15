using FluentValidation;
using ServiceDesk.Application.Abstractions.Messaging;
using ServiceDesk.Application.Abstractions.Time;
using ServiceDesk.Identity.Application.Abstractions;
using ServiceDesk.Identity.Domain.Exceptions;
using ServiceDesk.Kernel.Results;

namespace ServiceDesk.Identity.Application.Commands;

/// <summary>Update a user's contact details (phone / WhatsApp opt-in). Used by self-service and admin.</summary>
public sealed record UpdateUserContactCommand(
    Guid UserId,
    string? PhoneNumber,
    bool WhatsAppOptIn) : ICommand;

public sealed class UpdateUserContactCommandValidator : AbstractValidator<UpdateUserContactCommand>
{
    public UpdateUserContactCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{6,14}$")
            .When(c => !string.IsNullOrWhiteSpace(c.PhoneNumber))
            .WithMessage("Phone number must be in E.164 format (e.g. +994501234567).");
    }
}

internal sealed class UpdateUserContactCommandHandler(
    IUserRepository users,
    IIdentityUnitOfWork unitOfWork,
    IDateTimeProvider clock) : ICommandHandler<UpdateUserContactCommand>
{
    public async Task<Result> Handle(UpdateUserContactCommand command, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            return UserErrors.NotFound(command.UserId);
        }

        user.UpdateContact(command.PhoneNumber?.Trim(), command.WhatsAppOptIn, clock.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
