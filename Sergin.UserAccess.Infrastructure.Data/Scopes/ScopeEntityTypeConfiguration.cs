using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sergin.UserAccess.Domain.Scopes;
using Sergin.UserAccess.Infrastructure.Data.Scopes.Converters;

namespace Sergin.UserAccess.Infrastructure.Data.Scopes;

internal sealed class ScopeEntityTypeConfiguration : IEntityTypeConfiguration<Scope>
{
    public void Configure(EntityTypeBuilder<Scope> builder)
    {
        builder.ToTable("scopes");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasConversion<ScopeIdConverter>()
            .ValueGeneratedNever();

        builder.Property(s => s.Name)
            .HasConversion<ScopeNameConverter>()
            .HasMaxLength(ScopeName.MaxLength);

        builder.Property(s => s.ParentId)
            .HasConversion<ScopeIdConverter>();

        // ClientNoAction: a scope is soft-deleted, never removed, so EF must not fix up tracked children when one
        // is Removed (the interceptor turns that into an UPDATE). The database key still stops a raw DELETE from
        // orphaning a subtree.
        builder.HasOne<Scope>()
            .WithMany()
            .HasForeignKey(s => s.ParentId)
            .OnDelete(DeleteBehavior.ClientNoAction);

        // ltree: the materialised path every ancestor/descendant question is answered from with @> and <@.
        // GiST is the index those operators use.
        builder.Property(s => s.Path)
            .HasConversion<ScopePathConverter>()
            .HasColumnType("ltree")
            .IsRequired();

        builder.HasIndex(s => s.Path).HasMethod("gist");

        // Sibling names are unique, so a breadcrumb or tree view never shows two identical children.
        builder.HasIndex(s => new { s.ParentId, s.Name }).IsUnique();

        builder.Ignore(s => s.IsRoot);
    }
}
