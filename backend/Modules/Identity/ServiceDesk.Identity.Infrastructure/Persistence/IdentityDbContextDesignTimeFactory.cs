using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ServiceDesk.Identity.Infrastructure.Persistence;

/// <summary>
/// Used by <c>dotnet ef</c> at design time (migrations) without booting the API host.
/// The connection string only needs to be valid for <c>database update</c>; migration
/// generation reads the model only.
/// </summary>
public sealed class IdentityDbContextDesignTimeFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("SERVICEDESK_DB")
            ?? "Host=localhost;Port=5432;Database=servicedesk;Username=servicedesk;Password=servicedesk_dev_pw";

        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsHistoryTable(IdentityDbContext.MigrationsHistoryTableName))
            .UseSnakeCaseNamingConvention()
            .Options;

        return new IdentityDbContext(options);
    }
}
