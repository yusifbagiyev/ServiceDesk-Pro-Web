namespace ServiceDesk.Identity.Domain.Enums;

/// <summary>
/// The fixed role set: User (employee), Admin (full control), and Worker (helpdesk / IT staff who
/// creates and resolves tickets on behalf of users). The legacy Users table is not migrated.
/// </summary>
public enum UserRole
{
    User = 0,
    Admin = 1,

    /// <summary>Helpdesk / IT staff: creates and resolves tickets on behalf of users.</summary>
    Worker = 2,
}
