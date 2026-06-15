namespace ServiceDesk.Identity.Domain.Enums;

/// <summary>
/// The fixed role set. Legacy <c>Users.type</c> maps: 'User' -> User, 'Admin' -> Admin.
/// </summary>
public enum UserRole
{
    User = 0,
    Admin = 1,
}
