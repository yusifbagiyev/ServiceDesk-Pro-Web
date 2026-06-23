using Microsoft.EntityFrameworkCore;
using ServiceDesk.Tickets.Application.Abstractions;
using ServiceDesk.Tickets.Domain.Enums;

namespace ServiceDesk.Tickets.Infrastructure.Persistence;

/// <summary>
/// Read-side projections for ticket lists and detail. Owned child collections load with the aggregate;
/// enum-to-string mapping is done in memory after materialization (EF cannot translate enum.ToString()).
/// </summary>
internal sealed class TicketReadRepository(TicketsDbContext dbContext) : ITicketReadRepository
{
    public async Task<TicketDetail?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ticket = await dbContext.Tickets
            .AsNoTracking()
            .Include(t => t.Rating)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (ticket is null)
        {
            return null;
        }

        return new TicketDetail(
            ticket.Id,
            ticket.Title,
            ticket.Solution,
            ticket.Status.ToString(),
            ticket.Priority.ToString(),
            ticket.InventoryCode,
            ticket.DeviceName,
            ticket.DepartmentName,
            ticket.Worker,
            ticket.ReporterUserId,
            ticket.CreatedByUserId,
            ticket.OpenedAtUtc,
            ticket.FirstResponseAtUtc,
            ticket.ResolvedAtUtc,
            ticket.ClosedAtUtc,
            ticket.ResponseDueAtUtc,
            ticket.ResolutionDueAtUtc,
            ticket.ResponseBreached,
            ticket.ResolutionBreached,
            [.. ticket.Assignees.Select(a => new TicketAssigneeDto(a.AssigneeUserId, a.FullNameSnapshot, a.AssignedAtUtc))],
            [.. ticket.Categories.Select(c => new TicketCategoryDto(c.CategoryId, c.NameSnapshot))],
            [.. ticket.Comments.Select(c => new TicketCommentDto(
                c.Id, c.ParentCommentId, c.AuthorUserId, c.AuthorFullName, c.Body, c.Visibility.ToString(), c.IsEdited, c.CreatedAtUtc))],
            [.. ticket.Attachments.Select(a => new TicketAttachmentDto(
                a.Id, a.CommentId, a.FileName, a.ContentType, a.SizeBytes, a.StorageKey, a.UploadedByUserId, a.CreatedAtUtc))],
            [.. ticket.StatusHistory.Select(h => new TicketStatusHistoryDto(
                h.FromStatus.HasValue ? h.FromStatus.Value.ToString() : null, h.ToStatus.ToString(), h.ChangedByUserId, h.ChangedAtUtc, h.Note))],
            ticket.Rating is null
                ? null
                : new TicketRatingDto(ticket.Rating.Value, ticket.Rating.Message, ticket.Rating.RaterUserId, ticket.Rating.RatedAtUtc));
    }

    public async Task<IReadOnlyList<TicketListItem>> ListAsync(
        TicketListFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Tickets.AsNoTracking();

        query = filter.ClosedOnly
            ? query.Where(t =>
                t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed || t.Status == TicketStatus.Cancelled)
            : query.Where(t =>
                t.Status == TicketStatus.New || t.Status == TicketStatus.Pending || t.Status == TicketStatus.Resolving);

        if (filter.AssigneeUserId is { } assigneeId)
        {
            query = query.Where(t => t.Assignees.Any(a => a.AssigneeUserId == assigneeId));
        }

        if (filter.ReporterUserId is { } reporterId)
        {
            query = query.Where(t => t.ReporterUserId == reporterId);
        }

        if (!string.IsNullOrWhiteSpace(filter.Status) && Enum.TryParse<TicketStatus>(filter.Status, true, out var status))
        {
            query = query.Where(t => t.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(filter.Priority) && Enum.TryParse<TicketPriority>(filter.Priority, true, out var priority))
        {
            query = query.Where(t => t.Priority == priority);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = $"%{filter.Search.Trim()}%";
            query = query.Where(t =>
                (t.Title != null && EF.Functions.ILike(t.Title, pattern)) ||
                (t.InventoryCode != null && EF.Functions.ILike(t.InventoryCode, pattern)) ||
                (t.DeviceName != null && EF.Functions.ILike(t.DeviceName, pattern)));
        }

        // Keyset pagination by creation time (Id is UUID v7, so it is effectively time-ordered too).
        if (filter.CreatedBeforeUtc is { } createdBefore)
        {
            query = query.Where(t => t.CreatedAtUtc < createdBefore);
        }

        var rows = await query
            .OrderByDescending(t => t.CreatedAtUtc)
            .ThenByDescending(t => t.Id)
            .Take(filter.Take)
            .Select(t => new Row(
                t.Id,
                t.Title,
                t.Status,
                t.Priority,
                t.InventoryCode,
                t.DeviceName,
                t.DepartmentName,
                t.ReporterUserId,
                t.OpenedAtUtc,
                t.ResolutionDueAtUtc,
                t.ResolutionBreached,
                t.Rating == null ? null : t.Rating.Value,
                t.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return [.. rows.Select(r => new TicketListItem(
            r.Id,
            r.Title,
            r.Status.ToString(),
            r.Priority.ToString(),
            r.InventoryCode,
            r.DeviceName,
            r.DepartmentName,
            r.ReporterUserId,
            r.OpenedAtUtc,
            r.ResolutionDueAtUtc,
            r.ResolutionBreached,
            r.Rating,
            r.CreatedAtUtc))];
    }

    private sealed record Row(
        Guid Id,
        string? Title,
        TicketStatus Status,
        TicketPriority Priority,
        string? InventoryCode,
        string? DeviceName,
        string? DepartmentName,
        Guid? ReporterUserId,
        DateTime OpenedAtUtc,
        DateTime? ResolutionDueAtUtc,
        bool ResolutionBreached,
        int? Rating,
        DateTime CreatedAtUtc);
}
