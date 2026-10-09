using Sergin.SharedKernel.Application.Commands;
using Sergin.SharedKernel.Application.Securities.Authorization;
using Sergin.UserAccess.Application.Authorization;
using Sergin.UserAccess.Application.Contracts;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.RevokePermission;
using Sergin.UserAccess.Domain.Roles;

namespace Sergin.UserAccess.Application.Roles.Commands.RevokePermission;

internal sealed class RevokeRolePermissionCommandHandler(
    IUserAccessUnitOfWork unitOfWork,
    IRoleRepository roles,
    IScopeAuthorization authorization,
    IAuthorizationCacheInvalidator cache) : ICommandHandler<RevokeRolePermissionCommand, RevokeRolePermissionCommandResponse>
{
    public async Task<ErrorOr<RevokeRolePermissionCommandResponse>> Handle(RevokeRolePermissionCommand request, CancellationToken cancellationToken)
    {
        Role? role = await roles.GetAsync(new RoleId(request.Id), cancellationToken);

        if (role is null)
        {
            return Error.NotFound();
        }

        if (!await authorization.CurrentUserHasAsync(UserAccessPermissions.RolesManage, role.OwnerScopeId.Value, cancellationToken))
        {
            return Error.Forbidden();
        }

        role.Revoke(request.Permission);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        cache.Invalidate();

        return new RevokeRolePermissionCommandResponse(role.Id.Value);
    }
}
