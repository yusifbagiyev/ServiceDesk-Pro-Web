using ServiceDesk.Kernel.Domain;
using ServiceDesk.Tickets.Domain.Enums;

namespace ServiceDesk.Tickets.Domain;

/// <summary>A threaded comment on a ticket. Owned by <see cref="Ticket"/>.</summary>
public sealed class TicketComment : Entity
{
    private TicketComment()
    {
    }

    internal TicketComment(
        Guid ticketId,
        Guid? parentCommentId,
        Guid authorUserId,
        string authorFullName,
        string body,
        CommentVisibility visibility,
        DateTime nowUtc)
        : base(NewId())
    {
        TicketId = ticketId;
        ParentCommentId = parentCommentId;
        AuthorUserId = authorUserId;
        AuthorFullName = authorFullName;
        Body = body;
        Visibility = visibility;
        CreatedAtUtc = nowUtc;
    }

    public Guid TicketId { get; private set; }

    public Guid? ParentCommentId { get; private set; }

    public Guid AuthorUserId { get; private set; }

    public string AuthorFullName { get; private set; } = null!;

    public string Body { get; private set; } = null!;

    public CommentVisibility Visibility { get; private set; }

    public bool IsEdited { get; private set; }
}
