using FluentValidation;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.RevokePermission;

namespace Sergin.UserAccess.Application.Roles.Commands.RevokePermission;

internal sealed class RevokeRolePermissionCommandValidator : AbstractValidator<RevokeRolePermissionCommand>
{
    public RevokeRolePermissionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Permission).NotNull();
    }
}
