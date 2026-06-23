using FluentValidation;
using ServiceDesk.Application.Abstractions.Messaging;
using ServiceDesk.Application.Abstractions.Time;
using ServiceDesk.Tickets.Application.Abstractions;
using ServiceDesk.Tickets.Domain.Enums;
using ServiceDesk.Tickets.Domain.Exceptions;
using ServiceDesk.Kernel.Results;

namespace ServiceDesk.Tickets.Application.Commands;

public sealed record AddTicketCommentCommand(
    Guid TicketId,
    Guid AuthorUserId,
    string Body,
    string Visibility,
    Guid? ParentCommentId) : ICommand<Guid>;

public sealed class AddTicketCommentCommandValidator : AbstractValidator<AddTicketCommentCommand>
{
    public AddTicketCommentCommandValidator()
    {
        RuleFor(c => c.TicketId).NotEmpty();
        RuleFor(c => c.AuthorUserId).NotEmpty();
        RuleFor(c => c.Body).NotEmpty().MaximumLength(4000);
        RuleFor(c => c.Visibility)
            .Must(v => Enum.TryParse<CommentVisibility>(v, true, out _))
            .WithMessage("Visibility must be Public or Internal.");
    }
}

internal sealed class AddTicketCommentCommandHandler(
    ITicketRepository tickets,
    ITicketDirectoryReader directory,
    ITicketsUnitOfWork unitOfWork,
    IDateTimeProvider clock) : ICommandHandler<AddTicketCommentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddTicketCommentCommand command, CancellationToken cancellationToken)
    {
        var ticket = await tickets.GetByIdAsync(command.TicketId, cancellationToken);
        if (ticket is null)
        {
            return TicketErrors.NotFound(command.TicketId);
        }

        var author = await directory.FindUserAsync(command.AuthorUserId, cancellationToken);
        if (author is null || !author.IsActive)
        {
            return TicketErrors.UserNotFound(command.AuthorUserId);
        }

        var visibility = Enum.Parse<CommentVisibility>(command.Visibility, true);
        var commentId = ticket.AddComment(
            command.AuthorUserId,
            author.FullName,
            command.Body.Trim(),
            visibility,
            command.ParentCommentId,
            clock.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return commentId;
    }
}
