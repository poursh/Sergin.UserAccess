using Sergin.SharedKernel.Application.Commands;
using Sergin.SharedKernel.Application.Securities.Authorization;
using Sergin.UserAccess.Application.Contracts.Roles;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.Create;
using Sergin.SharedKernel.Domain.Securities;
using Sergin.UserAccess.Domain.Roles;
using Sergin.UserAccess.Domain.Scopes;

namespace Sergin.UserAccess.Application.Roles.Commands.Create;

internal sealed class CreateRoleCommandHandler(
    IUserAccessUnitOfWork unitOfWork,
    IRoleRepository roles,
    IScopeAuthorization authorization) : ICommandHandler<CreateRoleCommand, CreateRoleCommandResponse>
{
    public async Task<ErrorOr<CreateRoleCommandResponse>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        if (!await authorization.CurrentUserHasAsync(RolePermissions.Manage, request.OwnerScopeId, cancellationToken))
        {
            return Error.Forbidden();
        }

        foreach (Permission permission in request.Permissions)
        {
            if (!await authorization.CurrentUserHasAsync(permission, request.OwnerScopeId, cancellationToken))
            {
                return PermissionCeiling.Exceeded(permission);
            }
        }

        var role = Role.Create(new ScopeId(request.OwnerScopeId), request.Name, request.IsInheritable, request.Permissions);
        roles.Insert(role);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateRoleCommandResponse(role.Id.Value);
    }
}
