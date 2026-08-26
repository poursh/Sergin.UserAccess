using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sergin.UserAccess.Domain.Users;
using Sergin.UserAccess.Infrastructure.Data.Roles.Converters;
using Sergin.UserAccess.Infrastructure.Data.Users.Converters;

namespace Sergin.UserAccess.Infrastructure.Data.Users;

internal sealed class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasConversion<UserInternalIdConverter>()
            .ValueGeneratedNever();

        builder.Property(u => u.UserName)
            .HasConversion<UserNameConverter>();

        // Nullable: rows created before external sign-in existed have no subject, and User.Create still
        // makes one without a provider. Unique, so two Keycloak users cannot collapse onto one account.
        builder.Property(u => u.ExternalId)
            .HasConversion<ExternalUserIdConverter>()
            .HasMaxLength(200);

        builder.HasIndex(u => u.ExternalId).IsUnique();

        builder.Property(u => u.Email)
            .HasConversion<EmailAddressConverter>()
            .HasMaxLength(320);

        builder.Property(u => u.FirstName).HasMaxLength(200);
        builder.Property(u => u.LastName).HasMaxLength(200);

        builder.OwnsMany(u => u.Roles, role =>
        {
            role.ToTable("user_roles");
            role.WithOwner().HasForeignKey("user_id");
            role.Property(r => r.RoleId).HasConversion<RoleIdConverter>().HasColumnName("role_id");
            role.HasKey("user_id", "RoleId");
        });

        builder.Navigation(u => u.Roles)
            .HasField("roles")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
