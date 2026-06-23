using ServiceDesk.Application.Abstractions.Messaging;
using ServiceDesk.Kernel.Results;
using ServiceDesk.Tickets.Application.Abstractions;

namespace ServiceDesk.Tickets.Application.Queries;

public sealed record ListClosedTicketsQuery(
    Guid? AssigneeUserId,
    Guid? ReporterUserId,
    string? Priority,
    string? Search,
    DateTime? CreatedBeforeUtc,
    int Take = 50) : IQuery<IReadOnlyList<TicketListItem>>;

internal sealed class ListClosedTicketsQueryHandler(
    ITicketReadRepository read) : IQueryHandler<ListClosedTicketsQuery, IReadOnlyList<TicketListItem>>
{
    public async Task<Result<IReadOnlyList<TicketListItem>>> Handle(ListClosedTicketsQuery query, CancellationToken cancellationToken)
    {
        var take = Math.Clamp(query.Take, 1, 200);
        var filter = new TicketListFilter(
            ClosedOnly: true,
            AssigneeUserId: query.AssigneeUserId,
            ReporterUserId: query.ReporterUserId,
            Status: null,
            Priority: query.Priority,
            Search: query.Search,
            CreatedBeforeUtc: query.CreatedBeforeUtc,
            Take: take);

        var result = await read.ListAsync(filter, cancellationToken);
        return Result.Success(result);
    }
}
