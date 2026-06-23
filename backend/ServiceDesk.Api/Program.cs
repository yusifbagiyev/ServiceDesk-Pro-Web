using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceDesk.Application.Abstractions.Time;
using ServiceDesk.Catalog.Api;
using ServiceDesk.Catalog.Application;
using ServiceDesk.Catalog.Infrastructure;
using ServiceDesk.Catalog.Infrastructure.Persistence;
using ServiceDesk.Identity.Api;
using ServiceDesk.Identity.Application;
using ServiceDesk.Identity.Application.Commands;
using ServiceDesk.Identity.Infrastructure;
using ServiceDesk.Identity.Infrastructure.Persistence;
using ServiceDesk.Inventory.Infrastructure;
using ServiceDesk.SharedInfrastructure;
using ServiceDesk.SharedInfrastructure.Middleware;
using ServiceDesk.Tickets.Api;
using ServiceDesk.Tickets.Application;
using ServiceDesk.Tickets.Application.Abstractions;
using ServiceDesk.Tickets.Domain;
using ServiceDesk.Tickets.Domain.Enums;
using ServiceDesk.Tickets.Infrastructure;
using ServiceDesk.Tickets.Infrastructure.Persistence;

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

builder.Services.AddTicketsApplication();
builder.Services.AddTicketsInfrastructure(postgres);
builder.Services.Configure<AttachmentStorageOptions>(builder.Configuration.GetSection("Attachments"));

// Inventory ACL: resilient typed HttpClient over the external ProductService (autofill on ticket create).
builder.Services.AddInventoryInfrastructure(options =>
{
    options.BaseUrl = builder.Configuration[$"{InventoryOptions.SectionName}:BaseUrl"] ?? options.BaseUrl;
    options.ApiKey = builder.Configuration[$"{InventoryOptions.SectionName}:ApiKey"];
});

builder.Services
    .AddControllers()
    .AddApplicationPart(typeof(IdentityApiAssemblyMarker).Assembly)
    .AddApplicationPart(typeof(CatalogApiAssemblyMarker).Assembly)
    .AddApplicationPart(typeof(TicketsApiAssemblyMarker).Assembly);

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

    var ticketsDbContext = scope.ServiceProvider.GetRequiredService<TicketsDbContext>();
    await ticketsDbContext.Database.MigrateAsync();

    // Seed the priority-driven SLA policies once (response/resolution minutes).
    if (!await ticketsDbContext.SlaPolicies.AnyAsync())
    {
        var now = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>().UtcNow;
        ticketsDbContext.SlaPolicies.AddRange(
            SlaPolicy.Create("Urgent", TicketPriority.Urgent, 15, 240, now),
            SlaPolicy.Create("High", TicketPriority.High, 60, 480, now),
            SlaPolicy.Create("Normal", TicketPriority.Normal, 240, 1440, now),
            SlaPolicy.Create("Low", TicketPriority.Low, 480, 4320, now));
        await ticketsDbContext.SaveChangesAsync();
    }

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
