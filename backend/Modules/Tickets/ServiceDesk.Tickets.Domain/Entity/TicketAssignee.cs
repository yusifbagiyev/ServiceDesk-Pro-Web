using ServiceDesk.Kernel.Domain;

namespace ServiceDesk.Tickets.Domain;

/// <summary>An assigned user (cross-module id + a name snapshot for display). Owned by <see cref="Ticket"/>.</summary>
public sealed class TicketAssignee : Entity
{
    private TicketAssignee()
    {
    }

    internal TicketAssignee(Guid ticketId, Guid assigneeUserId, string fullNameSnapshot, DateTime nowUtc)
        : base(NewId())
    {
        TicketId = ticketId;
        AssigneeUserId = assigneeUserId;
        FullNameSnapshot = fullNameSnapshot;
        AssignedAtUtc = nowUtc;
        CreatedAtUtc = nowUtc;
    }

    public Guid TicketId { get; private set; }

    public Guid AssigneeUserId { get; private set; }

    public string FullNameSnapshot { get; private set; } = null!;

    public DateTime AssignedAtUtc { get; private set; }
}
