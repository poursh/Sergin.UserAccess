using Sergin.SharedKernel.Application.Commands;
using Sergin.SharedKernel.Application.Securities.Authorization;
using Sergin.UserAccess.Application.Authorization;
using Sergin.UserAccess.Application.Contracts.Roles;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.GrantPermission;
using Sergin.UserAccess.Domain.Roles;

namespace Sergin.UserAccess.Application.Roles.Commands.GrantPermission;

internal sealed class GrantRolePermissionCommandHandler(
    IUserAccessUnitOfWork unitOfWork,
    IRoleRepository roles,
    IScopeAuthorization authorization,
    IAuthorizationCacheInvalidator cache) : ICommandHandler<GrantRolePermissionCommand, GrantRolePermissionCommandResponse>
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

        if (!await authorization.CurrentUserHasAsync(request.Permission, role.OwnerScopeId.Value, cancellationToken))
        {
            return PermissionCeiling.Exceeded(request.Permission);
        }

        role.Grant(request.Permission);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        cache.Invalidate();

        return new GrantRolePermissionCommandResponse(role.Id.Value);
    }
}
