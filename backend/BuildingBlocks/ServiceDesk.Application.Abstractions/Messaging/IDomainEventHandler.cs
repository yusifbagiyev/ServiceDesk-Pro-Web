using MediatR;
using ServiceDesk.Kernel.Domain;

namespace ServiceDesk.Application.Abstractions.Messaging;

/// <summary>
/// Handles a domain event. Implemented in module Application layers; invoked by the
/// SaveChanges interceptor (after commit) via the <see cref="DomainEventNotification{TDomainEvent}"/> wrapper.
/// </summary>
public interface IDomainEventHandler<TDomainEvent> : INotificationHandler<DomainEventNotification<TDomainEvent>>
    where TDomainEvent : IDomainEvent;

/// <summary>
/// MediatR envelope around a domain event. Keeps the domain layer free of any MediatR dependency:
/// the domain raises a bare <see cref="IDomainEvent"/>, the dispatcher wraps it here.
/// </summary>
public sealed record DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent) : INotification
    where TDomainEvent : IDomainEvent;
