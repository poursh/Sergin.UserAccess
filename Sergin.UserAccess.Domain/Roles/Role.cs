using Sergin.SharedKernel.Domain;
using Sergin.SharedKernel.Domain.Securities;
using Sergin.UserAccess.Domain.Scopes;

namespace Sergin.UserAccess.Domain.Roles;

/// <summary>
/// A named bundle of permissions, owned by exactly one scope. Roles are what Sergin grants; Keycloak grants
/// nothing — it only proves identity, so the whole authorization model lives here. A role can be assigned at
/// its owner scope and, when <see cref="IsInheritable"/>, at any scope below it (BR-2, BR-3); a root-owned
/// inheritable role is therefore a global role (BR-7).
/// </summary>
public class Role : AggregateRoot<RoleId>
{
    private readonly List<Permission> permissions = [];

    private Role() { }

    /// <summary>
    /// The role a user is given the first time they sign in, at the root scope over the whole tree. Without it
    /// a provisioned user would hold no permissions and see an empty application.
    /// </summary>
    public const string DefaultRoleName = "viewer";

    /// <summary>The role's code: unique within its owner scope, and across that scope's ancestors and descendants (BR-4).</summary>
    public RoleName Name { get; private set; }

    public ScopeId OwnerScopeId { get; private set; }

    public bool IsInheritable { get; private set; }

    public IReadOnlyCollection<Permission> Permissions => permissions;

    public static Role Create(ScopeId ownerScopeId, RoleName name, bool isInheritable, IEnumerable<Permission> permissions)
    {
        ArgumentNullException.ThrowIfNull(permissions);

        Role role = new()
        {
            Id = new RoleId(Guid.CreateVersion7()),
            OwnerScopeId = ownerScopeId,
            Name = name,
            IsInheritable = isInheritable
        };

        role.permissions.AddRange(permissions.DistinctBy(permission => permission.Value));

        return role;
    }

    /// <summary>
    /// True when this role can be seen and assigned at <paramref name="target"/>, given its owner scope's path
    /// (BR-2, BR-3): at the owner itself, or below it when inheritable.
    /// </summary>
    public bool IsAssignableAt(ScopePath ownerPath, ScopePath target)
    {
        ArgumentNullException.ThrowIfNull(ownerPath);
        ArgumentNullException.ThrowIfNull(target);

        return ownerPath == target || IsInheritable && ownerPath.IsAncestorOrSelfOf(target);
    }

    public void Rename(RoleName name)
    {
        Name = name;
    }

    public void SetInheritable(bool isInheritable)
    {
        if (IsInheritable == isInheritable)
        {
            return;
        }

        IsInheritable = isInheritable;
    }

    public void Grant(Permission permission)
    {
        ArgumentNullException.ThrowIfNull(permission);

        if (!permissions.Contains(permission))
        {
            permissions.Add(permission);
        }
    }

    public void Revoke(Permission permission)
    {
        ArgumentNullException.ThrowIfNull(permission);

        permissions.Remove(permission);
    }
}

public sealed record RoleId(Guid Value);

public sealed record RoleName(string Value)
{
    public const int MaxLength = 100;
}
