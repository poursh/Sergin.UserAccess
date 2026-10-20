using Sergin.SharedKernel.Application.Commands;
using Sergin.SharedKernel.Application.Securities.Authorization;
using Sergin.UserAccess.Application.Authorization;
using Sergin.UserAccess.Application.Contracts.Roles;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.SetInheritable;
using Sergin.UserAccess.Domain.Roles;

namespace Sergin.UserAccess.Application.Roles.Commands.SetInheritable;

internal sealed class SetRoleInheritableCommandHandler(
    IUserAccessUnitOfWork unitOfWork,
    IRoleRepository roles,
    IScopeAuthorization authorization,
    IAuthorizationCacheInvalidator cache) : ICommandHandler<SetRoleInheritableCommand, SetRoleInheritableCommandResponse>
{
    public async Task<ErrorOr<SetRoleInheritableCommandResponse>> Handle(SetRoleInheritableCommand request, CancellationToken cancellationToken)
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

        role.SetInheritable(request.IsInheritable);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        cache.Invalidate();

        return new SetRoleInheritableCommandResponse(role.Id.Value);
    }
}
