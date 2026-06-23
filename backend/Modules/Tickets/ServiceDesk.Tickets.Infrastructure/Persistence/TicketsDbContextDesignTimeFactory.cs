using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ServiceDesk.Tickets.Infrastructure.Persistence;

public sealed class TicketsDbContextDesignTimeFactory : IDesignTimeDbContextFactory<TicketsDbContext>
{
    public TicketsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("SERVICEDESK_DB")
            ?? "Host=localhost;Port=5432;Database=servicedesk;Username=servicedesk;Password=servicedesk_dev_pw";

        var options = new DbContextOptionsBuilder<TicketsDbContext>()
            .UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsHistoryTable(TicketsDbContext.MigrationsHistoryTableName))
            .UseSnakeCaseNamingConvention()
            .Options;

        return new TicketsDbContext(options);
    }
}
