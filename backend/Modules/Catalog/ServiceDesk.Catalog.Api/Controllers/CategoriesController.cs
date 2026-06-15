using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Catalog.Application.Categories;
using ServiceDesk.SharedInfrastructure.Authorization;
using ServiceDesk.SharedInfrastructure.Web;

namespace ServiceDesk.Catalog.Api.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public sealed class CategoriesController(ISender sender) : ControllerBase
{
    /// <summary>List categories (the ticket category picker); any authenticated user.</summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken, [FromQuery] bool activeOnly = true) =>
        (await sender.Send(new ListCategoriesQuery(activeOnly), cancellationToken)).ToActionResult(this);

    [HttpPost]
    [RequirePermission(Permissions.CategoriesManage)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCategoryCommand command,
        CancellationToken cancellationToken) =>
        (await sender.Send(command, cancellationToken)).ToActionResult(this);

    [HttpPost("{id:guid}/rename")]
    [RequirePermission(Permissions.CategoriesManage)]
    public async Task<IActionResult> Rename(
        Guid id,
        [FromBody] RenameRequest request,
        CancellationToken cancellationToken) =>
        (await sender.Send(new RenameCategoryCommand(id, request.Name), cancellationToken)).ToActionResult(this);

    [HttpPost("{id:guid}/activation")]
    [RequirePermission(Permissions.CategoriesManage)]
    public async Task<IActionResult> SetActivation(
        Guid id,
        [FromBody] ActivationRequest request,
        CancellationToken cancellationToken) =>
        (await sender.Send(new SetCategoryActivationCommand(id, request.IsActive), cancellationToken)).ToActionResult(this);

    public sealed record RenameRequest(string Name);

    public sealed record ActivationRequest(bool IsActive);
}
