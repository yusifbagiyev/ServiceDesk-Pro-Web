using ServiceDesk.Kernel.Results;
using ServiceDesk.Tickets.Domain.Enums;

namespace ServiceDesk.Tickets.Domain.Exceptions;

public static class TicketErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Tickets.NotFound", $"Ticket '{id}' was not found.");

    public static Error InvalidTransition(TicketStatus from, TicketStatus to) =>
        Error.Conflict("Tickets.InvalidTransition", $"Cannot move a ticket from {from} to {to}.");

    public static readonly Error NoAssignees =
        Error.Validation("Tickets.NoAssignees", "A ticket must have at least one assignee.");

    public static readonly Error NoCategories =
        Error.Validation("Tickets.NoCategories", "A ticket must have at least one category.");

    public static readonly Error SolutionRequired =
        Error.Validation("Tickets.SolutionRequired", "A solution is required to resolve or close a ticket.");

    public static readonly Error AlreadyDeleted =
        Error.Conflict("Tickets.AlreadyDeleted", "The ticket has been deleted.");
}
