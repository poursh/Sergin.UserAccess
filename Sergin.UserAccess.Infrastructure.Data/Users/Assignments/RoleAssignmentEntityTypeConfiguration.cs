using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sergin.UserAccess.Domain.Roles;
using Sergin.UserAccess.Domain.Scopes;
using Sergin.UserAccess.Domain.Users;
using Sergin.UserAccess.Domain.Users.Assignments;
using Sergin.UserAccess.Infrastructure.Data.Roles.Converters;
using Sergin.UserAccess.Infrastructure.Data.Scopes.Converters;
using Sergin.UserAccess.Infrastructure.Data.Users.Assignments.Converters;
using Sergin.UserAccess.Infrastructure.Data.Users.Converters;

namespace Sergin.UserAccess.Infrastructure.Data.Users.Assignments;

internal sealed class RoleAssignmentEntityTypeConfiguration : IEntityTypeConfiguration<RoleAssignment>
{
    public void Configure(EntityTypeBuilder<RoleAssignment> builder)
    {
        builder.ToTable("role_assignments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasConversion<RoleAssignmentIdConverter>()
            .ValueGeneratedNever();

        // The owning user, as a shadow key: the assignment is reached only through User.Assignments, which
        // declares the relationship; declared here too so this configuration can index it.
        builder.Property<UserInternalId>("UserId").HasConversion<UserInternalIdConverter>();

        builder.Property(a => a.RoleId).HasConversion<RoleIdConverter>();
        builder.Property(a => a.ScopeId).HasConversion<ScopeIdConverter>();

        // Restrict on both: roles and scopes are soft-deleted, so these never fire in normal operation; a
        // revoked assignment keeps naming the role and scope it was for.
        builder.HasOne<Role>()
            .WithMany()
            .HasForeignKey(a => a.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Scope>()
            .WithMany()
            .HasForeignKey(a => a.ScopeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(a => a.IsRevoked);

        // The permission check loads one user's live assignments; revoked rows are history it never reads.
        builder.HasIndex("UserId").HasFilter("revoked_at_utc IS NULL");
        builder.HasIndex(a => a.RoleId);
        builder.HasIndex(a => a.ScopeId);
    }
}
