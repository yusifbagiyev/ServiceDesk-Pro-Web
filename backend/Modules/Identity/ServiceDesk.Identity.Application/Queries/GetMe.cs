using ServiceDesk.Application.Abstractions.Messaging;
using ServiceDesk.Application.Abstractions.Security;
using ServiceDesk.Identity.Application.Abstractions;
using ServiceDesk.Identity.Domain.Exceptions;
using ServiceDesk.Kernel.Results;

namespace ServiceDesk.Identity.Application.Queries;

public sealed record GetMeQuery : IQuery<MeResponse>;

public sealed record MeResponse(
    Guid Id,
    string Email,
    string FullName,
    string Role,
    string? PhoneNumber,
    bool WhatsAppOptIn,
    int? Csat);

internal sealed class GetMeQueryHandler(IUserRepository users, ICurrentUser currentUser)
    : IQueryHandler<GetMeQuery, MeResponse>
{
    public async Task<Result<MeResponse>> Handle(GetMeQuery query, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(currentUser.UserId, cancellationToken);
        if (user is null)
        {
            return UserErrors.NotFound(currentUser.UserId);
        }

        return new MeResponse(
            user.Id,
            user.Email,
            user.FullName,
            user.Role.ToString(),
            user.PhoneNumber,
            user.WhatsAppOptIn,
            user.Csat);
    }
}
