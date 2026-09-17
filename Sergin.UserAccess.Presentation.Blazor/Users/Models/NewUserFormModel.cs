namespace Sergin.UserAccess.Presentation.Blazor.Users.Models;

/// <summary>
/// The binding target for the create form. No DataAnnotations: the field is validated by the
/// pipeline's own <c>CreateUserCommandValidator</c> through <c>ISerginFormValidator</c>, and
/// attributes here would make MudForm report every rule twice.
/// </summary>
public sealed class NewUserFormModel
{
    public string UserName { get; set; } = string.Empty;
}
