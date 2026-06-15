using ServiceDesk.Kernel.Results;

namespace ServiceDesk.Identity.Domain.Exceptions;

/// <summary>Typed errors for the Identity module.</summary>
public static class UserErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Users.NotFound", $"User '{id}' was not found.");

    public static readonly Error EmailTaken =
        Error.Conflict("Users.EmailTaken", "A user with this email already exists.");

    public static readonly Error InvalidCredentials =
        Error.Unauthorized("Users.InvalidCredentials", "Invalid username or password.");

    public static readonly Error Inactive =
        Error.Forbidden("Users.Inactive", "This account is deactivated.");

    public static readonly Error CannotDemoteLastAdmin =
        Error.Conflict("Users.LastAdmin", "The last administrator cannot be demoted or deactivated.");
}
