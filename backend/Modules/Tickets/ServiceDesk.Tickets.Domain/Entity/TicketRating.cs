using ServiceDesk.Kernel.Domain;

namespace ServiceDesk.Tickets.Domain;

/// <summary>The single satisfaction rating (0-5) on a ticket. Owned by <see cref="Ticket"/>.</summary>
public sealed class TicketRating : Entity
{
    private TicketRating()
    {
    }

    internal TicketRating(Guid ticketId, int value, string? message, Guid? raterUserId, DateTime nowUtc)
        : base(NewId())
    {
        TicketId = ticketId;
        Value = value;
        Message = message;
        RaterUserId = raterUserId;
        RatedAtUtc = nowUtc;
        CreatedAtUtc = nowUtc;
    }

    public Guid TicketId { get; private set; }

    public int Value { get; private set; }

    public string? Message { get; private set; }

    public Guid? RaterUserId { get; private set; }

    public DateTime RatedAtUtc { get; private set; }

    internal void Update(int value, string? message, DateTime nowUtc)
    {
        Value = value;
        Message = message;
        RatedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }
}
