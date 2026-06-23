using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceDesk.SharedInfrastructure.Persistence;
using ServiceDesk.Tickets.Application.Abstractions;
using ServiceDesk.Tickets.Infrastructure.Persistence;
using ServiceDesk.Tickets.Infrastructure.Storage;

namespace ServiceDesk.Tickets.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registers the Tickets DbContext (shared schema + domain-event interceptor) and its services.</summary>
    public static IServiceCollection AddTicketsInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<TicketsDbContext>((serviceProvider, options) =>
        {
            options
                .UseNpgsql(
                    connectionString,
                    npgsql => npgsql.MigrationsHistoryTable(TicketsDbContext.MigrationsHistoryTableName))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(serviceProvider.GetRequiredService<DomainEventsInterceptor>());
        });

        services.AddScoped<ITicketsUnitOfWork>(serviceProvider =>
            serviceProvider.GetRequiredService<TicketsDbContext>());

        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<ISlaPolicyRepository, SlaPolicyRepository>();
        services.AddScoped<ITicketReadRepository, TicketReadRepository>();
        services.AddScoped<ITicketDirectoryReader, TicketDirectoryReader>();
        services.AddSingleton<IFileStorage, LocalFileStorage>();

        return services;
    }
}
