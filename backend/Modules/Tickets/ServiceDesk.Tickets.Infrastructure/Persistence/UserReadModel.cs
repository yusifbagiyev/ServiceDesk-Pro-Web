namespace ServiceDesk.Tickets.Infrastructure.Persistence;

/// <summary>
/// Read-only projection of the Identity-owned <c>users</c> table, mapped <c>ExcludeFromMigrations</c>.
/// Lets the Tickets module resolve a user's name/status for snapshots without a cross-module service or FK.
/// </summary>
internal sealed class UserReadModel
{
    public Guid Id { get; private set; }

    public string FullName { get; private set; } = null!;

    public string Email { get; private set; } = null!;

    public bool IsActive { get; private set; }
}
