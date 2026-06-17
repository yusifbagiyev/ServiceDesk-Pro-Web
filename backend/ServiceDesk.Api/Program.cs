using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceDesk.Catalog.Api;
using ServiceDesk.Catalog.Application;
using ServiceDesk.Catalog.Infrastructure;
using ServiceDesk.Catalog.Infrastructure.Persistence;
using ServiceDesk.Identity.Api;
using ServiceDesk.Identity.Application;
using ServiceDesk.Identity.Application.Commands;
using ServiceDesk.Identity.Infrastructure;
using ServiceDesk.Identity.Infrastructure.Persistence;
using ServiceDesk.SharedInfrastructure;
using ServiceDesk.SharedInfrastructure.Middleware;

var builder = WebApplication.CreateBuilder(args);

var postgres = builder.Configuration.GetConnectionString("ServiceDeskDb")
    ?? throw new InvalidOperationException("Missing connection string 'ServiceDeskDb'.");
var redis = builder.Configuration.GetConnectionString("Redis")
    ?? throw new InvalidOperationException("Missing connection string 'Redis'.");

// Shared infrastructure: clock + domain-event interceptor, CQRS behaviors, BFF auth.
builder.Services.AddSharedCore();
builder.Services.AddCrossCuttingBehaviors();
builder.Services.AddSharedAuth(redis);

// Modules.
builder.Services.AddIdentityApplication();
builder.Services.AddIdentityInfrastructure(postgres);

builder.Services.AddCatalogApplication();
builder.Services.AddCatalogInfrastructure(postgres);

builder.Services
    .AddControllers()
    .AddApplicationPart(typeof(IdentityApiAssemblyMarker).Assembly)
    .AddApplicationPart(typeof(CatalogApiAssemblyMarker).Assembly);

// CSRF: trusted browser origins for cookie-authenticated state-changing requests.
var csrfProtectionOptions = new CsrfProtectionOptions();
foreach (var origin in builder.Configuration.GetSection("Security:AllowedOrigins").Get<string[]>() ?? [])
{
    csrfProtectionOptions.AllowedOrigins.Add(origin);
}

// CSRF fails closed on an empty allowlist; outside Development that would 403 every cookie request,
// so fail loud at boot (like the connection-string checks) instead of mysteriously at runtime.
if (!builder.Environment.IsDevelopment() && csrfProtectionOptions.AllowedOrigins.Count == 0)
{
    throw new InvalidOperationException(
        "Security:AllowedOrigins must be configured outside Development; CSRF protection fails closed on an empty allowlist.");
}

builder.Services.AddSingleton(csrfProtectionOptions);

var app = builder.Build();

// Apply each module's migrations at startup, and seed a dev admin if there are no users yet.
await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await dbContext.Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<CatalogDbContext>().Database.MigrateAsync();

    if (app.Environment.IsDevelopment() && !await dbContext.Users.AnyAsync())
    {
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        await sender.Send(new CreateUserCommand("admin@servicedesk.local", "System Admin", "Admin123!", "Admin", null));
    }
}

app.UseAuthentication();
app.UseCsrfProtection();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

await app.RunAsync();
