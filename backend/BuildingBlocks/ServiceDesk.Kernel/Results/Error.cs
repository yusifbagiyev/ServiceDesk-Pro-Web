namespace ServiceDesk.Kernel.Results;

/// <summary>
/// The category of a failure. Controllers map this to an HTTP status code,
/// so handlers never need to know about HTTP.
/// </summary>
public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Forbidden = 4,
    Unauthorized = 5,
}

/// <summary>
/// A typed, expected failure. Returned inside <see cref="Result"/> rather than thrown.
/// </summary>
public record Error(string Code, string Message, ErrorType Type)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    public static Error Failure(string code, string message) => new(code, message, ErrorType.Failure);

    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);

    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);

    public static Error Forbidden(string code, string message) => new(code, message, ErrorType.Forbidden);

    public static Error Unauthorized(string code, string message) => new(code, message, ErrorType.Unauthorized);
}

/// <summary>
/// Aggregates one or more field-level validation failures. Produced by the validation
/// pipeline behavior and unpacked into a ProblemDetails <c>errors</c> dictionary by the API.
/// </summary>
public sealed record ValidationError(Error[] Errors)
    : Error("Validation.General", "One or more validation errors occurred.", ErrorType.Validation);
