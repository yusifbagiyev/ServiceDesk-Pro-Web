namespace ServiceDesk.Kernel.Domain;

/// <summary>
/// Marker for a domain event. Kept free of any MediatR/infrastructure dependency so the
/// domain layer stays pure; the SaveChanges interceptor wraps these and publishes them
/// through MediatR after commit.
/// </summary>
public interface IDomainEvent
{
    /// <summary>When the event occurred (UTC). Set by the aggregate when it raises the event.</summary>
    DateTime OccurredAtUtc { get; }
}
