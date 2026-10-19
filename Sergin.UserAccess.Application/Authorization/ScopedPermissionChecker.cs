using Sergin.SharedKernel.Application.Securities.Authorization;
using Sergin.SharedKernel.Application.Times;
using Sergin.SharedKernel.Domain.Securities;
using Sergin.UserAccess.Domain.Scopes;

namespace Sergin.UserAccess.Application.Authorization;

/// <summary>
/// FR-9: whether a user holds a permission on a target scope, from every active assignment. An assignment grants
/// on its own scope, and below it when it covers the subtree; <see cref="Permission.AllPlatform"/> in a role
/// grants every permission wherever that role reaches. An unknown or deleted target scope grants nothing.
/// Also SharedKernel's <see cref="IScopePathResolver"/>, answered from the same cached path lookup.
/// </summary>
internal sealed class ScopedPermissionChecker(
    IEffectiveGrantQueryRepository grants,
    EffectiveGrantCache cache,
    IDateTimeProvider clock) : IScopedPermissionChecker, IScopePathResolver
{
    public async Task<bool> HasPermissionAsync(
        Guid userId, Permission permission, Guid scopeId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(permission);

        DateTime nowUtc = clock.UtcNow;

        ScopePath? target = await GetPathAsync(scopeId, nowUtc, cancellationToken);

        if (target is null)
        {
            return false;
        }

        IReadOnlyCollection<EffectiveGrant> userGrants = await GetGrantsAsync(userId, nowUtc, cancellationToken);

        return userGrants.Any(grant =>
            grant.IsActiveAt(nowUtc)
            && grant.Covers(target)
            && (grant.Permissions.Contains(permission.Value) || grant.Permissions.Contains(Permission.AllPlatform.Value)));
    }

    public async Task<ScopeReach> GetReachAsync(Guid userId, Permission permission, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(permission);

        IReadOnlyCollection<EffectiveGrant> active = await GetActiveGrantsAsync(userId, cancellationToken);

        return ScopeReach.Of(active
            .Where(grant => grant.Permissions.Contains(permission.Value) || grant.Permissions.Contains(Permission.AllPlatform.Value))
            .Select(grant => new ScopeReachEntry(grant.ScopeId, grant.ScopePath.Value, grant.IncludeSubtree)));
    }

    public async Task<string?> GetPathAsync(Guid scopeId, CancellationToken cancellationToken = default) =>
        (await GetPathAsync(scopeId, clock.UtcNow, cancellationToken))?.Value;

    /// <summary>FR-11: every permission the user holds, per assignment, with the scope each one reaches from.</summary>
    internal async Task<IReadOnlyCollection<EffectiveGrant>> GetActiveGrantsAsync(Guid userId, CancellationToken cancellationToken)
    {
        DateTime nowUtc = clock.UtcNow;

        return [.. (await GetGrantsAsync(userId, nowUtc, cancellationToken)).Where(grant => grant.IsActiveAt(nowUtc))];
    }

    private async Task<ScopePath?> GetPathAsync(Guid scopeId, DateTime nowUtc, CancellationToken cancellationToken)
    {
        if (cache.TryGetPath(scopeId, nowUtc, out ScopePath? cached))
        {
            return cached;
        }

        long generation = cache.Generation;
        string? path = await grants.GetScopePathAsync(scopeId, cancellationToken);
        ScopePath? value = path is null ? null : new ScopePath(path);
        cache.SetPath(scopeId, generation, nowUtc, value);

        return value;
    }

    private async Task<IReadOnlyCollection<EffectiveGrant>> GetGrantsAsync(Guid userId, DateTime nowUtc, CancellationToken cancellationToken)
    {
        if (cache.TryGetGrants(userId, nowUtc, out IReadOnlyCollection<EffectiveGrant> cached))
        {
            return cached;
        }

        long generation = cache.Generation;
        IReadOnlyCollection<EffectiveGrantRow> rows = await grants.GetGrantsAsync(userId, cancellationToken);

        IReadOnlyCollection<EffectiveGrant> value =
        [
            .. rows
                .GroupBy(row => row.AssignmentId)
                .Select(assignment =>
                {
                    EffectiveGrantRow first = assignment.First();

                    return new EffectiveGrant(
                        first.AssignmentId,
                        first.ScopeId,
                        new ScopePath(first.ScopePath),
                        first.IncludeSubtree,
                        first.ValidFromUtc,
                        first.ValidUntilUtc,
                        assignment.Select(row => row.Permission).ToHashSet(StringComparer.Ordinal));
                })
        ];

        cache.SetGrants(userId, generation, nowUtc, value);

        return value;
    }
}
