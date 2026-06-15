namespace ServiceDesk.Inventory.Application.Contracts;

/// <summary>
/// Anti-corruption gateway to the external ProductService inventory API. The external DTOs never
/// cross this boundary; callers (the Tickets module) see only these internal records.
/// </summary>
public interface IInventoryLookup
{
    /// <summary>Resolve an inventory code to its current snapshot, or null if unknown/unreachable.</summary>
    Task<InventoryItem?> GetByInventoryCodeAsync(string inventoryCode, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<InventoryDepartment>> GetDepartmentsAsync(CancellationToken cancellationToken = default);
}

/// <summary>The fields a ticket snapshots when it is opened against an inventory item.</summary>
public sealed record InventoryItem(
    string InventoryCode,
    string? DepartmentName,
    string? Worker,
    string? DeviceName,
    string? Model,
    string? Vendor);

public sealed record InventoryDepartment(int Id, string Name);
