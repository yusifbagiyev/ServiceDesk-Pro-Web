namespace ServiceDesk.Tickets.Infrastructure.Persistence;

/// <summary>
/// Read-only projection of the Catalog-owned <c>categories</c> table, mapped <c>ExcludeFromMigrations</c>.
/// Lets the Tickets module resolve a category's name/status for snapshots without a cross-module service or FK.
/// </summary>
internal sealed class CategoryReadModel
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    public bool IsActive { get; private set; }
}
