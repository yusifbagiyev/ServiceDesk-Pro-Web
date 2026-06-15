namespace ServiceDesk.Kernel.Domain;

/// <summary>
/// Base for all persisted entities. Carries a client-generated UUID v7 primary key
/// (configured <c>ValueGeneratedNever</c> in EF) and UTC audit timestamps.
/// </summary>
public abstract class Entity
{
    protected Entity(Guid id) => Id = id;

    /// <summary>Required by EF Core materialization.</summary>
    protected Entity()
    {
    }

    public Guid Id { get; protected set; }

    public DateTime CreatedAtUtc { get; protected set; }

    public DateTime? UpdatedAtUtc { get; protected set; }

    /// <summary>Generate a sortable, time-ordered primary key for a new entity.</summary>
    protected static Guid NewId() => Guid.CreateVersion7();
}
