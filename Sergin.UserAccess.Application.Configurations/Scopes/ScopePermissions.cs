using Sergin.SharedKernel.Domain.Securities;

namespace Sergin.UserAccess.Application.Configurations.Scopes;

/// <summary>The permissions the Scope aggregate owns, checked against the target scope.</summary>
public static class ScopePermissions
{
    public static PermissionDefinition Read { get; } = PermissionDefinition.Create("permission.ua.scopes.read", "View scopes");

    public static PermissionDefinition Manage { get; } = PermissionDefinition.Create("permission.ua.scopes.manage", "Create, rename, move and delete scopes");
}
