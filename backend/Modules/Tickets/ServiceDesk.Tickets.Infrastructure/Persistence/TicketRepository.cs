using Microsoft.EntityFrameworkCore;
using ServiceDesk.Tickets.Application.Abstractions;
using ServiceDesk.Tickets.Domain;

namespace ServiceDesk.Tickets.Infrastructure.Persistence;

internal sealed class TicketRepository(TicketsDbContext dbContext) : ITicketRepository
{
    public Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Tickets
            .Include(t => t.Rating)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public void Add(Ticket ticket) => dbContext.Tickets.Add(ticket);
}
