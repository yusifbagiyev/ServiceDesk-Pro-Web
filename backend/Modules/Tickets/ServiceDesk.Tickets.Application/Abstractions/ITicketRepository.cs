using ServiceDesk.Tickets.Domain;

namespace ServiceDesk.Tickets.Application.Abstractions;

/// <summary>Write-side gateway for the <see cref="Ticket"/> aggregate (loads the aggregate with its children).</summary>
public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    void Add(Ticket ticket);
}
