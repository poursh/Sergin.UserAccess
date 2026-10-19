namespace Sergin.UserAccess.Presentation.Blazor.Users.Assignments.Models;

/// <summary>The assign form. Dates are calendar days, taken as UTC midnight.</summary>
public sealed class NewAssignmentFormModel
{
    public Guid ScopeId { get; set; }

    public Guid RoleId { get; set; }

    public bool IncludeSubtree { get; set; } = true;

    public DateTime? ValidFromUtc { get; set; }

    public DateTime? ValidUntilUtc { get; set; }
}
