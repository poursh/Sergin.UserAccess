using MudBlazor;
using Sergin.SharedKernel.Modules;

namespace Sergin.UserAccess.Presentation.Blazor;

public static class UserAccessNavigation
{
    public static IReadOnlyCollection<SerginNavItem> Items { get; } =
    [
        new SerginNavItem("Users", "/ua/users", Icons.Material.Filled.People, Order: 200)
    ];
}
