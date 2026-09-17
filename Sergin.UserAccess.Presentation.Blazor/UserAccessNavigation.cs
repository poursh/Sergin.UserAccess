using MudBlazor;
using Sergin.SharedKernel.Modules;

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
        RequiredPermission: "permission.ua.users.read");

    public static IReadOnlyCollection<SerginNavItem> Items { get; } = [Users];
}
