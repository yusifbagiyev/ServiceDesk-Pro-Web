using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceDesk.Catalog.Application.Abstractions;
using ServiceDesk.Catalog.Infrastructure.Persistence;
using ServiceDesk.SharedInfrastructure.Persistence;

namespace ServiceDesk.Catalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<CatalogDbContext>((serviceProvider, options) =>
        {
            options
                .UseNpgsql(
                    connectionString,
                    npgsql => npgsql.MigrationsHistoryTable(CatalogDbContext.MigrationsHistoryTableName))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(serviceProvider.GetRequiredService<DomainEventsInterceptor>());
        });

        services.AddScoped<ICatalogUnitOfWork>(serviceProvider =>
            serviceProvider.GetRequiredService<CatalogDbContext>());
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        return services;
    }
}
