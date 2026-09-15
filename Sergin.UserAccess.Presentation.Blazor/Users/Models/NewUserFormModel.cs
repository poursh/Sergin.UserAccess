using System.ComponentModel.DataAnnotations;

namespace Sergin.UserAccess.Presentation.Blazor.Users.Models;

public sealed class NewUserFormModel
{
    [Required]
    [StringLength(Domain.Users.UserName.MaxLength, MinimumLength = 1)]
    public string UserName { get; set; } = string.Empty;
}
