namespace ServiceDesk.Application.Abstractions.Time;

/// <summary>
/// Abstracts the system clock so handlers and aggregates receive UTC time as a dependency
/// (testable, and keeps reducers/domain logic free of ambient <c>DateTime.UtcNow</c> reads).
/// </summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
