using ServiceDesk.Application.Abstractions.Messaging;
using ServiceDesk.Identity.Domain.Events;
using ServiceDesk.SharedInfrastructure.Authentication;

namespace ServiceDesk.Identity.Infrastructure;

/// <summary>
/// Revokes a user's BFF sessions when their role changes or they are deactivated, so the change
/// takes effect immediately (forces re-login on all devices).
/// </summary>
internal sealed class SessionRevocationHandler(ISessionStore sessionStore)
    : IDomainEventHandler<UserRoleChangedDomainEvent>,
        IDomainEventHandler<UserDeactivatedDomainEvent>
{
    public Task Handle(
        DomainEventNotification<UserRoleChangedDomainEvent> notification,
        CancellationToken cancellationToken) =>
        sessionStore.RemoveAllForUserAsync(notification.DomainEvent.UserId, cancellationToken);

    public Task Handle(
        DomainEventNotification<UserDeactivatedDomainEvent> notification,
        CancellationToken cancellationToken) =>
        sessionStore.RemoveAllForUserAsync(notification.DomainEvent.UserId, cancellationToken);
}
