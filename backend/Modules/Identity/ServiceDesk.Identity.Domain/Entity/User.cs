using ServiceDesk.Identity.Domain.Enums;
using ServiceDesk.Identity.Domain.Events;
using ServiceDesk.Kernel.Domain;

namespace ServiceDesk.Identity.Domain.Entity;

/// <summary>
/// A ServiceDesk user. <see cref="Email"/> is the unique login handle, stored normalized
/// (trimmed + lowercased) so the unique index and login lookups are case-insensitive.
/// <see cref="FullName"/> is a display name only. <see cref="PhoneNumber"/> doubles as the
/// WhatsApp number for SLA reminders.
/// </summary>
public sealed class User : AggregateRoot
{
    private User()
    {
        // EF Core materialization.
    }

    private User(Guid id, string email, string fullName, string passwordHash, UserRole role, DateTime nowUtc)
        : base(id)
    {
        Email = NormalizeEmail(email);
        FullName = fullName;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
        WhatsAppOptIn = true;
        CreatedAtUtc = nowUtc;
    }

    /// <summary>The login handle, stored normalized (trimmed + lowercased).</summary>
    public string Email { get; private set; } = null!;

    /// <summary>Display name (shown on tickets, dashboards, audit). Not a login handle.</summary>
    public string FullName { get; private set; } = null!;

    public string PasswordHash { get; private set; } = null!;

    public UserRole Role { get; private set; }

    public int? Csat { get; private set; }

    /// <summary>Mobile number in E.164; also the WhatsApp number for reminders.</summary>
    public string? PhoneNumber { get; private set; }

    public bool WhatsAppOptIn { get; private set; }

    public bool IsActive { get; private set; }

    public static User Create(
        string email,
        string fullName,
        string passwordHash,
        UserRole role,
        DateTime nowUtc,
        string? phoneNumber = null)
    {
        var user = new User(NewId(), email, fullName, passwordHash, role, nowUtc)
        {
            PhoneNumber = phoneNumber,
        };

        user.RaiseDomainEvent(new UserCreatedDomainEvent(user.Id, user.FullName, user.Role, nowUtc));
        return user;
    }

    public void ChangePassword(string newPasswordHash, DateTime nowUtc)
    {
        PasswordHash = newPasswordHash;
        Touch(nowUtc);
        RaiseDomainEvent(new UserPasswordChangedDomainEvent(Id, FullName, nowUtc));
    }

    public void Rename(string fullName, DateTime nowUtc)
    {
        FullName = fullName;
        Touch(nowUtc);
    }

    public void ChangeEmail(string email, DateTime nowUtc)
    {
        Email = NormalizeEmail(email);
        Touch(nowUtc);
    }

    public void ChangeRole(UserRole newRole, DateTime nowUtc)
    {
        if (Role == newRole)
        {
            return;
        }

        var oldRole = Role;
        Role = newRole;
        Touch(nowUtc);
        RaiseDomainEvent(new UserRoleChangedDomainEvent(Id, FullName, oldRole, newRole, nowUtc));
    }

    public void UpdateContact(string? phoneNumber, bool whatsAppOptIn, DateTime nowUtc)
    {
        PhoneNumber = phoneNumber;
        WhatsAppOptIn = whatsAppOptIn;
        Touch(nowUtc);
    }

    public void Deactivate(DateTime nowUtc)
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        Touch(nowUtc);
        RaiseDomainEvent(new UserDeactivatedDomainEvent(Id, FullName, nowUtc));
    }

    public void Reactivate(DateTime nowUtc)
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        Touch(nowUtc);
        RaiseDomainEvent(new UserReactivatedDomainEvent(Id, FullName, nowUtc));
    }

    public void SetCsat(int csat, DateTime nowUtc)
    {
        Csat = Math.Clamp(csat, 0, 100);
        Touch(nowUtc);
    }

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
