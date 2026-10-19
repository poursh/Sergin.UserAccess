using Sergin.SharedKernel.Domain;
using Sergin.UserAccess.Domain.Roles;
using Sergin.UserAccess.Domain.Scopes;

namespace Sergin.UserAccess.Domain.Users.Assignments;

/// <summary>
/// A user holding a role at a scope (FR-6): over that scope alone or its whole subtree (FR-7), optionally only
/// within a validity period (FR-8). Revoking keeps the row and stamps it (FR-12), so history stays; a revoked or
/// out-of-period assignment grants nothing. A child of <see cref="User"/>: created and revoked through the user.
/// </summary>
public class RoleAssignment : Entity<RoleAssignmentId>
{
    private RoleAssignment() { }

    public RoleId RoleId { get; private set; }

    public ScopeId ScopeId { get; private set; }

    public bool IncludeSubtree { get; private set; }

    public DateTime? ValidFromUtc { get; private set; }

    public DateTime? ValidUntilUtc { get; private set; }

    public DateTime? RevokedAtUtc { get; private set; }

    public Guid? RevokedBy { get; private set; }

    public bool IsRevoked => RevokedAtUtc is not null;

    internal static RoleAssignment Create(RoleId roleId, ScopeId scopeId, bool includeSubtree, ValidityPeriod period)
    {
        return new RoleAssignment
        {
            Id = new RoleAssignmentId(Guid.CreateVersion7()),
            RoleId = roleId,
            ScopeId = scopeId,
            IncludeSubtree = includeSubtree,
            ValidFromUtc = period.FromUtc,
            ValidUntilUtc = period.UntilUtc
        };
    }

    /// <summary>Not revoked, and <paramref name="nowUtc"/> inside the validity period (start inclusive, end exclusive).</summary>
    public bool IsActiveAt(DateTime nowUtc) =>
        !IsRevoked
        && (ValidFromUtc is null || ValidFromUtc <= nowUtc)
        && (ValidUntilUtc is null || nowUtc < ValidUntilUtc);

    internal void Revoke(DateTime nowUtc, Guid revokedBy)
    {
        RevokedAtUtc = nowUtc;
        RevokedBy = revokedBy;
    }
}

public sealed record RoleAssignmentId(Guid Value);

/// <summary>An optional start and end; either may be open. <see cref="Create"/> refuses an end at or before the start.</summary>
public sealed record ValidityPeriod
{
    private ValidityPeriod(DateTime? fromUtc, DateTime? untilUtc)
    {
        FromUtc = fromUtc;
        UntilUtc = untilUtc;
    }

    public static ValidityPeriod Always { get; } = new(null, null);

    public DateTime? FromUtc { get; }

    public DateTime? UntilUtc { get; }

    public static ErrorOr<ValidityPeriod> Create(DateTime? fromUtc, DateTime? untilUtc)
    {
        if (fromUtc is not null && untilUtc is not null && untilUtc <= fromUtc)
        {
            return Error.Validation("ValidUntilUtc", "'Valid Until Utc' must be after 'Valid From Utc'.");
        }

        return new ValidityPeriod(AsUtc(fromUtc), AsUtc(untilUtc));
    }

    // An unspecified kind is taken as already UTC, as the parameter names say; a local time is converted.
    private static DateTime? AsUtc(DateTime? value) => value switch
    {
        null => null,
        { Kind: DateTimeKind.Local } local => local.ToUniversalTime(),
        { } other => DateTime.SpecifyKind(other, DateTimeKind.Utc)
    };
}
