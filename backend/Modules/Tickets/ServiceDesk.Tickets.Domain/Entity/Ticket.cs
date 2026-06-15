using ServiceDesk.Kernel.Domain;
using ServiceDesk.Kernel.Results;
using ServiceDesk.Tickets.Domain.Enums;
using ServiceDesk.Tickets.Domain.Events;
using ServiceDesk.Tickets.Domain.Exceptions;

namespace ServiceDesk.Tickets.Domain;

/// <summary>
/// The ticket aggregate root. Owns its assignees, categories, status-history timeline, comments,
/// attachments and rating. Inventory fields are snapshots captured at creation (no FK to inventory).
/// </summary>
public sealed class Ticket : AggregateRoot
{
    private readonly List<TicketAssignee> _assignees = [];
    private readonly List<TicketCategory> _categories = [];
    private readonly List<TicketStatusHistory> _statusHistory = [];
    private readonly List<TicketComment> _comments = [];
    private readonly List<TicketAttachment> _attachments = [];

    private Ticket()
    {
    }

    private Ticket(Guid id, DateTime openedAtUtc, DateTime createdAtUtc, TicketPriority priority, Guid? createdByUserId)
        : base(id)
    {
        OpenedAtUtc = openedAtUtc;
        Priority = priority;
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = createdAtUtc;
    }

    public string? InventoryCode { get; private set; }

    public string? DepartmentName { get; private set; }

    public string? Worker { get; private set; }

    public string? DeviceName { get; private set; }

    public string? Title { get; private set; }

    public string? Solution { get; private set; }

    public TicketStatus Status { get; private set; }

    public TicketPriority Priority { get; private set; }

    public DateTime OpenedAtUtc { get; private set; }

    public DateTime? FirstResponseAtUtc { get; private set; }

    public DateTime? ResolvedAtUtc { get; private set; }

    public DateTime? ClosedAtUtc { get; private set; }

    public Guid? SlaPolicyId { get; private set; }

    public DateTime? ResponseDueAtUtc { get; private set; }

    public DateTime? ResolutionDueAtUtc { get; private set; }

    public bool ResponseBreached { get; private set; }

    public bool ResolutionBreached { get; private set; }

    public Guid? CreatedByUserId { get; private set; }

    /// <summary>Original integer id from the legacy MSSQL <c>Ticket</c> table (ETL idempotency / traceability).</summary>
    public int? LegacyId { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTime? DeletedAtUtc { get; private set; }

    public TicketRating? Rating { get; private set; }

    public IReadOnlyList<TicketAssignee> Assignees => _assignees.AsReadOnly();

    public IReadOnlyList<TicketCategory> Categories => _categories.AsReadOnly();

    public IReadOnlyList<TicketStatusHistory> StatusHistory => _statusHistory.AsReadOnly();

    public IReadOnlyList<TicketComment> Comments => _comments.AsReadOnly();

    public IReadOnlyList<TicketAttachment> Attachments => _attachments.AsReadOnly();

    public static Ticket Create(
        DateTime nowUtc,
        TicketPriority priority,
        Guid? createdByUserId,
        string? inventoryCode,
        string? departmentName,
        string? worker,
        string? deviceName,
        string? title)
    {
        var ticket = new Ticket(NewId(), nowUtc, nowUtc, priority, createdByUserId)
        {
            InventoryCode = inventoryCode,
            DepartmentName = departmentName,
            Worker = worker,
            DeviceName = deviceName,
            Title = title,
            Status = TicketStatus.Pending,
        };

        ticket._statusHistory.Add(
            new TicketStatusHistory(ticket.Id, null, TicketStatus.Pending, createdByUserId, nowUtc, "Created"));
        ticket.RaiseDomainEvent(new TicketCreatedDomainEvent(ticket.Id, createdByUserId, nowUtc));
        return ticket;
    }

    /// <summary>Legacy "not today": create a ticket already closed at a past date.</summary>
    public static Ticket CreateBackdatedClosed(
        DateTime closedAtUtc,
        DateTime nowUtc,
        TicketPriority priority,
        Guid? createdByUserId,
        string? inventoryCode,
        string? departmentName,
        string? worker,
        string? deviceName,
        string? title,
        string solution)
    {
        var ticket = new Ticket(NewId(), closedAtUtc, nowUtc, priority, createdByUserId)
        {
            InventoryCode = inventoryCode,
            DepartmentName = departmentName,
            Worker = worker,
            DeviceName = deviceName,
            Title = title,
            Solution = solution,
            Status = TicketStatus.Closed,
            ResolvedAtUtc = closedAtUtc,
            ClosedAtUtc = closedAtUtc,
        };

        ticket._statusHistory.Add(new TicketStatusHistory(
            ticket.Id, null, TicketStatus.Closed, createdByUserId, closedAtUtc, "Created (backdated, closed)"));
        ticket.RaiseDomainEvent(new TicketCreatedDomainEvent(ticket.Id, createdByUserId, nowUtc));
        return ticket;
    }

    /// <summary>Stamp SLA due dates from the policy (called on create and on priority change).</summary>
    public void ApplySla(SlaPolicy policy, DateTime nowUtc)
    {
        SlaPolicyId = policy.Id;
        ResponseDueAtUtc = OpenedAtUtc.AddMinutes(policy.ResponseMinutes);
        ResolutionDueAtUtc = OpenedAtUtc.AddMinutes(policy.ResolutionMinutes);
        Touch(nowUtc);
    }

    public void AssignUser(Guid userId, string fullNameSnapshot, DateTime nowUtc)
    {
        if (_assignees.Exists(assignee => assignee.AssigneeUserId == userId))
        {
            return;
        }

        _assignees.Add(new TicketAssignee(Id, userId, fullNameSnapshot, nowUtc));
        Touch(nowUtc);
        RaiseDomainEvent(new TicketAssignedDomainEvent(Id, userId, nowUtc));
    }

    public void UnassignUser(Guid userId, DateTime nowUtc)
    {
        var existing = _assignees.Find(assignee => assignee.AssigneeUserId == userId);
        if (existing is not null)
        {
            _assignees.Remove(existing);
            Touch(nowUtc);
        }
    }

    public void AddCategory(Guid categoryId, string nameSnapshot, DateTime nowUtc)
    {
        if (_categories.Exists(category => category.CategoryId == categoryId))
        {
            return;
        }

        _categories.Add(new TicketCategory(Id, categoryId, nameSnapshot, nowUtc));
        Touch(nowUtc);
    }

    public void ClearCategories(DateTime nowUtc)
    {
        _categories.Clear();
        Touch(nowUtc);
    }

    public void SetPriority(TicketPriority priority, DateTime nowUtc)
    {
        Priority = priority;
        Touch(nowUtc);
    }

    public void UpdateDetails(
        string? title,
        string? solution,
        string? worker,
        string? deviceName,
        string? departmentName,
        DateTime nowUtc)
    {
        Title = title;
        Solution = solution;
        Worker = worker;
        DeviceName = deviceName;
        DepartmentName = departmentName;
        Touch(nowUtc);
    }

    public Result Transition(TicketStatus toStatus, Guid? byUserId, DateTime nowUtc, string? note)
    {
        if (IsDeleted)
        {
            return TicketErrors.AlreadyDeleted;
        }

        if (!TicketStatusTransitions.CanTransition(Status, toStatus))
        {
            return TicketErrors.InvalidTransition(Status, toStatus);
        }

        if (toStatus is TicketStatus.Resolved or TicketStatus.Closed)
        {
            if (_assignees.Count == 0)
            {
                return TicketErrors.NoAssignees;
            }

            if (_categories.Count == 0)
            {
                return TicketErrors.NoCategories;
            }

            if (string.IsNullOrWhiteSpace(Solution))
            {
                return TicketErrors.SolutionRequired;
            }
        }

        var fromStatus = Status;
        Status = toStatus;

        if (FirstResponseAtUtc is null && toStatus is TicketStatus.Resolving)
        {
            FirstResponseAtUtc = nowUtc;
        }

        switch (toStatus)
        {
            case TicketStatus.Resolved:
                ResolvedAtUtc ??= nowUtc;
                break;
            case TicketStatus.Closed:
                ResolvedAtUtc ??= nowUtc;
                ClosedAtUtc = nowUtc;
                break;
            case TicketStatus.Resolving:
                ClosedAtUtc = null;
                break;
            default:
                break;
        }

        _statusHistory.Add(new TicketStatusHistory(Id, fromStatus, toStatus, byUserId, nowUtc, note));
        Touch(nowUtc);
        RaiseDomainEvent(new TicketStatusChangedDomainEvent(Id, fromStatus, toStatus, byUserId, nowUtc));
        return Result.Success();
    }

    public Guid AddComment(
        Guid authorUserId,
        string authorFullName,
        string body,
        CommentVisibility visibility,
        Guid? parentCommentId,
        DateTime nowUtc)
    {
        var comment = new TicketComment(Id, parentCommentId, authorUserId, authorFullName, body, visibility, nowUtc);
        _comments.Add(comment);
        Touch(nowUtc);
        RaiseDomainEvent(new TicketCommentAddedDomainEvent(Id, comment.Id, authorUserId, nowUtc));
        return comment.Id;
    }

    public Guid AddAttachment(
        Guid? commentId,
        string fileName,
        string contentType,
        long sizeBytes,
        string storageKey,
        Guid uploadedByUserId,
        DateTime nowUtc)
    {
        var attachment = new TicketAttachment(
            Id, commentId, fileName, contentType, sizeBytes, storageKey, uploadedByUserId, nowUtc);
        _attachments.Add(attachment);
        Touch(nowUtc);
        return attachment.Id;
    }

    public void SubmitRating(int score, string? message, Guid? raterUserId, DateTime nowUtc)
    {
        if (Rating is null)
        {
            Rating = new TicketRating(Id, score, message, raterUserId, nowUtc);
        }
        else
        {
            Rating.Update(score, message, nowUtc);
        }

        Touch(nowUtc);
        RaiseDomainEvent(new TicketRatingSubmittedDomainEvent(
            Id, score, _assignees.Select(assignee => assignee.AssigneeUserId).ToList(), nowUtc));
    }

    public void SoftDelete(DateTime nowUtc)
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        DeletedAtUtc = nowUtc;
        Touch(nowUtc);
    }
}
