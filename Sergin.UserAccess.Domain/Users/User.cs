using Sergin.SharedKernel.Domain;
using Sergin.UserAccess.Domain.Roles;

namespace Sergin.UserAccess.Domain.Users;

public class User : AggregateRoot<UserInternalId>
{
    private readonly List<UserRole> roles = [];

    private User() { }

    public UserName UserName { get; private set; }
    public bool IsActive { get; private set; }

    /// <summary>
    /// The identity provider's subject for this user, or null for a user created before external sign-in
    /// existed — or by <see cref="Create"/>, which has no provider to take one from.
    /// </summary>
    public ExternalUserId? ExternalId { get; private set; }

    public EmailAddress? Email { get; private set; }

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public IReadOnlyCollection<UserRole> Roles => roles;

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

    public void AssignRole(RoleId roleId)
    {
        if (!roles.Any(role => role.RoleId == roleId))
        {
            roles.Add(new UserRole(roleId));
        }
    }

    public void RevokeRole(RoleId roleId) => roles.RemoveAll(role => role.RoleId == roleId);

    public void Deactivate()
    {
        IsActive = false;
    }
}

public sealed record UserInternalId(Guid Value);
public sealed record UserName(string Value);

/// <summary>The identity provider's stable subject identifier — Keycloak's <c>sub</c>.</summary>
public sealed record ExternalUserId(string Value);

public sealed record EmailAddress(string Value);

/// <summary>One row of the user-to-role assignment, owned by the user.</summary>
public sealed record UserRole(RoleId RoleId);
