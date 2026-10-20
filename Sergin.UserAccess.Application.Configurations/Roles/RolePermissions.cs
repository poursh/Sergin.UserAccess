using Sergin.SharedKernel.Domain.Securities;

namespace Sergin.UserAccess.Application.Configurations.Roles;

/// <summary>The permissions the Role aggregate owns, checked against the role's owner scope.</summary>
public static class RolePermissions
{
    public static PermissionDefinition Read { get; } = PermissionDefinition.Create("permission.ua.roles.read", "View roles");

    public static PermissionDefinition Manage { get; } = PermissionDefinition.Create("permission.ua.roles.manage", "Create and edit roles");
}
