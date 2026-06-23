using ServiceDesk.Application.Abstractions.Messaging;
using ServiceDesk.Kernel.Results;
using ServiceDesk.Tickets.Application.Abstractions;
using ServiceDesk.Tickets.Domain.Exceptions;

namespace ServiceDesk.Tickets.Application.Queries;

public sealed record GetTicketByIdQuery(Guid Id) : IQuery<TicketDetail>;

internal sealed class GetTicketByIdQueryHandler(
    ITicketReadRepository read) : IQueryHandler<GetTicketByIdQuery, TicketDetail>
{
    public async Task<Result<TicketDetail>> Handle(GetTicketByIdQuery query, CancellationToken cancellationToken)
    {
        var dto = await read.GetDetailAsync(query.Id, cancellationToken);
        if (dto is null)
        {
            return TicketErrors.NotFound(query.Id);
        }

        return dto;
    }
}
