using Sergin.SharedKernel.Application.Concurrency;
using Sergin.UserAccess.Application.Contracts.Scopes.Commands.GetOne;

namespace Sergin.UserAccess.Application.Scopes;

public interface IScopeQueryRepository
{
    Task<Versioned<ScopeQueryResponse>?> GetScopeById(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Every live scope, root first; the tree handler narrows it to what the caller may read.</summary>
    Task<IReadOnlyCollection<ScopeTreeRow>> GetTreeAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, string>> GetNamesAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);

    Task<bool> IsSiblingNameTakenAsync(Guid parentId, string name, Guid? excludeScopeId, CancellationToken cancellationToken = default);

    Task<bool> HasLiveAssignmentsAsync(Guid scopeId, CancellationToken cancellationToken = default);

    Task<bool> OwnsLiveRolesAsync(Guid scopeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// BR-6: true when moving the scope under the new parent would leave a live assignment in its subtree outside
    /// its role's owner subtree — an inheritable role owned above the moved subtree whose owner is not an
    /// ancestor of the new parent.
    /// </summary>
    Task<bool> MoveWouldStrandAssignmentsAsync(Guid scopeId, Guid newParentId, CancellationToken cancellationToken = default);
}

public sealed record ScopeTreeRow(Guid Id, Guid? ParentId, string Name, string Path);
