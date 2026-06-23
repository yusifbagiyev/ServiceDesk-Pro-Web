using MediatR;
using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Tickets.Application.Commands;
using ServiceDesk.Tickets.Application.DTOs;
using ServiceDesk.Tickets.Application.Queries;
using ServiceDesk.SharedInfrastructure.Authorization;
using ServiceDesk.SharedInfrastructure.Web;

namespace ServiceDesk.Tickets.Api.Controllers;

[ApiController]
[Route("api/sla-policies")]
[RequirePermission(Permissions.SlaManage)]
public sealed class SlaPoliciesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        (await sender.Send(new ListSlaPoliciesQuery(), cancellationToken)).ToActionResult(this);

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateSlaPolicyRequest request,
        CancellationToken cancellationToken) =>
        (await sender.Send(
            new CreateSlaPolicyCommand(request.Name, request.Priority, request.ResponseMinutes, request.ResolutionMinutes),
            cancellationToken)).ToActionResult(this);

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateSlaPolicyRequest request,
        CancellationToken cancellationToken) =>
        (await sender.Send(
            new UpdateSlaPolicyCommand(id, request.Name, request.ResponseMinutes, request.ResolutionMinutes),
            cancellationToken)).ToActionResult(this);

    [HttpPost("{id:guid}/activation")]
    public async Task<IActionResult> SetActivation(
        Guid id,
        [FromBody] SetSlaPolicyActivationRequest request,
        CancellationToken cancellationToken) =>
        (await sender.Send(new SetSlaPolicyActivationCommand(id, request.IsActive), cancellationToken)).ToActionResult(this);
}
