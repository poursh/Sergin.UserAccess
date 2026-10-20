using FluentValidation;
using Sergin.SharedKernel.Application.Securities.Authorization;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.GrantPermission;

namespace Sergin.UserAccess.Application.Roles.Commands.GrantPermission;

internal sealed class GrantRolePermissionCommandValidator : AbstractValidator<GrantRolePermissionCommand>
{
    public GrantRolePermissionCommandValidator(PermissionCatalog catalog)
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Permission).NotNull();

        // Advisory like the other repository rules: the catalogue is what the platform defines today. Revoke
        // has no such rule, so a name a release dropped can still be taken off a role.
        RuleFor(x => x.Permission)
            .Must(permission => catalog.TryGet(permission, out _))
            .WithMessage("'{PropertyName}' must be a permission the platform defines.")
            .When(x => x.Permission is not null);
    }
}
