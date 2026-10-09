namespace Sergin.UserAccess.Presentation.Blazor.Roles.Models;

public sealed class NewRoleFormModel
{
    public string Name { get; set; } = string.Empty;

    public bool IsInheritable { get; set; }

    public IReadOnlyCollection<string> Permissions { get; set; } = [];
}
