using ServiceDesk.Kernel.Domain;

namespace ServiceDesk.Tickets.Domain;

/// <summary>A category link (cross-module Catalog id + name snapshot). Owned by <see cref="Ticket"/>.</summary>
public sealed class TicketCategory : Entity
{
    private TicketCategory()
    {
    }

    internal TicketCategory(Guid ticketId, Guid categoryId, string nameSnapshot, DateTime nowUtc)
        : base(NewId())
    {
        TicketId = ticketId;
        CategoryId = categoryId;
        NameSnapshot = nameSnapshot;
        CreatedAtUtc = nowUtc;
    }

    public Guid TicketId { get; private set; }

    public Guid CategoryId { get; private set; }

    public string NameSnapshot { get; private set; } = null!;
}
