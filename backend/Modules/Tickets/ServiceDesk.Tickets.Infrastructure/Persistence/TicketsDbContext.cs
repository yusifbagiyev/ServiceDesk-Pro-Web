using Microsoft.EntityFrameworkCore;
using ServiceDesk.Tickets.Application.Abstractions;
using ServiceDesk.Tickets.Domain;

namespace ServiceDesk.Tickets.Infrastructure.Persistence;

/// <summary>
/// The Tickets module's DbContext. Shares the default database schema with the other modules so
/// cross-module reads (users, categories) can map those tables read-only; keeps its own
/// migration-history table so its migrations do not collide.
/// </summary>
public sealed class TicketsDbContext(DbContextOptions<TicketsDbContext> options)
    : DbContext(options), ITicketsUnitOfWork
{
    public const string MigrationsHistoryTableName = "__ef_migrations_history_tickets";

    public DbSet<Ticket> Tickets => Set<Ticket>();

    public DbSet<SlaPolicy> SlaPolicies => Set<SlaPolicy>();

    /// <summary>Read-only cross-module mapping of the Identity users table (ExcludeFromMigrations).</summary>
    internal DbSet<UserReadModel> Users => Set<UserReadModel>();

    /// <summary>Read-only cross-module mapping of the Catalog categories table (ExcludeFromMigrations).</summary>
    internal DbSet<CategoryReadModel> Categories => Set<CategoryReadModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TicketsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
