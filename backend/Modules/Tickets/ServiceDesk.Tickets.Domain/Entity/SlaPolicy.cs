using ServiceDesk.Kernel.Domain;
using ServiceDesk.Tickets.Domain.Enums;

namespace ServiceDesk.Tickets.Domain;

/// <summary>
/// An SLA policy keyed by priority: response and resolution targets (minutes). A separate aggregate
/// (configuration); a ticket reads the matching policy to stamp its due dates.
/// </summary>
public sealed class SlaPolicy : AggregateRoot
{
    private SlaPolicy()
    {
    }

    private SlaPolicy(
        Guid id,
        string name,
        TicketPriority priority,
        int responseMinutes,
        int resolutionMinutes,
        DateTime nowUtc)
        : base(id)
    {
        Name = name;
        Priority = priority;
        ResponseMinutes = responseMinutes;
        ResolutionMinutes = resolutionMinutes;
        IsActive = true;
        CreatedAtUtc = nowUtc;
    }

    public string Name { get; private set; } = null!;

    public TicketPriority Priority { get; private set; }

    public int ResponseMinutes { get; private set; }

    public int ResolutionMinutes { get; private set; }

    public bool IsActive { get; private set; }

    public static SlaPolicy Create(
        string name,
        TicketPriority priority,
        int responseMinutes,
        int resolutionMinutes,
        DateTime nowUtc) =>
        new(NewId(), name, priority, responseMinutes, resolutionMinutes, nowUtc);

    public void Update(string name, int responseMinutes, int resolutionMinutes, DateTime nowUtc)
    {
        Name = name;
        ResponseMinutes = responseMinutes;
        ResolutionMinutes = resolutionMinutes;
        Touch(nowUtc);
    }

    public void SetActive(bool isActive, DateTime nowUtc)
    {
        IsActive = isActive;
        Touch(nowUtc);
    }
}
