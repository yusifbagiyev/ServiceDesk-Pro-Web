using ServiceDesk.Application.Abstractions.Messaging;
using ServiceDesk.Identity.Application.Abstractions;
using ServiceDesk.Kernel.Results;

namespace ServiceDesk.Identity.Application.Queries;

/// <summary>Lightweight directory of active users for assignee / mention pickers (any authenticated user).</summary>
public sealed record ListUserDirectoryQuery : IQuery<IReadOnlyList<UserDirectoryItem>>;

public sealed record UserDirectoryItem(Guid Id, string FullName, string Email);

internal sealed class ListUserDirectoryQueryHandler(IUserRepository users)
    : IQueryHandler<ListUserDirectoryQuery, IReadOnlyList<UserDirectoryItem>>
{
    public async Task<Result<IReadOnlyList<UserDirectoryItem>>> Handle(
        ListUserDirectoryQuery query,
        CancellationToken cancellationToken)
    {
        var all = await users.ListAsync(cancellationToken);

        IReadOnlyList<UserDirectoryItem> items =
            [.. all
                .Where(user => user.IsActive)
                .Select(user => new UserDirectoryItem(user.Id, user.FullName, user.Email))];

        return Result.Success(items);
    }
}
