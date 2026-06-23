using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceDesk.Tickets.Domain;

namespace ServiceDesk.Tickets.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps the <see cref="Ticket"/> aggregate. Child collections and the rating are modelled as OWNED
/// types (no independent lifecycle): they live in their own tables, load with the aggregate, and are
/// written through the private backing fields (field access). Cross-module ids (assignee/category)
/// are plain Guids with no FK.
/// </summary>
internal sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("tickets");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.InventoryCode).HasMaxLength(64);
        builder.Property(t => t.DepartmentName).HasMaxLength(200);
        builder.Property(t => t.Worker).HasMaxLength(200);
        builder.Property(t => t.DeviceName).HasMaxLength(200);
        builder.Property(t => t.Title).HasMaxLength(300);
        builder.Property(t => t.Solution).HasMaxLength(4000);

        builder.Property(t => t.Status).HasConversion<int>();
        builder.Property(t => t.Priority).HasConversion<int>();

        builder.HasIndex(t => t.Status, "ix_tickets_status");
        builder.HasIndex(t => t.Priority, "ix_tickets_priority");
        builder.HasIndex(t => t.CreatedByUserId, "ix_tickets_created_by");
        builder.HasIndex(t => t.ReporterUserId, "ix_tickets_reporter");
        builder.HasIndex(t => t.CreatedAtUtc, "ix_tickets_created_at").IsDescending();
        builder.HasIndex(t => t.LegacyId, "ix_tickets_legacy_id")
            .IsUnique()
            .HasFilter("legacy_id IS NOT NULL");

        // Soft delete: deleted tickets are excluded from every query unless IgnoreQueryFilters is used.
        builder.HasQueryFilter(t => !t.IsDeleted);

        builder.Ignore(t => t.DomainEvents);

        builder.OwnsMany(t => t.Assignees, owned =>
        {
            owned.ToTable("ticket_assignees");
            owned.WithOwner().HasForeignKey(a => a.TicketId);
            owned.HasKey(a => a.Id);
            owned.Property(a => a.Id).ValueGeneratedNever();
            owned.Property(a => a.FullNameSnapshot).HasMaxLength(200).IsRequired();
            owned.HasIndex(a => new { a.TicketId, a.AssigneeUserId }, "ix_ticket_assignees_ticket_user").IsUnique();
        });
        builder.Navigation(t => t.Assignees).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(t => t.Categories, owned =>
        {
            owned.ToTable("ticket_categories");
            owned.WithOwner().HasForeignKey(c => c.TicketId);
            owned.HasKey(c => c.Id);
            owned.Property(c => c.Id).ValueGeneratedNever();
            owned.Property(c => c.NameSnapshot).HasMaxLength(200).IsRequired();
            owned.HasIndex(c => new { c.TicketId, c.CategoryId }, "ix_ticket_categories_ticket_category").IsUnique();
        });
        builder.Navigation(t => t.Categories).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(t => t.StatusHistory, owned =>
        {
            owned.ToTable("ticket_status_history");
            owned.WithOwner().HasForeignKey(h => h.TicketId);
            owned.HasKey(h => h.Id);
            owned.Property(h => h.Id).ValueGeneratedNever();
            owned.Property(h => h.FromStatus).HasConversion<int>();
            owned.Property(h => h.ToStatus).HasConversion<int>();
            owned.Property(h => h.Note).HasMaxLength(1000);
            owned.HasIndex(h => h.TicketId, "ix_ticket_status_history_ticket");
        });
        builder.Navigation(t => t.StatusHistory).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(t => t.Comments, owned =>
        {
            owned.ToTable("ticket_comments");
            owned.WithOwner().HasForeignKey(c => c.TicketId);
            owned.HasKey(c => c.Id);
            owned.Property(c => c.Id).ValueGeneratedNever();
            owned.Property(c => c.AuthorFullName).HasMaxLength(200).IsRequired();
            owned.Property(c => c.Body).HasMaxLength(4000).IsRequired();
            owned.Property(c => c.Visibility).HasConversion<int>();
            owned.HasIndex(c => c.TicketId, "ix_ticket_comments_ticket");
        });
        builder.Navigation(t => t.Comments).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(t => t.Attachments, owned =>
        {
            owned.ToTable("ticket_attachments");
            owned.WithOwner().HasForeignKey(a => a.TicketId);
            owned.HasKey(a => a.Id);
            owned.Property(a => a.Id).ValueGeneratedNever();
            owned.Property(a => a.FileName).HasMaxLength(260).IsRequired();
            owned.Property(a => a.ContentType).HasMaxLength(200).IsRequired();
            owned.Property(a => a.StorageKey).HasMaxLength(1024).IsRequired();
            owned.HasIndex(a => a.TicketId, "ix_ticket_attachments_ticket");
        });
        builder.Navigation(t => t.Attachments).UsePropertyAccessMode(PropertyAccessMode.Field);

        // Rating is a 1:1 child entity (its own Id PK + a unique ticket_id FK). Modelled as a regular
        // relationship rather than OwnsOne to avoid an owned-1:1 shadow-key clash with its inherited Id.
        builder.HasOne(t => t.Rating)
            .WithOne()
            .HasForeignKey<TicketRating>(r => r.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(t => t.Rating).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
