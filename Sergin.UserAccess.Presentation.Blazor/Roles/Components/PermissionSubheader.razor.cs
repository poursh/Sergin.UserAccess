using Microsoft.AspNetCore.Components;

namespace Sergin.UserAccess.Presentation.Blazor.Roles.Components;

/// <summary>
/// A module or aggregate heading inside the permission <c>MudSelect</c>. The select renders its children a second
/// time, outside the popover, to register its items, cascading <c>HideContent</c> to that copy; <c>MudSelectItem</c>
/// honours it but <c>MudListSubheader</c> does not, so a bare subheader would leak onto the page under the field.
/// </summary>
public sealed partial class PermissionSubheader
{
    [CascadingParameter(Name = "HideContent")]
    private bool HideContent { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
