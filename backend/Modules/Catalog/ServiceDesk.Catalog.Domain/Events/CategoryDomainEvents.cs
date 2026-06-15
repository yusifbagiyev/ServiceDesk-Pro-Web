using ServiceDesk.Kernel.Domain;

namespace ServiceDesk.Catalog.Domain.Events;

public sealed record CategoryCreatedDomainEvent(Guid CategoryId, string Name, DateTime OccurredAtUtc) : IDomainEvent;

public sealed record CategoryRenamedDomainEvent(Guid CategoryId, string Name, DateTime OccurredAtUtc) : IDomainEvent;

public sealed record CategoryActivationChangedDomainEvent(Guid CategoryId, bool IsActive, DateTime OccurredAtUtc)
    : IDomainEvent;
