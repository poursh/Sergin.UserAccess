using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sergin.UserAccess.Domain.Authorization;

namespace Sergin.UserAccess.Infrastructure.Data.Authorization;

internal sealed class AuthorizationAuditEntryEntityTypeConfiguration : IEntityTypeConfiguration<AuthorizationAuditEntry>
{
    public const int ActionMaxLength = 100;

    public void Configure(EntityTypeBuilder<AuthorizationAuditEntry> builder)
    {
        // Append-only: the AddScopedRbac migration puts a trigger on this table that refuses UPDATE, DELETE and
        // TRUNCATE (NFR-6), so not even a hand-written statement can rewrite history.
        builder.ToTable("authorization_audit_log");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Action).HasMaxLength(ActionMaxLength);
        builder.Property(e => e.EntityType).HasMaxLength(ActionMaxLength);
        builder.Property(e => e.Details).HasColumnType("jsonb");

        builder.HasIndex(e => e.OccurredAtUtc);
        builder.HasIndex(e => e.EntityId);
    }
}
