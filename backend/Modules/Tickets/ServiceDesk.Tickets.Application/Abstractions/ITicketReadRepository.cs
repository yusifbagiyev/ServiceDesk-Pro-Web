namespace ServiceDesk.Tickets.Application.Abstractions;

public sealed record TicketListFilter(
    bool ClosedOnly,
    Guid? AssigneeUserId,
    Guid? ReporterUserId,
    string? Status,
    string? Priority,
    string? Search,
    DateTime? CreatedBeforeUtc,
    int Take);

public sealed record TicketListItem(
    Guid Id,
    string? Title,
    string Status,
    string Priority,
    string? InventoryCode,
    string? DeviceName,
    string? DepartmentName,
    Guid? ReporterUserId,
    DateTime OpenedAtUtc,
    DateTime? ResolutionDueAtUtc,
    bool ResolutionBreached,
    int? Rating,
    DateTime CreatedAtUtc);

public sealed record TicketAssigneeDto(
    Guid UserId,
    string FullName,
    DateTime AssignedAtUtc);

public sealed record TicketCategoryDto(
    Guid CategoryId,
    string Name);

public sealed record TicketCommentDto(
    Guid Id,
    Guid? ParentCommentId,
    Guid AuthorUserId,
    string AuthorFullName,
    string Body,
    string Visibility,
    bool IsEdited,
    DateTime CreatedAtUtc);

public sealed record TicketAttachmentDto(
    Guid Id,
    Guid? CommentId,
    string FileName,
    string ContentType,
    long SizeBytes,
    string StorageKey,
    Guid UploadedByUserId,
    DateTime CreatedAtUtc);

public sealed record TicketStatusHistoryDto(
    string? FromStatus,
    string ToStatus,
    Guid? ChangedByUserId,
    DateTime ChangedAtUtc,
    string? Note);

public sealed record TicketRatingDto(
    int Value,
    string? Message,
    Guid? RaterUserId,
    DateTime RatedAtUtc);

public sealed record TicketDetail(
    Guid Id,
    string? Title,
    string? Solution,
    string Status,
    string Priority,
    string? InventoryCode,
    string? DeviceName,
    string? DepartmentName,
    string? Worker,
    Guid? ReporterUserId,
    Guid? CreatedByUserId,
    DateTime OpenedAtUtc,
    DateTime? FirstResponseAtUtc,
    DateTime? ResolvedAtUtc,
    DateTime? ClosedAtUtc,
    DateTime? ResponseDueAtUtc,
    DateTime? ResolutionDueAtUtc,
    bool ResponseBreached,
    bool ResolutionBreached,
    IReadOnlyList<TicketAssigneeDto> Assignees,
    IReadOnlyList<TicketCategoryDto> Categories,
    IReadOnlyList<TicketCommentDto> Comments,
    IReadOnlyList<TicketAttachmentDto> Attachments,
    IReadOnlyList<TicketStatusHistoryDto> History,
    TicketRatingDto? Rating);

public interface ITicketReadRepository
{
    Task<TicketDetail?> GetDetailAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<TicketListItem>> ListAsync(TicketListFilter filter, CancellationToken ct = default);
}
