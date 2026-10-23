using Sergin.SharedKernel.Application.Commands;
using Sergin.SharedKernel.Application.Securities.Authorization;
using Sergin.UserAccess.Application.Authorization;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.GrantPermission;
using Sergin.SharedKernel.Domain.Securities;
using Sergin.UserAccess.Domain.Roles;
using Sergin.UserAccess.Application.Configurations.Roles;

namespace Sergin.UserAccess.Application.Roles.Commands.GrantPermission;

internal sealed class GrantRolePermissionCommandHandler(
    IUserAccessUnitOfWork unitOfWork,
    IRoleRepository roles,
    IScopeAuthorization authorization,
    IAuthorizationCacheInvalidator cache,
    PermissionCatalog catalog) : ICommandHandler<GrantRolePermissionCommand, GrantRolePermissionCommandResponse>
{
    public async Task<ErrorOr<GrantRolePermissionCommandResponse>> Handle(GrantRolePermissionCommand request, CancellationToken cancellationToken)
    {
        Role? role = await roles.GetAsync(new RoleId(request.Id), cancellationToken);

        if (role is null)
        {
            return Error.NotFound();
        }

        if (!await authorization.CurrentUserHasAsync(RolePermissions.Manage, role.OwnerScopeId.Value, cancellationToken))
        {
            return Error.Forbidden();
        }

        // A grant is data: resolved against the catalogue, which the validator already required it to be in.
        if (!catalog.TryGet(request.Permission, out PermissionDefinition? definition)
            || !await authorization.CurrentUserHasAsync(definition, role.OwnerScopeId.Value, cancellationToken))
        {
            return PermissionCeiling.Exceeded(request.Permission);
        }

        role.Grant(request.Permission);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        cache.Invalidate();

        return new GrantRolePermissionCommandResponse(role.Id.Value);
    }
}
