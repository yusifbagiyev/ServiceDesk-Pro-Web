using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Identity.Application.Queries;
using ServiceDesk.SharedInfrastructure.Web;

namespace ServiceDesk.Identity.Api.Controllers;

/// <summary>
/// The active-user directory for assignee / mention pickers. Any authenticated user may read it
/// (internal names + emails); it exposes none of the admin user-management fields.
/// </summary>
[ApiController]
[Route("api/users/directory")]
[Authorize]
public sealed class UserDirectoryController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        (await sender.Send(new ListUserDirectoryQuery(), cancellationToken)).ToActionResult(this);
}
