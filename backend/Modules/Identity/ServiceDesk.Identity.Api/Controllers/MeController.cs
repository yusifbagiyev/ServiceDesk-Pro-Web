using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Application.Abstractions.Security;
using ServiceDesk.Identity.Application.Commands;
using ServiceDesk.Identity.Application.Queries;
using ServiceDesk.SharedInfrastructure.Web;

namespace ServiceDesk.Identity.Api.Controllers;

[ApiController]
[Route("api/me")]
[Authorize]
public sealed class MeController(ISender sender, ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken) =>
        (await sender.Send(new GetMeQuery(), cancellationToken)).ToActionResult(this);

    [HttpPost("password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangeOwnPasswordCommand command,
        CancellationToken cancellationToken) =>
        (await sender.Send(command, cancellationToken)).ToActionResult(this);

    [HttpPost("contact")]
    public async Task<IActionResult> UpdateContact(
        [FromBody] UpdateContactRequest request,
        CancellationToken cancellationToken) =>
        (await sender.Send(
            new UpdateUserContactCommand(currentUser.UserId, request.PhoneNumber, request.WhatsAppOptIn),
            cancellationToken)).ToActionResult(this);

    public sealed record UpdateContactRequest(string? PhoneNumber, bool WhatsAppOptIn);
}
