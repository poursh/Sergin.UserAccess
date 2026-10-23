using Sergin.SharedKernel.Application.Commands;
using Sergin.SharedKernel.Application.Securities.Authorization;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.Create;
using Sergin.SharedKernel.Domain.Securities;
using Sergin.UserAccess.Domain.Roles;
using Sergin.UserAccess.Domain.Scopes;
using Sergin.UserAccess.Application.Configurations.Roles;

namespace Sergin.UserAccess.Application.Roles.Commands.Create;

internal sealed class CreateRoleCommandHandler(
    IUserAccessUnitOfWork unitOfWork,
    IRoleRepository roles,
    IScopeAuthorization authorization,
    PermissionCatalog catalog) : ICommandHandler<CreateRoleCommand, CreateRoleCommandResponse>
{
    public async Task<ErrorOr<CreateRoleCommandResponse>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        if (!await authorization.CurrentUserHasAsync(RolePermissions.Manage, request.OwnerScopeId, cancellationToken))
        {
            return Error.Forbidden();
        }

        foreach (Permission permission in request.Permissions)
        {
            // A grant is data: resolved against the catalogue, which the validator already required it to be in.
            if (!catalog.TryGet(permission, out PermissionDefinition? definition)
                || !await authorization.CurrentUserHasAsync(definition, request.OwnerScopeId, cancellationToken))
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
