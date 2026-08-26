using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sergin.UserAccess.Domain.Roles;
using Sergin.UserAccess.Infrastructure.Data.Roles.Converters;

namespace Sergin.UserAccess.Infrastructure.Data.Roles;

internal sealed class RoleEntityTypeConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasConversion<RoleIdConverter>()
            .ValueGeneratedNever();

        builder.Property(r => r.Name)
            .HasConversion<RoleNameConverter>()
            .HasMaxLength(100);

        builder.HasIndex(r => r.Name).IsUnique();

        // Owned rather than a separate aggregate: a permission has no life outside the role granting it.
        builder.OwnsMany(r => r.Permissions, permission =>
        {
            permission.ToTable("role_permissions");
            permission.WithOwner().HasForeignKey("role_id");
            permission.Property(p => p.Value).HasColumnName("permission").HasMaxLength(300);
            permission.HasKey("role_id", "Value");
        });

        builder.Navigation(r => r.Permissions)
            .HasField("permissions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
