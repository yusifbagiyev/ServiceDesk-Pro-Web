using ServiceDesk.Catalog.Domain.Events;
using ServiceDesk.Kernel.Domain;

namespace ServiceDesk.Catalog.Domain.Entity;

/// <summary>
/// A ticket issue/problem category (the legacy <c>Tasks</c> catalog, renamed). Referenced by
/// tickets through a join table; the name is unique (case-insensitive via <see cref="NameNormalized"/>).
/// </summary>
public sealed class Category : AggregateRoot
{
    private Category()
    {
        // EF Core materialization.
    }

    private Category(Guid id, string name, DateTime nowUtc)
        : base(id)
    {
        Name = name;
        NameNormalized = NormalizeName(name);
        IsActive = true;
        CreatedAtUtc = nowUtc;
    }

    public string Name { get; private set; } = null!;

    public string NameNormalized { get; private set; } = null!;

    public bool IsActive { get; private set; }

    public static Category Create(string name, DateTime nowUtc)
    {
        var category = new Category(NewId(), name, nowUtc);
        category.RaiseDomainEvent(new CategoryCreatedDomainEvent(category.Id, category.Name, nowUtc));
        return category;
    }

    public void Rename(string name, DateTime nowUtc)
    {
        Name = name;
        NameNormalized = NormalizeName(name);
        Touch(nowUtc);
        RaiseDomainEvent(new CategoryRenamedDomainEvent(Id, name, nowUtc));
    }

    public void SetActive(bool isActive, DateTime nowUtc)
    {
        if (IsActive == isActive)
        {
            return;
        }

        IsActive = isActive;
        Touch(nowUtc);
        RaiseDomainEvent(new CategoryActivationChangedDomainEvent(Id, isActive, nowUtc));
    }

    public static string NormalizeName(string name) => name.Trim().ToLowerInvariant();
}
