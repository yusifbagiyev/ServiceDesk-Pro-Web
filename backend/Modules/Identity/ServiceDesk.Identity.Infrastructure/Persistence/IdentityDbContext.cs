using Microsoft.EntityFrameworkCore;
using ServiceDesk.Identity.Application.Abstractions;
using ServiceDesk.Identity.Domain.Entity;

namespace ServiceDesk.Identity.Infrastructure.Persistence;

/// <summary>
/// The Identity module's DbContext. All modules share one database schema (the default),
/// so cross-module reads can map the same tables directly; each module keeps its own
/// migration-history table so their migrations do not collide.
/// </summary>
public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : DbContext(options), IIdentityUnitOfWork
{
    public const string MigrationsHistoryTableName = "__ef_migrations_history_identity";

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
