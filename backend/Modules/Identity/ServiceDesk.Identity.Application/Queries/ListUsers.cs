using ServiceDesk.Application.Abstractions.Messaging;
using ServiceDesk.Identity.Application.Abstractions;
using ServiceDesk.Kernel.Results;

namespace ServiceDesk.Identity.Application.Queries;

public sealed record ListUsersQuery : IQuery<IReadOnlyList<UserListItem>>;

public sealed record UserListItem(
    Guid Id,
    string FullName,
    string Role,
    bool IsActive,
    string? Email,
    string? PhoneNumber,
    int? Csat);

internal sealed class ListUsersQueryHandler(IUserRepository users)
    : IQueryHandler<ListUsersQuery, IReadOnlyList<UserListItem>>
{
    public async Task<Result<IReadOnlyList<UserListItem>>> Handle(
        ListUsersQuery query,
        CancellationToken cancellationToken)
    {
        var all = await users.ListAsync(cancellationToken);

        IReadOnlyList<UserListItem> items = all
            .Select(user => new UserListItem(
                user.Id,
                user.FullName,
                user.Role.ToString(),
                user.IsActive,
                user.Email,
                user.PhoneNumber,
                user.Csat))
            .ToList();

        return Result.Success(items);
    }
}
