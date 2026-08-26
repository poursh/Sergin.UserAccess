using Sergin.SharedKernel.Domain;

namespace Sergin.UserAccess.Domain.Roles;

/// <summary>
/// A named bundle of permissions. Roles are what Sergin grants; Keycloak grants nothing — it only
/// proves identity, so the whole authorization model lives here.
/// </summary>
public class Role : AggregateRoot<RoleId>
{
    private readonly List<PermissionCode> permissions = [];

    private Role() { }

    /// <summary>
    /// The role a user is given the first time they sign in. Without it a provisioned user would hold
    /// no permissions and see an empty application, with no way to fix it short of editing the database.
    /// </summary>
    public const string DefaultRoleName = "viewer";

    public RoleName Name { get; private set; }

    public IReadOnlyCollection<PermissionCode> Permissions => permissions;

    public static Role Create(RoleName name, IEnumerable<PermissionCode> permissions)
    {
        Role role = new()
        {
            Id = new RoleId(Guid.CreateVersion7()),
            Name = name
        };

        foreach (PermissionCode permission in permissions)
        {
            role.Grant(permission);
        }

        return role;
    }

    public void Grant(PermissionCode permission)
    {
        if (!permissions.Contains(permission))
        {
            permissions.Add(permission);
        }
    }

    public void Revoke(PermissionCode permission) => permissions.Remove(permission);
}

public sealed record RoleId(Guid Value);

public sealed record RoleName(string Value);

/// <summary>
/// A permission string as stored. Deliberately not <c>Sergin.SharedKernel.Application.Securities.Permission</c>:
/// that type lives in the Application layer and a Domain project references only SharedKernel.Domain.
/// The Application layer validates a code through <c>Permission.Create</c> on the way in.
/// </summary>
public sealed record PermissionCode(string Value);
