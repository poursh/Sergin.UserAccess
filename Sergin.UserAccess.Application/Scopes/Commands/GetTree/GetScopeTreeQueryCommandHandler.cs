using Sergin.SharedKernel.Application.Commands.Queries;
using Sergin.SharedKernel.Application.Securities.Users;
using Sergin.SharedKernel.Domain.Securities;
using Sergin.UserAccess.Application.Authorization;
using Sergin.UserAccess.Application.Contracts;
using Sergin.UserAccess.Application.Contracts.Scopes.Commands.GetTree;
using Sergin.UserAccess.Domain.Scopes;

namespace Sergin.UserAccess.Application.Scopes.Commands.GetTree;

/// <summary>
/// The scopes the caller may read: all of them with the permission globally, otherwise the scopes their active
/// assignments carrying it reach. A visible scope whose parent is not visible comes back as a top-level node.
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

        if (userContext.HasPermission(UserAccessPermissions.ScopesRead))
        {
            visible = _ => true;
        }
        else
        {
            IReadOnlyCollection<EffectiveGrant> grants = [
                .. (await checker.GetActiveGrantsAsync(userContext.Id.Value, cancellationToken))
                    .Where(grant => grant.Permissions.Contains(UserAccessPermissions.ScopesRead)
                        || grant.Permissions.Contains(Permission.AllPlatform.Value))
            ];

            visible = path => grants.Any(grant => grant.Covers(path));
        }

        HashSet<Guid> shown = [.. rows.Where(row => visible(new ScopePath(row.Path))).Select(row => row.Id)];

        IReadOnlyCollection<ScopeTreeItem> tree =
        [
            .. rows
                .Where(row => shown.Contains(row.Id))
                .Select(row => new ScopeTreeItem(
                    row.Id,
                    row.ParentId is { } parentId && shown.Contains(parentId) ? parentId : null,
                    row.Name,
                    new ScopePath(row.Path).Depth))
        ];

        return tree.ToErrorOr();
    }
}
