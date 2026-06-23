namespace ServiceDesk.Tickets.Application.DTOs;

// Request bodies for the Tickets API. The ticket id (route), and the caller-derived ids
// (created-by, author, rater) are set server-side in the controller, never from the body.

public sealed record CreateTicketRequest(
    Guid? ReporterUserId,
    string Priority,
    string? Title,
    string? InventoryCode,
    IReadOnlyList<Guid> CategoryIds);

public sealed record UpdateTicketDetailsRequest(
    string? Title,
    string? Solution,
    string? Worker,
    string? DeviceName,
    string? DepartmentName);

public sealed record TransitionTicketRequest(string Status, string? Note);

public sealed record AssignUsersRequest(IReadOnlyList<Guid> UserIds);

public sealed record SetTicketCategoriesRequest(IReadOnlyList<Guid> CategoryIds);

public sealed record SetTicketPriorityRequest(string Priority);

public sealed record AddCommentRequest(string Body, string Visibility, Guid? ParentCommentId);

public sealed record SubmitRatingRequest(int Score, string? Message);

public sealed record CreateSlaPolicyRequest(string Name, string Priority, int ResponseMinutes, int ResolutionMinutes);

public sealed record UpdateSlaPolicyRequest(string Name, int ResponseMinutes, int ResolutionMinutes);

public sealed record SetSlaPolicyActivationRequest(bool IsActive);
