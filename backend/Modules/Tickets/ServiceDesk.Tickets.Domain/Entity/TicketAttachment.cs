using ServiceDesk.Kernel.Domain;

namespace ServiceDesk.Tickets.Domain;

/// <summary>Attachment metadata (bytes live in object storage at <see cref="StorageKey"/>). Owned by <see cref="Ticket"/>.</summary>
public sealed class TicketAttachment : Entity
{
    private TicketAttachment()
    {
    }

    internal TicketAttachment(
        Guid ticketId,
        Guid? commentId,
        string fileName,
        string contentType,
        long sizeBytes,
        string storageKey,
        Guid uploadedByUserId,
        DateTime nowUtc)
        : base(NewId())
    {
        TicketId = ticketId;
        CommentId = commentId;
        FileName = fileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        StorageKey = storageKey;
        UploadedByUserId = uploadedByUserId;
        CreatedAtUtc = nowUtc;
    }

    public Guid TicketId { get; private set; }

    public Guid? CommentId { get; private set; }

    public string FileName { get; private set; } = null!;

    /// <summary>Validated by magic bytes at upload, not trusted from the client header.</summary>
    public string ContentType { get; private set; } = null!;

    public long SizeBytes { get; private set; }

    public string StorageKey { get; private set; } = null!;

    public Guid UploadedByUserId { get; private set; }
}
