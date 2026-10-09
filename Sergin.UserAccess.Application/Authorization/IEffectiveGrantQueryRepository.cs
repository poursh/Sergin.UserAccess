namespace Sergin.UserAccess.Application.Authorization;

/// <summary>
/// The reads the scoped permission check is answered from. Each is one indexed query, never recursive (NFR-2):
/// the scope tree is a materialised <c>ltree</c> path, so "is this scope below that one" is a prefix test.
/// </summary>
public interface IEffectiveGrantQueryRepository
{
    /// <summary>
    /// One row per permission of every assignment that can still grant something: not revoked, not expired, its
    /// role and scope live, and the user active (FR-13) — an inactive user gets no rows at all.
    /// </summary>
    Task<IReadOnlyCollection<EffectiveGrantRow>> GetGrantsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>The live scope's <c>ltree</c> path, or null for an unknown or deleted scope.</summary>
    Task<string?> GetScopePathAsync(Guid scopeId, CancellationToken cancellationToken = default);
}

public sealed record EffectiveGrantRow(
    Guid AssignmentId,
    Guid ScopeId,
    string ScopePath,
    bool IncludeSubtree,
    DateTime? ValidFromUtc,
    DateTime? ValidUntilUtc,
    string Permission);
