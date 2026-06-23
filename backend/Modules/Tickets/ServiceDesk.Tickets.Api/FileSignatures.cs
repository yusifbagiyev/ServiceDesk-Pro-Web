namespace ServiceDesk.Tickets.Api;

/// <summary>
/// Validates an uploaded file by its leading magic bytes against an allowlist. Anything that is not a
/// recognised binary type (e.g. SVG, HTML, scripts - which are text and have no signature) is rejected.
/// </summary>
internal static class FileSignatures
{
    private static readonly (byte[] Signature, string ContentType)[] Allowed =
    [
        ([0x89, 0x50, 0x4E, 0x47], "image/png"),
        ([0xFF, 0xD8, 0xFF], "image/jpeg"),
        ([0x47, 0x49, 0x46, 0x38], "image/gif"),
        ([0x25, 0x50, 0x44, 0x46], "application/pdf"),
        ([0x50, 0x4B, 0x03, 0x04], "application/zip"),
    ];

    /// <summary>Returns the validated content type from the stream's header, or null if it is not allowed.</summary>
    public static async Task<string?> DetectAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var header = new byte[8];
        var read = await stream.ReadAsync(header, cancellationToken);

        foreach (var (signature, contentType) in Allowed)
        {
            if (read >= signature.Length && header.AsSpan(0, signature.Length).SequenceEqual(signature))
            {
                return contentType;
            }
        }

        return null;
    }
}
