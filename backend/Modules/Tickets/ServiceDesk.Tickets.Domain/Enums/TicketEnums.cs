namespace ServiceDesk.Tickets.Domain.Enums;

public enum TicketStatus
{
    New = 0,
    Pending = 1,
    Resolving = 2,
    Resolved = 3,
    Closed = 4,
    Cancelled = 5,
}

public enum TicketPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Urgent = 3,
}

public enum CommentVisibility
{
    Public = 0,
    Internal = 1,
}
