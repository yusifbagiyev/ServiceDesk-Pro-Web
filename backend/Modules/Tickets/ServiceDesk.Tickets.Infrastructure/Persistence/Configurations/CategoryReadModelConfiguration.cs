using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ServiceDesk.Tickets.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps the read-only <see cref="CategoryReadModel"/> onto the Catalog-owned <c>categories</c> table.
/// ExcludeFromMigrations: Tickets reads this table but never creates or alters it.
/// </summary>
internal sealed class CategoryReadModelConfiguration : IEntityTypeConfiguration<CategoryReadModel>
{
    public void Configure(EntityTypeBuilder<CategoryReadModel> builder)
    {
        builder.ToTable("categories", t => t.ExcludeFromMigrations());
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();
        builder.Property(c => c.Name).HasColumnName("name");
        builder.Property(c => c.IsActive).HasColumnName("is_active");
    }
}
