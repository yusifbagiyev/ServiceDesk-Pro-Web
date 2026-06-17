using MediatR;
using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Identity.Application.Commands;
using ServiceDesk.Identity.Application.DTOs;
using ServiceDesk.Identity.Application.Queries;
using ServiceDesk.SharedInfrastructure.Authentication;
using ServiceDesk.SharedInfrastructure.Authorization;
using ServiceDesk.SharedInfrastructure.Web;

namespace ServiceDesk.Identity.Api.Controllers;

[ApiController]
[Route("api/users")]
[RequirePermission(Permissions.UsersManage)]
public sealed partial class UsersController(ISender sender, ISessionStore sessionStore) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        (await sender.Send(new ListUsersQuery(), cancellationToken)).ToActionResult(this);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand command, CancellationToken cancellationToken) =>
        (await sender.Send(command, cancellationToken)).ToActionResult(this);

    [HttpPost("{id:guid}/role")]
    public async Task<IActionResult> ChangeRole(
        Guid id,
        [FromBody] ChangeRoleRequest request,
        CancellationToken cancellationToken) =>
        (await sender.Send(new ChangeUserRoleCommand(id, request.Role), cancellationToken)).ToActionResult(this);

    [HttpPost("{id:guid}/activation")]
    public async Task<IActionResult> SetActivation(
        Guid id,
        [FromBody] SetActivationRequest request,
        CancellationToken cancellationToken) =>
        (await sender.Send(new SetUserActivationCommand(id, request.IsActive), cancellationToken)).ToActionResult(this);

    /// <summary>Revoke all of a user's sessions immediately (force re-login on every device).</summary>
    [HttpPost("{id:guid}/force-logout")]
    [RequirePermission(Permissions.UsersForceLogout)]
    public async Task<IActionResult> ForceLogout(Guid id, CancellationToken cancellationToken)
    {
        await sessionStore.RemoveAllForUserAsync(id, cancellationToken);
        return NoContent();
    }
}
