using ServiceDesk.Identity.Domain.Enums;
using ServiceDesk.Kernel.Domain;

namespace ServiceDesk.Identity.Domain.Events;

public sealed record UserCreatedDomainEvent(Guid UserId, string FullName, UserRole Role, DateTime OccurredAtUtc)
    : IDomainEvent;

public sealed record UserRoleChangedDomainEvent(
    Guid UserId,
    string FullName,
    UserRole OldRole,
    UserRole NewRole,
    DateTime OccurredAtUtc) : IDomainEvent;

public sealed record UserDeactivatedDomainEvent(Guid UserId, string FullName, DateTime OccurredAtUtc) : IDomainEvent;

public sealed record UserReactivatedDomainEvent(Guid UserId, string FullName, DateTime OccurredAtUtc) : IDomainEvent;

public sealed record UserPasswordChangedDomainEvent(Guid UserId, string FullName, DateTime OccurredAtUtc) : IDomainEvent;
