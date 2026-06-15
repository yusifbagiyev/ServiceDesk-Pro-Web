namespace ServiceDesk.Inventory.Infrastructure;

/// <summary>Configuration for the external ProductService inventory API (bound from "Inventory").</summary>
public sealed class InventoryOptions
{
    public const string SectionName = "Inventory";

    public string BaseUrl { get; set; } = "http://inventory166.az:5001/";

    public string? ApiKey { get; set; }
}
