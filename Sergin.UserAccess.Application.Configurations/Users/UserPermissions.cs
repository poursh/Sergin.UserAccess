using Sergin.SharedKernel.Domain.Securities;

namespace Sergin.UserAccess.Application.Configurations.Users;

/// <summary>The permissions the User aggregate owns. Users belong to no scope, so the command configurations check these on any scope; the reach guard then narrows them to the caller's region.</summary>
public static class UserPermissions
{
    public static PermissionDefinition Read { get; } = PermissionDefinition.Create("permission.ua.users.read", "View users");

    public static PermissionDefinition Manage { get; } = PermissionDefinition.Create("permission.ua.users.manage", "Create and deactivate users");

    /// <summary>Checked against the scope an assignment targets, not globally.</summary>
    public static PermissionDefinition AssignmentsManage { get; } = PermissionDefinition.Create("permission.ua.assignments.manage", "Assign and revoke roles");
}
