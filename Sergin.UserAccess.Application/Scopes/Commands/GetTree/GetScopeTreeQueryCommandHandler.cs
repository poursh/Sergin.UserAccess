using Sergin.SharedKernel.Application.Commands.Queries;
using Sergin.SharedKernel.Application.Securities.Users;
using Sergin.SharedKernel.Domain.Securities;
using Sergin.UserAccess.Application.Authorization;
using Sergin.UserAccess.Application.Contracts.Scopes.Commands.GetTree;
using Sergin.UserAccess.Domain.Scopes;
using Sergin.UserAccess.Application.Configurations.Scopes;

namespace Sergin.UserAccess.Application.Scopes.Commands.GetTree;

/// <summary>
/// The scopes the caller may read: all of them for a system identity, otherwise the scopes their active assignments
/// carrying the permission reach (a root grant over the subtree reaches all of them). A visible scope whose parent is not visible comes back as a top-level node.
/// </summary>
internal sealed class GetScopeTreeQueryCommandHandler(
    IScopeQueryRepository repository,
    IUserContext userContext,
    ScopedPermissionChecker checker) : IQueryHandler<GetScopeTreeQueryCommand, IReadOnlyCollection<ScopeTreeItem>>
{
    public async Task<ErrorOr<IReadOnlyCollection<ScopeTreeItem>>> Handle(GetScopeTreeQueryCommand request, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<ScopeTreeRow> rows = await repository.GetTreeAsync(cancellationToken);

        Func<ScopePath, bool> visible;

        if (userContext is ISystemUserContext)
        {
            visible = _ => true;
        }
        else
        {
            IReadOnlyCollection<EffectiveGrant> grants = [
                .. (await checker.GetActiveGrantsAsync(userContext.Id.Value, cancellationToken))
                    .Where(grant => grant.Permissions.Contains(ScopePermissions.Read.Permission.Value)
                        || grant.Permissions.Contains(Permission.AllPlatform.Value))
            ];

            visible = path => grants.Any(grant => grant.Covers(path));
        }

        IReadOnlyCollection<ScopeTreeItem> tree =
        [
            .. ScopeTreeProjection.Project(rows, visible)
                .Select(scope => new ScopeTreeItem(scope.Id, scope.ParentId, scope.Name, scope.Depth))
        ];

        return tree.ToErrorOr();
    }
}
