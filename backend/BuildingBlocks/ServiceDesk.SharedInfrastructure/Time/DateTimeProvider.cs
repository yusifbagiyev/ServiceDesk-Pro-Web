using ServiceDesk.Application.Abstractions.Time;

namespace ServiceDesk.SharedInfrastructure.Time;

/// <summary>UTC system clock. The single source of "now" for handlers, aggregates, and workers.</summary>
public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
