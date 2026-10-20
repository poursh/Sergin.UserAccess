using MudBlazor;
using Sergin.SharedKernel.Modules;
using Sergin.UserAccess.Application.Contracts.Users;
using Sergin.UserAccess.Application.Contracts.Scopes;

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
        RequiredPermission: UserPermissions.Read);

    // Shown to a regional administrator too: the tree page lists the subtrees their scopes.read grants reach.
    public static SerginNavItem Scopes { get; } = new(
        "Scopes",
        "/ua/scopes",
        Icons.Material.Filled.AccountTree,
        Order: 210,
        RequiredPermission: ScopePermissions.Read);

    public static IReadOnlyCollection<SerginNavItem> Items { get; } = [Users, Scopes];
}
