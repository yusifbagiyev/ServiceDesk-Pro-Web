using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using ServiceDesk.Application.Abstractions.Behaviors;
using ServiceDesk.Application.Abstractions.Security;
using ServiceDesk.Application.Abstractions.Time;
using ServiceDesk.SharedInfrastructure.Authentication;
using ServiceDesk.SharedInfrastructure.Authorization;
using ServiceDesk.SharedInfrastructure.Persistence;
using ServiceDesk.SharedInfrastructure.Security;
using ServiceDesk.SharedInfrastructure.Time;
using StackExchange.Redis;

namespace ServiceDesk.SharedInfrastructure;

/// <summary>
/// Registration helpers for the shared infrastructure. Called once from the API composition root,
/// before the module <c>Add{Module}Infrastructure()</c> calls.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Clock + the domain-event SaveChanges interceptor shared by every module DbContext.</summary>
    public static IServiceCollection AddSharedCore(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<DomainEventsInterceptor>();
        return services;
    }

    /// <summary>
    /// Registers the cross-cutting MediatR pipeline behaviors (outer-to-inner: logging, validation).
    /// Transaction behavior is added by its respective phase.
    /// </summary>
    public static IServiceCollection AddCrossCuttingBehaviors(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        return services;
    }

    /// <summary>
    /// BFF authentication + authorization: Redis session store, Argon2id hasher, the opaque
    /// session-cookie scheme, <c>ICurrentUser</c>, and the dynamic permission policy provider.
    /// </summary>
    public static IServiceCollection AddSharedAuth(this IServiceCollection services, string redisConnectionString)
    {
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
        services.AddScoped<ISessionStore, RedisSessionStore>();
        services.AddSingleton<IPasswordHasher, Argon2idPasswordHasher>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services
            .AddAuthentication(SessionConstants.Scheme)
            .AddScheme<AuthenticationSchemeOptions, SessionAuthenticationHandler>(SessionConstants.Scheme, _ => { });

        services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddAuthorization();

        return services;
    }
}
