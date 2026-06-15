using ServiceDesk.Kernel.Domain;
using ServiceDesk.Tickets.Domain.Enums;

namespace ServiceDesk.Tickets.Domain;

/// <summary>One status-change event in a ticket's timeline. Owned by <see cref="Ticket"/>.</summary>
public sealed class TicketStatusHistory : Entity
{
    private TicketStatusHistory()
    {
    }

    internal TicketStatusHistory(
        Guid ticketId,
        TicketStatus? fromStatus,
        TicketStatus toStatus,
        Guid? changedByUserId,
        DateTime changedAtUtc,
        string? note)
        : base(NewId())
    {
        TicketId = ticketId;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        ChangedByUserId = changedByUserId;
        ChangedAtUtc = changedAtUtc;
        Note = note;
        CreatedAtUtc = changedAtUtc;
    }

    public Guid TicketId { get; private set; }

    public TicketStatus? FromStatus { get; private set; }

    public TicketStatus ToStatus { get; private set; }

    public Guid? ChangedByUserId { get; private set; }

    public DateTime ChangedAtUtc { get; private set; }

    public string? Note { get; private set; }
}
