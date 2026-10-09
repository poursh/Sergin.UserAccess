using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sergin.UserAccess.Domain.Roles;
using Sergin.UserAccess.Domain.Scopes;
using Sergin.UserAccess.Infrastructure.Data.Roles.Converters;
using Sergin.UserAccess.Infrastructure.Data.Scopes.Converters;

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
            .HasMaxLength(RoleName.MaxLength);

        builder.Property(r => r.OwnerScopeId)
            .HasConversion<ScopeIdConverter>()
            .IsRequired();

        builder.HasOne<Scope>()
            .WithMany()
            .HasForeignKey(r => r.OwnerScopeId)
            .OnDelete(DeleteBehavior.Restrict);

        // FR-3: a role code is unique within its owner scope. BR-4 (no repeat across ancestors and descendants)
        // cannot be an index; the create and rename validators check it, advisory like every repository rule.
        builder.HasIndex(r => new { r.OwnerScopeId, r.Name }).IsUnique();

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
