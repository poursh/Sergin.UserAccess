using System.Collections.Concurrent;
using Sergin.UserAccess.Domain.Scopes;

namespace Sergin.UserAccess.Application.Authorization;

/// <summary>
/// Per-user effective grants and per-scope paths, held in process (NFR-1). Every entry carries the generation it
/// was loaded under; <see cref="Invalidate"/> moves the generation on and clears both maps, so an entry loaded by
/// a read that raced the change is never served (NFR-3). Entries also expire after <see cref="TimeToLive"/>, so
/// a change made outside the application is picked up without a restart.
/// </summary>
public sealed class EffectiveGrantCache : IAuthorizationCacheInvalidator
{
    public static readonly TimeSpan TimeToLive = TimeSpan.FromMinutes(5);

    private readonly ConcurrentDictionary<Guid, Entry<IReadOnlyCollection<EffectiveGrant>>> grants = new();
    private readonly ConcurrentDictionary<Guid, Entry<ScopePath?>> paths = new();
    private long generation;

    public long Generation => Interlocked.Read(ref generation);

    public void Invalidate()
    {
        Interlocked.Increment(ref generation);
        grants.Clear();
        paths.Clear();
    }

    internal bool TryGetGrants(Guid userId, DateTime nowUtc, out IReadOnlyCollection<EffectiveGrant> value) =>
        TryGet(grants, userId, nowUtc, out value);

    internal void SetGrants(Guid userId, long loadedUnder, DateTime nowUtc, IReadOnlyCollection<EffectiveGrant> value) =>
        Set(grants, userId, loadedUnder, nowUtc, value);

    internal bool TryGetPath(Guid scopeId, DateTime nowUtc, out ScopePath? value) =>
        TryGet(paths, scopeId, nowUtc, out value);

    internal void SetPath(Guid scopeId, long loadedUnder, DateTime nowUtc, ScopePath? value) =>
        Set(paths, scopeId, loadedUnder, nowUtc, value);

    private bool TryGet<T>(ConcurrentDictionary<Guid, Entry<T>> map, Guid key, DateTime nowUtc, out T value)
    {
        if (map.TryGetValue(key, out Entry<T>? entry) && entry.Generation == Generation && nowUtc < entry.ExpiresAtUtc)
        {
            value = entry.Value;
            return true;
        }

        value = default!;
        return false;
    }

    private void Set<T>(ConcurrentDictionary<Guid, Entry<T>> map, Guid key, long loadedUnder, DateTime nowUtc, T value)
    {
        // Loaded before an invalidation that has since happened: the data may predate the change, so drop it.
        if (loadedUnder == Generation)
        {
            map[key] = new Entry<T>(value, loadedUnder, nowUtc + TimeToLive);
        }
    }

    private sealed record Entry<T>(T Value, long Generation, DateTime ExpiresAtUtc);
}

/// <summary>One assignment's reach and permissions, as the check evaluates it.</summary>
public sealed record EffectiveGrant(
    Guid AssignmentId,
    Guid ScopeId,
    ScopePath ScopePath,
    bool IncludeSubtree,
    DateTime? ValidFromUtc,
    DateTime? ValidUntilUtc,
    IReadOnlySet<string> Permissions)
{
    public bool IsActiveAt(DateTime nowUtc) =>
        (ValidFromUtc is null || ValidFromUtc <= nowUtc) && (ValidUntilUtc is null || nowUtc < ValidUntilUtc);

    /// <summary>FR-7: the assignment's own scope, or anywhere below it when it covers the subtree.</summary>
    public bool Covers(ScopePath target) =>
        ScopePath == target || IncludeSubtree && ScopePath.IsAncestorOrSelfOf(target);
}
