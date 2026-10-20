using Sergin.SharedKernel.Application.Commands;
using Sergin.SharedKernel.Application.Securities.Authorization;
using Sergin.UserAccess.Application.Authorization;
using Sergin.UserAccess.Application.Contracts.Roles;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.Rename;
using Sergin.UserAccess.Domain.Roles;

namespace Sergin.UserAccess.Application.Roles.Commands.Rename;

internal sealed class RenameRoleCommandHandler(
    IUserAccessUnitOfWork unitOfWork,
    IRoleRepository roles,
    IScopeAuthorization authorization,
    IAuthorizationCacheInvalidator cache) : ICommandHandler<RenameRoleCommand, RenameRoleCommandResponse>
{
    public async Task<ErrorOr<RenameRoleCommandResponse>> Handle(RenameRoleCommand request, CancellationToken cancellationToken)
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

        role.Rename(request.Name);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        cache.Invalidate();

        return new RenameRoleCommandResponse(role.Id.Value);
    }
}
