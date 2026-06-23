using FluentValidation;
using ServiceDesk.Application.Abstractions.Messaging;
using ServiceDesk.Application.Abstractions.Time;
using ServiceDesk.Tickets.Application.Abstractions;
using ServiceDesk.Tickets.Domain.Exceptions;
using ServiceDesk.Kernel.Results;

namespace ServiceDesk.Tickets.Application.Commands;

public sealed record AddTicketAttachmentCommand(
    Guid TicketId,
    Guid? CommentId,
    string FileName,
    string ContentType,
    long SizeBytes,
    string StorageKey,
    Guid UploadedByUserId) : ICommand<Guid>;

public sealed class AddTicketAttachmentCommandValidator : AbstractValidator<AddTicketAttachmentCommand>
{
    public AddTicketAttachmentCommandValidator()
    {
        RuleFor(c => c.TicketId).NotEmpty();
        RuleFor(c => c.FileName).NotEmpty().MaximumLength(260);
        RuleFor(c => c.ContentType).NotEmpty();
        RuleFor(c => c.StorageKey).NotEmpty();
        RuleFor(c => c.UploadedByUserId).NotEmpty();
        RuleFor(c => c.SizeBytes).GreaterThan(0);
    }
}

internal sealed class AddTicketAttachmentCommandHandler(
    ITicketRepository tickets,
    ITicketsUnitOfWork unitOfWork,
    IDateTimeProvider clock) : ICommandHandler<AddTicketAttachmentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddTicketAttachmentCommand command, CancellationToken cancellationToken)
    {
        var ticket = await tickets.GetByIdAsync(command.TicketId, cancellationToken);
        if (ticket is null)
        {
            return TicketErrors.NotFound(command.TicketId);
        }

        var now = clock.UtcNow;
        var id = ticket.AddAttachment(
            command.CommentId,
            command.FileName.Trim(),
            command.ContentType.Trim(),
            command.SizeBytes,
            command.StorageKey.Trim(),
            command.UploadedByUserId,
            now);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return id;
    }
}
