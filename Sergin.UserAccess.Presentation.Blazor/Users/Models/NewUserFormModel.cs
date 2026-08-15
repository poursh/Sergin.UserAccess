using System.ComponentModel.DataAnnotations;

namespace Sergin.UserAccess.Presentation.Blazor.Users.Models;

public sealed class NewUserFormModel
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string UserName { get; set; } = string.Empty;
}
