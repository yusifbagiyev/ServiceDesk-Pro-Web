using ServiceDesk.Tickets.Domain;
using ServiceDesk.Tickets.Domain.Enums;

namespace ServiceDesk.Tickets.Application.Abstractions;

/// <summary>Persistence gateway for the <see cref="SlaPolicy"/> aggregate (SLA configuration).</summary>
public interface ISlaPolicyRepository
{
    Task<SlaPolicy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>The active policy for a priority, used to stamp a ticket's SLA due dates; null if none.</summary>
    Task<SlaPolicy?> GetActiveByPriorityAsync(TicketPriority priority, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SlaPolicy>> ListAsync(CancellationToken cancellationToken = default);

    void Add(SlaPolicy policy);
}
