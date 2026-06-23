using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ServiceDesk.Tickets.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps the read-only <see cref="UserReadModel"/> onto the Identity-owned <c>users</c> table.
/// ExcludeFromMigrations: Tickets reads this table but never creates or alters it.
/// </summary>
internal sealed class UserReadModelConfiguration : IEntityTypeConfiguration<UserReadModel>
{
    public void Configure(EntityTypeBuilder<UserReadModel> builder)
    {
        builder.ToTable("users", t => t.ExcludeFromMigrations());
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();
        builder.Property(u => u.FullName).HasColumnName("full_name");
        builder.Property(u => u.Email).HasColumnName("email");
        builder.Property(u => u.IsActive).HasColumnName("is_active");
    }
}
