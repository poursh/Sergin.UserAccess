using Sergin.SharedKernel.Domain;
using Sergin.SharedKernel.Domain.Securities;

namespace Sergin.UserAccess.Domain.Roles;

/// <summary>
/// A named bundle of permissions. Roles are what Sergin grants; Keycloak grants nothing — it only
/// proves identity, so the whole authorization model lives here.
/// </summary>
public class Role : AggregateRoot<RoleId>
{
    private readonly List<Permission> permissions = [];

    private Role() { }

    /// <summary>
    /// The role a user is given the first time they sign in. Without it a provisioned user would hold
    /// no permissions and see an empty application, with no way to fix it short of editing the database.
    /// </summary>
    public const string DefaultRoleName = "viewer";

    public RoleName Name { get; private set; }

    public IReadOnlyCollection<Permission> Permissions => permissions;

    public static Role Create(RoleName name, IEnumerable<Permission> permissions)
    {
        Role role = new()
        {
            Id = new RoleId(Guid.CreateVersion7()),
            Name = name
        };

        foreach (Permission permission in permissions)
        {
            role.Grant(permission);
        }

        return role;
    }

    public void Grant(Permission permission)
    {
        if (!permissions.Contains(permission))
        {
            permissions.Add(permission);
        }
    }

    public void Revoke(Permission permission) => permissions.Remove(permission);
}

public sealed record RoleId(Guid Value);

public sealed record RoleName(string Value);
