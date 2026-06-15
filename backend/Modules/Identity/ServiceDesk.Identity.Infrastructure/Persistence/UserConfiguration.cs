using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceDesk.Identity.Domain.Entity;

namespace ServiceDesk.Identity.Infrastructure.Persistence;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).ValueGeneratedNever();

        builder.Property(user => user.Email).IsRequired().HasMaxLength(256);
        builder.Property(user => user.FullName).IsRequired().HasMaxLength(200);
        builder.Property(user => user.PasswordHash).IsRequired().HasMaxLength(512);
        builder.Property(user => user.Role).HasConversion<int>();
        builder.Property(user => user.PhoneNumber).HasMaxLength(32);
        builder.Property(user => user.WhatsAppOptIn).HasColumnName("whatsapp_opt_in");

        builder.HasIndex(user => user.Email, "ix_users_email").IsUnique();
        builder.HasIndex(user => user.Role, "ix_users_role");
        builder.HasIndex(user => user.Csat, "ix_users_csat").IsDescending();

        builder.Ignore(user => user.DomainEvents);
    }
}
