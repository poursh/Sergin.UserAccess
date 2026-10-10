using Sergin.SharedKernel.Domain;
using Sergin.UserAccess.Domain.Roles;
using Sergin.UserAccess.Domain.Scopes;
using Sergin.UserAccess.Domain.Users.Assignments;

namespace Sergin.UserAccess.Domain.Users;

public class User : AggregateRoot<UserInternalId>
{
    private readonly List<RoleAssignment> assignments = [];

    private User() { }

    public UserName UserName { get; private set; }
    public bool IsActive { get; private set; }

    /// <summary>
    /// The identity provider's subject for this user, or null for a user created before external sign-in
    /// existed — or by <see cref="Create"/>, which has no provider to take one from.
    /// </summary>
    public ExternalUserId? ExternalId { get; private set; }

    public EmailAddress? Email { get; private set; }

    /// <summary>
    /// Column width of <see cref="FirstName"/> and <see cref="LastName"/>; what the provisioning validator
    /// and the EF configuration both read, so the limit is stated once.
    /// </summary>
    public const int NameMaxLength = 200;

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public IReadOnlyCollection<RoleAssignment> Assignments => assignments;

    public static User Create(UserName userName)
    {
        return new User
        {
            Id = new UserInternalId(Guid.CreateVersion7()),
            UserName = userName,
            IsActive = true
        };
    }

    /// <summary>
    /// Creates the local record for a user the identity provider just authenticated for the first time.
    /// </summary>
    public static User CreateFromExternalIdentity(
        ExternalUserId externalId, UserName userName, EmailAddress? email, string firstName, string lastName)
    {
        return new User
        {
            Id = new UserInternalId(Guid.CreateVersion7()),
            UserName = userName,
            IsActive = true,
            ExternalId = externalId,
            Email = email,
            FirstName = firstName,
            LastName = lastName
        };
    }

    /// <summary>
    /// Creates the configured development user under the id the host's configuration gives it, so the user the
    /// development host signs everyone in as is the user its grants are stored for.
    /// </summary>
    public static User CreateDevelopmentUser(
        UserInternalId id, UserName userName, EmailAddress? email, string firstName, string lastName)
    {
        return new User
        {
            Id = id,
            UserName = userName,
            IsActive = true,
            Email = email,
            FirstName = firstName,
            LastName = lastName
        };
    }

    /// <summary>Links a pre-existing local user to the provider subject that now authenticates them.</summary>
    public void LinkExternalId(ExternalUserId externalId) => ExternalId = externalId;

    /// <summary>
    /// Refreshes the profile fields from the provider on each sign-in, so a rename in Keycloak shows up
    /// here rather than leaving the local copy permanently stale.
    /// </summary>
    public void UpdateProfile(EmailAddress? email, string firstName, string lastName)
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
    }

    /// <summary>
    /// Assigns <paramref name="role"/> at <paramref name="scope"/> (FR-6). Refuses a scope the role cannot reach
    /// (BR-2: the owner scope itself, or below it when the role is inheritable — <paramref name="ownerPath"/> is
    /// the role's owner scope path) and an assignment that is already active for the same role and scope.
    /// </summary>
    public ErrorOr<RoleAssignment> AssignRole(
        Role role, ScopePath ownerPath, Scope scope, bool includeSubtree, ValidityPeriod period, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(role);
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentNullException.ThrowIfNull(period);

        if (!role.IsAssignableAt(ownerPath, scope.Path))
        {
            return Error.Validation(
                "ScopeId", "The role can only be assigned at its owner scope, or below it when the role is inheritable.");
        }

        if (assignments.Exists(existing => existing.RoleId == role.Id && existing.ScopeId == scope.Id && !existing.IsRevoked
            && (existing.ValidUntilUtc is null || nowUtc < existing.ValidUntilUtc)))
        {
            return Error.Validation("RoleId", "The user already holds this role at this scope.");
        }

        var assignment = RoleAssignment.Create(role.Id, scope.Id, includeSubtree, period);
        assignments.Add(assignment);

        return assignment;
    }

    /// <summary>
    /// Gives a newly provisioned user a root-owned role at the root scope over the whole tree — a global grant.
    /// Only for roles owned by the root, which every scope is below, so the BR-2 check needs no paths.
    /// </summary>
    public void AssignGlobalRole(Role role)
    {
        ArgumentNullException.ThrowIfNull(role);

        if (role.OwnerScopeId != Scope.RootId)
        {
            throw new ArgumentException("Only a role owned by the root scope can be assigned globally.", nameof(role));
        }

        if (assignments.Exists(existing => existing.RoleId == role.Id && existing.ScopeId == Scope.RootId && !existing.IsRevoked))
        {
            return;
        }

        var assignment = RoleAssignment.Create(role.Id, Scope.RootId, includeSubtree: true, ValidityPeriod.Always);
        assignments.Add(assignment);
    }

    /// <summary>Revokes an assignment now (FR-12). The row stays, stamped; revoking twice is not found.</summary>
    public ErrorOr<Success> RevokeAssignment(RoleAssignmentId assignmentId, DateTime nowUtc, Guid revokedBy)
    {
        RoleAssignment? assignment = assignments.Find(candidate => candidate.Id == assignmentId);

        if (assignment is null || assignment.IsRevoked)
        {
            return Error.NotFound();
        }

        assignment.Revoke(nowUtc, revokedBy);

        return Result.Success;
    }

    /// <summary>Disables every check for this user (FR-13). Assignments are kept, so reactivating restores them.</summary>
    public void Deactivate() => IsActive = false;
}

public sealed record UserInternalId(Guid Value);

// MaxLength constants: the one number a command validator, a Blazor form model's [StringLength] and an
// EF HasMaxLength all read, so a limit is never repeated as a literal in three places.
public sealed record UserName(string Value)
{
    public const int MaxLength = 100;
}

/// <summary>The identity provider's stable subject identifier — Keycloak's <c>sub</c>.</summary>
public sealed record ExternalUserId(string Value)
{
    public const int MaxLength = 200;
}

public sealed record EmailAddress(string Value)
{
    public const int MaxLength = 320;
}
