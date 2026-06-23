using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ServiceDesk.Application.Abstractions.Security;
using ServiceDesk.Kernel.Results;
using ServiceDesk.SharedInfrastructure.Authorization;
using ServiceDesk.SharedInfrastructure.Web;
using ServiceDesk.Tickets.Application.Abstractions;
using ServiceDesk.Tickets.Application.Commands;
using ServiceDesk.Tickets.Application.DTOs;
using ServiceDesk.Tickets.Application.Queries;
using ServiceDesk.Tickets.Domain.Enums;

namespace ServiceDesk.Tickets.Api.Controllers;

[ApiController]
[Route("api/tickets")]
[Authorize]
public sealed class TicketsController(
    ISender sender,
    ICurrentUser currentUser,
    IFileStorage fileStorage,
    IOptions<AttachmentStorageOptions> storageOptions) : ControllerBase
{
    private bool CanViewAll => currentUser.HasPermission(Permissions.TicketsViewAll);

    /// <summary>Create a ticket. A plain user can only open it for themselves; staff may set the reporter.</summary>
    [HttpPost]
    [RequirePermission(Permissions.TicketsCreate)]
    public async Task<IActionResult> Create([FromBody] CreateTicketRequest request, CancellationToken cancellationToken)
    {
        var reporterUserId = CanViewAll ? request.ReporterUserId : null;
        var command = new CreateTicketCommand(
            currentUser.UserId,
            reporterUserId,
            request.Priority,
            request.Title,
            request.InventoryCode,
            request.CategoryIds ?? []);

        return (await sender.Send(command, cancellationToken)).ToActionResult(this);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var (result, canAccess, _) = await LoadAccessAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            return result.ToActionResult(this);
        }

        if (!canAccess)
        {
            return NotFound();
        }

        var ticket = result.Value;

        // Internal comments are helpdesk-private: hide them from non-staff (e.g. the reporter).
        if (!CanViewAll)
        {
            ticket = ticket with
            {
                Comments = [.. ticket.Comments.Where(c => c.Visibility == nameof(CommentVisibility.Public))],
            };
        }

        return Ok(ticket);
    }

    [HttpGet]
    public async Task<IActionResult> ListOpen(
        CancellationToken cancellationToken,
        [FromQuery] Guid? assigneeUserId = null,
        [FromQuery] Guid? reporterUserId = null,
        [FromQuery] string? priority = null,
        [FromQuery] string? search = null,
        [FromQuery] DateTime? createdBeforeUtc = null,
        [FromQuery] int take = 50)
    {
        var query = new ListOpenTicketsQuery(
            CanViewAll ? assigneeUserId : null,
            CanViewAll ? reporterUserId : currentUser.UserId,
            priority,
            search,
            createdBeforeUtc,
            take);

        return (await sender.Send(query, cancellationToken)).ToActionResult(this);
    }

    [HttpGet("closed")]
    public async Task<IActionResult> ListClosed(
        CancellationToken cancellationToken,
        [FromQuery] Guid? assigneeUserId = null,
        [FromQuery] Guid? reporterUserId = null,
        [FromQuery] string? priority = null,
        [FromQuery] string? search = null,
        [FromQuery] DateTime? createdBeforeUtc = null,
        [FromQuery] int take = 50)
    {
        var query = new ListClosedTicketsQuery(
            CanViewAll ? assigneeUserId : null,
            CanViewAll ? reporterUserId : currentUser.UserId,
            priority,
            search,
            createdBeforeUtc,
            take);

        return (await sender.Send(query, cancellationToken)).ToActionResult(this);
    }

    /// <summary>Full cross-ticket search; staff only (plain users use the own-scoped open/closed lists).</summary>
    [HttpGet("search")]
    [RequirePermission(Permissions.TicketsViewAll)]
    public async Task<IActionResult> Search(
        [FromQuery] SearchTicketsQuery query,
        CancellationToken cancellationToken) =>
        (await sender.Send(query, cancellationToken)).ToActionResult(this);

    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.TicketsTransition)]
    public async Task<IActionResult> UpdateDetails(
        Guid id,
        [FromBody] UpdateTicketDetailsRequest request,
        CancellationToken cancellationToken) =>
        (await sender.Send(
            new UpdateTicketDetailsCommand(
                id, request.Title, request.Solution, request.Worker, request.DeviceName, request.DepartmentName),
            cancellationToken)).ToActionResult(this);

    [HttpPost("{id:guid}/transition")]
    [RequirePermission(Permissions.TicketsTransition)]
    public async Task<IActionResult> Transition(
        Guid id,
        [FromBody] TransitionTicketRequest request,
        CancellationToken cancellationToken) =>
        (await sender.Send(
            new TransitionTicketStatusCommand(id, request.Status, currentUser.UserId, request.Note),
            cancellationToken)).ToActionResult(this);

    [HttpPost("{id:guid}/categories")]
    [RequirePermission(Permissions.TicketsTransition)]
    public async Task<IActionResult> SetCategories(
        Guid id,
        [FromBody] SetTicketCategoriesRequest request,
        CancellationToken cancellationToken) =>
        (await sender.Send(new SetTicketCategoriesCommand(id, request.CategoryIds), cancellationToken)).ToActionResult(this);

    [HttpPost("{id:guid}/priority")]
    [RequirePermission(Permissions.TicketsTransition)]
    public async Task<IActionResult> SetPriority(
        Guid id,
        [FromBody] SetTicketPriorityRequest request,
        CancellationToken cancellationToken) =>
        (await sender.Send(new SetTicketPriorityCommand(id, request.Priority), cancellationToken)).ToActionResult(this);

    [HttpPost("{id:guid}/assignees")]
    [RequirePermission(Permissions.TicketsAssign)]
    public async Task<IActionResult> AssignUsers(
        Guid id,
        [FromBody] AssignUsersRequest request,
        CancellationToken cancellationToken) =>
        (await sender.Send(new AssignUsersCommand(id, request.UserIds), cancellationToken)).ToActionResult(this);

    [HttpDelete("{id:guid}/assignees/{userId:guid}")]
    [RequirePermission(Permissions.TicketsAssign)]
    public async Task<IActionResult> Unassign(Guid id, Guid userId, CancellationToken cancellationToken) =>
        (await sender.Send(new UnassignUserCommand(id, userId), cancellationToken)).ToActionResult(this);

    [HttpPost("{id:guid}/comments")]
    [RequirePermission(Permissions.TicketsComment)]
    public async Task<IActionResult> AddComment(
        Guid id,
        [FromBody] AddCommentRequest request,
        CancellationToken cancellationToken)
    {
        var (result, canAccess, _) = await LoadAccessAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            return result.ToActionResult(this);
        }

        if (!canAccess)
        {
            return NotFound();
        }

        return (await sender.Send(
            new AddTicketCommentCommand(id, currentUser.UserId, request.Body, request.Visibility, request.ParentCommentId),
            cancellationToken)).ToActionResult(this);
    }

    [HttpPost("{id:guid}/rating")]
    [RequirePermission(Permissions.TicketsRate)]
    public async Task<IActionResult> SubmitRating(
        Guid id,
        [FromBody] SubmitRatingRequest request,
        CancellationToken cancellationToken)
    {
        var (result, canAccess, isReporter) = await LoadAccessAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            return result.ToActionResult(this);
        }

        if (!canAccess)
        {
            return NotFound();
        }

        // Only the requester (or an admin) rates the resolution, not an assignee.
        if (!isReporter && !CanViewAll)
        {
            return Forbid();
        }

        return (await sender.Send(
            new SubmitTicketRatingCommand(id, request.Score, request.Message, currentUser.UserId),
            cancellationToken)).ToActionResult(this);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.TicketsDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        (await sender.Send(new DeleteTicketCommand(id), cancellationToken)).ToActionResult(this);

    /// <summary>Upload an attachment: validated by magic bytes (SVG/scripts blocked) and streamed to storage.</summary>
    [HttpPost("{id:guid}/attachments")]
    [RequirePermission(Permissions.TicketsAttach)]
    [RequestSizeLimit(26_214_400)]
    public async Task<IActionResult> Upload(
        Guid id,
        IFormFile file,
        CancellationToken cancellationToken,
        [FromQuery] Guid? commentId = null)
    {
        var (result, canAccess, _) = await LoadAccessAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            return result.ToActionResult(this);
        }

        if (!canAccess)
        {
            return NotFound();
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest(new { error = "A non-empty file is required." });
        }

        if (file.Length > storageOptions.Value.MaxBytes)
        {
            return BadRequest(new { error = "The file exceeds the maximum allowed size." });
        }

        await using var stream = file.OpenReadStream();
        var contentType = await FileSignatures.DetectAsync(stream, cancellationToken);
        if (contentType is null)
        {
            return BadRequest(new { error = "Unsupported or unsafe file type." });
        }

        stream.Position = 0;
        var storageKey = await fileStorage.SaveAsync(stream, file.FileName, cancellationToken);

        var command = new AddTicketAttachmentCommand(
            id, commentId, file.FileName, contentType, file.Length, storageKey, currentUser.UserId);

        return (await sender.Send(command, cancellationToken)).ToActionResult(this);
    }

    /// <summary>Download an attachment as an octet-stream (forced download, never inline) for the entitled caller.</summary>
    [HttpGet("{id:guid}/attachments/{attachmentId:guid}/download")]
    public async Task<IActionResult> Download(Guid id, Guid attachmentId, CancellationToken cancellationToken)
    {
        var (result, canAccess, _) = await LoadAccessAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            return result.ToActionResult(this);
        }

        if (!canAccess)
        {
            return NotFound();
        }

        var attachment = result.Value.Attachments.FirstOrDefault(a => a.Id == attachmentId);
        if (attachment is null)
        {
            return NotFound();
        }

        var stream = await fileStorage.OpenReadAsync(attachment.StorageKey, cancellationToken);
        if (stream is null)
        {
            return NotFound();
        }

        return File(stream, "application/octet-stream", attachment.FileName);
    }

    /// <summary>
    /// Loads a ticket and resolves the caller's access: staff (tickets.view.all) reach any ticket,
    /// otherwise only the reporter or an assignee. Used to gate every per-ticket read AND mutation.
    /// </summary>
    private async Task<(Result<TicketDetail> Result, bool CanAccess, bool IsReporter)> LoadAccessAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTicketByIdQuery(id), cancellationToken);
        if (result.IsFailure)
        {
            return (result, false, false);
        }

        var ticket = result.Value;
        var isReporter = ticket.ReporterUserId == currentUser.UserId;
        var canAccess = CanViewAll || isReporter || ticket.Assignees.Any(a => a.UserId == currentUser.UserId);
        return (result, canAccess, isReporter);
    }
}
