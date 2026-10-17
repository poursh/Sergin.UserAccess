using MudBlazor;
using Sergin.SharedKernel.Modules;
using Sergin.UserAccess.Application.Contracts;

namespace Sergin.UserAccess.Presentation.Blazor;

/// <summary>
/// The module's drawer entries. Each is also named individually so a page can build its breadcrumb section step
/// with <c>SerginBreadcrumb.Of(...)</c> from the one place the drawer reads the label and href.
/// </summary>
public static class UserAccessNavigation
{
    public static SerginNavItem Users { get; } = new(
        "Users",
        "/ua/users",
        Icons.Material.Filled.People,
        Order: 200,
        RequiredPermission: UserAccessPermissions.UsersRead,
        AtRoot: true);

    // Gated on the global permission: a regional administrator, who holds scopes.read only in their region,
    // reaches the pages through a link rather than the drawer.
    public static SerginNavItem Scopes { get; } = new(
        "Scopes",
        "/ua/scopes",
        Icons.Material.Filled.AccountTree,
        Order: 210,
        RequiredPermission: UserAccessPermissions.ScopesRead,
        AtRoot: true);

    public static IReadOnlyCollection<SerginNavItem> Items { get; } = [Users, Scopes];
}
