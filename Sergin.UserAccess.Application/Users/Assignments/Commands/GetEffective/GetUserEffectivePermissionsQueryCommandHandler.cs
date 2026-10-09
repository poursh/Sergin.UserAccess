using Sergin.SharedKernel.Application.Commands.Queries;
using Sergin.UserAccess.Application.Authorization;
using Sergin.UserAccess.Application.Scopes;
using Sergin.UserAccess.Application.Contracts.Users.Assignments.Commands.GetEffective;

namespace Sergin.UserAccess.Application.Users.Assignments.Commands.GetEffective;

internal sealed class GetUserEffectivePermissionsQueryCommandHandler(ScopedPermissionChecker checker, IScopeQueryRepository scopes)
    : IQueryHandler<GetUserEffectivePermissionsQueryCommand, IReadOnlyCollection<EffectivePermissionItem>>
{
    public async Task<ErrorOr<IReadOnlyCollection<EffectivePermissionItem>>> Handle(
        GetUserEffectivePermissionsQueryCommand request, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<EffectiveGrant> grants = await checker.GetActiveGrantsAsync(request.UserId, cancellationToken);
        IReadOnlyDictionary<Guid, string> names =
            await scopes.GetNamesAsync([.. grants.Select(grant => grant.ScopeId).Distinct()], cancellationToken);

        // One row per scope and coverage: two roles at the same place read as one set of permissions.
        IReadOnlyCollection<EffectivePermissionItem> items =
        [
            .. grants
                .GroupBy(grant => (grant.ScopeId, grant.IncludeSubtree))
                .Select(group => new EffectivePermissionItem(
                    group.Key.ScopeId,
                    names.GetValueOrDefault(group.Key.ScopeId, string.Empty),
                    group.Key.IncludeSubtree,
                    [.. group.SelectMany(grant => grant.Permissions).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)]))
                .OrderBy(item => item.ScopeName, StringComparer.Ordinal)
        ];

        return items.ToErrorOr();
    }
}
