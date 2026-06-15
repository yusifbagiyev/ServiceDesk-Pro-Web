namespace ServiceDesk.Tickets.Domain.Enums;

/// <summary>The allowed ticket status transitions (the state machine).</summary>
public static class TicketStatusTransitions
{
    private static readonly Dictionary<TicketStatus, TicketStatus[]> Allowed = new()
    {
        [TicketStatus.New] = [TicketStatus.Pending, TicketStatus.Cancelled],
        [TicketStatus.Pending] = [TicketStatus.Resolving, TicketStatus.Resolved, TicketStatus.Closed, TicketStatus.Cancelled],
        [TicketStatus.Resolving] = [TicketStatus.Resolved, TicketStatus.Pending, TicketStatus.Cancelled],
        [TicketStatus.Resolved] = [TicketStatus.Closed, TicketStatus.Resolving, TicketStatus.Cancelled],
        [TicketStatus.Closed] = [TicketStatus.Resolving],
        [TicketStatus.Cancelled] = [],
    };

    public static bool CanTransition(TicketStatus from, TicketStatus to) =>
        from != to && Allowed.TryGetValue(from, out var targets) && Array.IndexOf(targets, to) >= 0;
}
