namespace Sergin.UserAccess.Application.Contracts;

/// <summary>
/// The permissions this module checks, named once. <c>read</c> and <c>manage</c> on scopes, roles and assignments
/// are checked against a target scope (a regional administrator holds them only in their region); the
/// <c>users</c> ones are global, through the command configurations.
/// </summary>
public static class UserAccessPermissions
{
    public const string UsersRead = "permission.ua.users.read";
    public const string UsersManage = "permission.ua.users.manage";
    public const string ScopesRead = "permission.ua.scopes.read";
    public const string ScopesManage = "permission.ua.scopes.manage";
    public const string RolesRead = "permission.ua.roles.read";
    public const string RolesManage = "permission.ua.roles.manage";
    public const string AssignmentsManage = "permission.ua.assignments.manage";
}
