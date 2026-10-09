using Sergin.SharedKernel.Application.Commands.Queries;
using Sergin.SharedKernel.Application.Securities.Authorization;
using Sergin.UserAccess.Application.Contracts;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.GetAssignable;

namespace Sergin.UserAccess.Application.Roles.Commands.GetAssignable;

internal sealed class GetAssignableRolesQueryCommandHandler(IRoleQueryRepository repository, IScopeAuthorization authorization)
    : IQueryHandler<GetAssignableRolesQueryCommand, IReadOnlyCollection<AssignableRoleItem>>
{
    public async Task<ErrorOr<IReadOnlyCollection<AssignableRoleItem>>> Handle(
        GetAssignableRolesQueryCommand request, CancellationToken cancellationToken)
    {
        if (!await authorization.CurrentUserHasAsync(UserAccessPermissions.RolesRead, request.ScopeId, cancellationToken))
        {
            return Error.Forbidden();
        }

        return (await repository.GetAssignableRolesAsync(request.ScopeId, cancellationToken)).ToErrorOr();
    }
}
