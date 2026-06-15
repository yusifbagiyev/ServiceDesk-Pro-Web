using Microsoft.EntityFrameworkCore;
using ServiceDesk.Catalog.Application.Abstractions;
using ServiceDesk.Catalog.Domain;

namespace ServiceDesk.Catalog.Infrastructure.Persistence;

/// <summary>
/// The Catalog module's DbContext. Shares the default database schema with the other modules
/// so cross-module reads can map the same tables directly; keeps its own migration-history
/// table to avoid migration collisions.
/// </summary>
public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options)
    : DbContext(options), ICatalogUnitOfWork
{
    public const string MigrationsHistoryTableName = "__ef_migrations_history_catalog";

    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
