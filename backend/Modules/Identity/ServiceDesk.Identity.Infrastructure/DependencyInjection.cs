using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceDesk.Application.Abstractions.Messaging;
using ServiceDesk.Identity.Application.Abstractions;
using ServiceDesk.Identity.Domain.Events;
using ServiceDesk.Identity.Infrastructure.Persistence;
using ServiceDesk.SharedInfrastructure.Persistence;

namespace ServiceDesk.Identity.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registers the Identity DbContext (shared schema + domain-event interceptor) and its services.</summary>
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<IdentityDbContext>((serviceProvider, options) =>
        {
            options
                .UseNpgsql(
                    connectionString,
                    npgsql => npgsql.MigrationsHistoryTable(IdentityDbContext.MigrationsHistoryTableName))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(serviceProvider.GetRequiredService<DomainEventsInterceptor>());
        });

        services.AddScoped<IIdentityUnitOfWork>(serviceProvider =>
            serviceProvider.GetRequiredService<IdentityDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();

        // Revoke a user's sessions when their role changes, their email (login handle) changes,
        // or they are deactivated.
        services
            .AddScoped<INotificationHandler<DomainEventNotification<UserRoleChangedDomainEvent>>, SessionRevocationHandler>();
        services
            .AddScoped<INotificationHandler<DomainEventNotification<UserDeactivatedDomainEvent>>, SessionRevocationHandler>();
        services
            .AddScoped<INotificationHandler<DomainEventNotification<UserEmailChangedDomainEvent>>, SessionRevocationHandler>();

        return services;
    }
}
