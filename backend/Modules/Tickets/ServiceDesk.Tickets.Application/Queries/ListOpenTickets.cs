using ServiceDesk.Application.Abstractions.Messaging;
using ServiceDesk.Kernel.Results;
using ServiceDesk.Tickets.Application.Abstractions;

namespace ServiceDesk.Tickets.Application.Queries;

public sealed record ListOpenTicketsQuery(
    Guid? AssigneeUserId,
    Guid? ReporterUserId,
    string? Priority,
    string? Search,
    DateTime? CreatedBeforeUtc,
    int Take = 50) : IQuery<IReadOnlyList<TicketListItem>>;

internal sealed class ListOpenTicketsQueryHandler(
    ITicketReadRepository read) : IQueryHandler<ListOpenTicketsQuery, IReadOnlyList<TicketListItem>>
{
    public async Task<Result<IReadOnlyList<TicketListItem>>> Handle(ListOpenTicketsQuery query, CancellationToken cancellationToken)
    {
        var take = Math.Clamp(query.Take, 1, 200);
        var filter = new TicketListFilter(
            ClosedOnly: false,
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
