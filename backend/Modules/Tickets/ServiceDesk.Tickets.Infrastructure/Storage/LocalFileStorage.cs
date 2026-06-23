using Microsoft.Extensions.Options;
using ServiceDesk.Tickets.Application.Abstractions;

namespace ServiceDesk.Tickets.Infrastructure.Storage;

/// <summary>
/// Streams attachment bytes to a directory on disk under a random key. A path-traversal guard keeps
/// reads inside the configured root. (Swap for an object-store implementation in production.)
/// </summary>
internal sealed class LocalFileStorage : IFileStorage
{
    private readonly string _root;

    public LocalFileStorage(IOptions<AttachmentStorageOptions> options)
    {
        var root = Path.GetFullPath(options.Value.RootPath);

        // Keep a trailing separator so the StartsWith boundary check cannot be bypassed by a
        // sibling directory that merely shares the root as a name prefix (e.g. "attachments_evil").
        _root = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;
    }

    public async Task<string> SaveAsync(Stream content, string fileName, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName);
        var key = $"{Guid.CreateVersion7():n}{extension}";
        Directory.CreateDirectory(_root);

        var fullPath = Path.Combine(_root, key);
        await using var file = File.Create(fullPath);
        await content.CopyToAsync(file, cancellationToken);

        return key;
    }

    public Task<Stream?> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_root, storageKey));

        // Defence in depth: never read outside the storage root even if the key were tampered with.
        if (!fullPath.StartsWith(_root, StringComparison.OrdinalIgnoreCase) || !File.Exists(fullPath))
        {
            return Task.FromResult<Stream?>(null);
        }

        return Task.FromResult<Stream?>(File.OpenRead(fullPath));
    }
}
