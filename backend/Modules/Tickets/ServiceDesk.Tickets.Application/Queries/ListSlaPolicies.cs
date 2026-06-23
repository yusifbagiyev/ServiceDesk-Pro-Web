using ServiceDesk.Application.Abstractions.Messaging;
using ServiceDesk.Tickets.Application.Abstractions;
using ServiceDesk.Kernel.Results;

namespace ServiceDesk.Tickets.Application.Queries;

public sealed record ListSlaPoliciesQuery : IQuery<IReadOnlyList<SlaPolicyListItem>>;

public sealed record SlaPolicyListItem(
    Guid Id,
    string Name,
    string Priority,
    int ResponseMinutes,
    int ResolutionMinutes,
    bool IsActive);

internal sealed class ListSlaPoliciesQueryHandler(ISlaPolicyRepository slaPolicies)
    : IQueryHandler<ListSlaPoliciesQuery, IReadOnlyList<SlaPolicyListItem>>
{
    public async Task<Result<IReadOnlyList<SlaPolicyListItem>>> Handle(
        ListSlaPoliciesQuery query,
        CancellationToken cancellationToken)
    {
        var all = await slaPolicies.ListAsync(cancellationToken);

        IReadOnlyList<SlaPolicyListItem> items =
        [
            .. all.Select(p => new SlaPolicyListItem(
                p.Id,
                p.Name,
                p.Priority.ToString(),
                p.ResponseMinutes,
                p.ResolutionMinutes,
                p.IsActive))
        ];

        return Result.Success(items);
    }
}
