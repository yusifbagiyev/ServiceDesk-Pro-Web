using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceDesk.Tickets.Application;

public static class DependencyInjection
{
    /// <summary>Registers the Tickets module's command/query handlers and validators.</summary>
    public static IServiceCollection AddTicketsApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        return services;
    }
}
