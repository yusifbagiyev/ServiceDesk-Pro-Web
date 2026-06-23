namespace ServiceDesk.Tickets.Application.Abstractions;

/// <summary>
/// Binary attachment storage (local disk / object store). Only the returned opaque storage key is
/// persisted on the ticket; the bytes are streamed straight to storage.
/// </summary>
public interface IFileStorage
{
    Task<string> SaveAsync(Stream content, string fileName, CancellationToken cancellationToken = default);

    /// <summary>Open a stored file for reading; null if the key is unknown or escapes the storage root.</summary>
    Task<Stream?> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default);
}

/// <summary>Attachment storage + upload-limit options (bound from the host configuration).</summary>
public sealed class AttachmentStorageOptions
{
    public string RootPath { get; set; } = "attachments";

    public long MaxBytes { get; set; } = 25 * 1024 * 1024;
}
