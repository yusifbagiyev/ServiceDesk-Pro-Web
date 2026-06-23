using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceDesk.Tickets.Domain;

namespace ServiceDesk.Tickets.Infrastructure.Persistence.Configurations;

/// <summary>
/// The single satisfaction rating per ticket: its own Id primary key plus a unique <c>ticket_id</c>
/// foreign key (1:1 with the ticket). The relationship itself is configured on <see cref="TicketConfiguration"/>.
/// </summary>
internal sealed class TicketRatingConfiguration : IEntityTypeConfiguration<TicketRating>
{
    public void Configure(EntityTypeBuilder<TicketRating> builder)
    {
        builder.ToTable("ticket_ratings");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.Message).HasMaxLength(2000);

        builder.HasIndex(r => r.TicketId, "ix_ticket_ratings_ticket").IsUnique();
    }
}
