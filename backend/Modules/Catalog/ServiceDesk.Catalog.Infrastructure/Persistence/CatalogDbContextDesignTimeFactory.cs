using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ServiceDesk.Catalog.Infrastructure.Persistence;

public sealed class CatalogDbContextDesignTimeFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("SERVICEDESK_DB")
            ?? "Host=localhost;Port=5432;Database=servicedesk;Username=servicedesk;Password=servicedesk_dev_pw";

        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsHistoryTable(CatalogDbContext.MigrationsHistoryTableName))
            .UseSnakeCaseNamingConvention()
            .Options;

        return new CatalogDbContext(options);
    }
}
