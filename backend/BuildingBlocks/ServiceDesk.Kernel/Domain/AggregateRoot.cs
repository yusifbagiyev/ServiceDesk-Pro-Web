namespace ServiceDesk.Kernel.Domain;

/// <summary>
/// Base for aggregate roots. Collects domain events raised during a unit of work;
/// the SaveChanges interceptor drains and dispatches them after commit, then clears.
/// </summary>
public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot(Guid id)
        : base(id)
    {
    }

    protected AggregateRoot()
    {
    }

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <summary>Stamp <see cref="Entity.UpdatedAtUtc"/>; call from mutating aggregate methods.</summary>
    protected void Touch(DateTime nowUtc) => UpdatedAtUtc = nowUtc;
}
