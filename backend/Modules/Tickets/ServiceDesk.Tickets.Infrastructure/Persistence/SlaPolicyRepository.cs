using Microsoft.EntityFrameworkCore;
using ServiceDesk.Tickets.Application.Abstractions;
using ServiceDesk.Tickets.Domain;
using ServiceDesk.Tickets.Domain.Enums;

namespace ServiceDesk.Tickets.Infrastructure.Persistence;

internal sealed class SlaPolicyRepository(TicketsDbContext dbContext) : ISlaPolicyRepository
{
    public Task<SlaPolicy?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => dbContext.SlaPolicies.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<SlaPolicy?> GetActiveByPriorityAsync(TicketPriority priority, CancellationToken ct = default)
        => dbContext.SlaPolicies.FirstOrDefaultAsync(p => p.IsActive && p.Priority == priority, ct);

    public async Task<IReadOnlyList<SlaPolicy>> ListAsync(CancellationToken ct = default)
        => await dbContext.SlaPolicies.AsNoTracking().OrderBy(p => p.Priority).ToListAsync(ct);

    public void Add(SlaPolicy policy) => dbContext.SlaPolicies.Add(policy);
}
