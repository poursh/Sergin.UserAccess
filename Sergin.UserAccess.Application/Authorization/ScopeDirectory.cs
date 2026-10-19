using Sergin.SharedKernel.Application.Securities.Authorization;
using Sergin.SharedKernel.Application.Securities.Users;
using Sergin.UserAccess.Application.Scopes;
using Sergin.UserAccess.Domain.Scopes;

namespace Sergin.UserAccess.Application.Authorization;

/// <summary>
/// SharedKernel's <see cref="IScopeDirectory"/>: every live scope an active assignment of the current user reaches,
/// whatever the assignment's permissions — unlike the tree query, which shows what <c>permission.ua.scopes.read</c>
/// reaches. A system identity reaches every scope.
/// </summary>
internal sealed class ScopeDirectory(
    IScopeQueryRepository repository,
    IUserContext userContext,
    ScopedPermissionChecker checker) : IScopeDirectory
{
    public async Task<IReadOnlyList<ScopeOption>> GetReachableAsync(CancellationToken cancellationToken = default)
    {
        Func<ScopePath, bool> visible;

        if (userContext is ISystemUserContext)
        {
            visible = _ => true;
        }
        else
        {
            IReadOnlyCollection<EffectiveGrant> grants = await checker.GetActiveGrantsAsync(userContext.Id.Value, cancellationToken);

            if (grants.Count == 0)
            {
                return [];
            }

            visible = path => grants.Any(grant => grant.Covers(path));
        }

        IReadOnlyCollection<ScopeTreeRow> rows = await repository.GetTreeAsync(cancellationToken);

        return
        [
            .. ScopeTreeProjection.Project(rows, visible)
                .Select(scope => new ScopeOption(scope.Id, scope.ParentId, scope.Name, scope.Depth))
        ];
    }
}
