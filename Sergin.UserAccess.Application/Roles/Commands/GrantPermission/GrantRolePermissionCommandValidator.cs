using FluentValidation;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.GrantPermission;

namespace Sergin.UserAccess.Application.Roles.Commands.GrantPermission;

internal sealed class GrantRolePermissionCommandValidator : AbstractValidator<GrantRolePermissionCommand>
{
    public GrantRolePermissionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Permission).NotNull();
    }
}
