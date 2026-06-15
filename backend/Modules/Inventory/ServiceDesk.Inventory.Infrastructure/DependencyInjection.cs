using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ServiceDesk.Inventory.Application.Contracts;

namespace ServiceDesk.Inventory.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registers the inventory ACL: a resilient typed HttpClient over the ProductService API.</summary>
    public static IServiceCollection AddInventoryInfrastructure(
        this IServiceCollection services,
        Action<InventoryOptions> configureOptions)
    {
        services.Configure(configureOptions);

        services.AddHttpClient<IInventoryLookup, ProductServiceClient>((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<InventoryOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                if (!string.IsNullOrWhiteSpace(options.ApiKey))
                {
                    client.DefaultRequestHeaders.Add("X-Api-Key", options.ApiKey);
                }
            })
            .AddStandardResilienceHandler();

        return services;
    }
}
