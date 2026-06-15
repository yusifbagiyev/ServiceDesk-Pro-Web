using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ServiceDesk.Inventory.Application.Contracts;

namespace ServiceDesk.Inventory.Infrastructure;

/// <summary>
/// Typed HttpClient over the external ProductService API. Maps the external JSON shape to the
/// internal <see cref="InventoryItem"/> contract; never leaks the external DTO. Fails soft
/// (returns null/empty + logs) so an inventory outage degrades ticket creation rather than breaking it.
/// </summary>
internal sealed class ProductServiceClient(HttpClient httpClient, ILogger<ProductServiceClient> logger)
    : IInventoryLookup
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<InventoryItem?> GetByInventoryCodeAsync(
        string inventoryCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.GetAsync(
                $"api/products/search/inventory-code/{Uri.EscapeDataString(inventoryCode)}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var product = await response.Content.ReadFromJsonAsync<ProductResponse>(JsonOptions, cancellationToken);
            if (product is null)
            {
                return null;
            }

            return new InventoryItem(
                product.InventoryCode.ToString(CultureInfo.InvariantCulture),
                product.DepartmentName,
                product.Worker,
                product.CategoryName,
                product.Model,
                product.Vendor);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogError(ex, "Inventory lookup failed for code {InventoryCode}", inventoryCode);
            return null;
        }
    }

    public async Task<IReadOnlyList<InventoryDepartment>> GetDepartmentsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.GetAsync("api/departments", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return [];
            }

            var departments = await response.Content
                .ReadFromJsonAsync<List<DepartmentResponse>>(JsonOptions, cancellationToken);

            return departments?
                .Select(department => new InventoryDepartment(department.Id, department.Name))
                .ToList() ?? [];
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogError(ex, "Inventory departments fetch failed");
            return [];
        }
    }

    private sealed record ProductResponse
    {
        public int InventoryCode { get; init; }

        public string? Model { get; init; }

        public string? Vendor { get; init; }

        public string? Worker { get; init; }

        public string? CategoryName { get; init; }

        public string? DepartmentName { get; init; }
    }

    private sealed record DepartmentResponse
    {
        public int Id { get; init; }

        public string Name { get; init; } = string.Empty;
    }
}
