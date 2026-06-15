using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceDesk.Catalog.Domain;

namespace ServiceDesk.Catalog.Infrastructure.Persistence;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");

        builder.HasKey(category => category.Id);
        builder.Property(category => category.Id).ValueGeneratedNever();

        builder.Property(category => category.Name).IsRequired().HasMaxLength(200);
        builder.Property(category => category.NameNormalized).IsRequired().HasMaxLength(200);

        builder.HasIndex(category => category.NameNormalized, "ix_categories_name_normalized").IsUnique();

        builder.Ignore(category => category.DomainEvents);
    }
}
