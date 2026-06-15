using ServiceDesk.Kernel.Domain;
using ServiceDesk.Tickets.Domain.Enums;

namespace ServiceDesk.Tickets.Domain.Events;

public sealed record TicketCreatedDomainEvent(Guid TicketId, Guid? CreatedByUserId, DateTime OccurredAtUtc)
    : IDomainEvent;

public sealed record TicketStatusChangedDomainEvent(
    Guid TicketId,
    TicketStatus? FromStatus,
    TicketStatus ToStatus,
    Guid? ChangedByUserId,
    DateTime OccurredAtUtc) : IDomainEvent;

public sealed record TicketAssignedDomainEvent(Guid TicketId, Guid AssigneeUserId, DateTime OccurredAtUtc)
    : IDomainEvent;

public sealed record TicketCommentAddedDomainEvent(
    Guid TicketId,
    Guid CommentId,
    Guid AuthorUserId,
    DateTime OccurredAtUtc) : IDomainEvent;

public sealed record TicketRatingSubmittedDomainEvent(
    Guid TicketId,
    int Score,
    IReadOnlyList<Guid> AssigneeUserIds,
    DateTime OccurredAtUtc) : IDomainEvent;
