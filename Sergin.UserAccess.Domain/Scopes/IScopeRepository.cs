using Sergin.SharedKernel.Domain.Repositories;

namespace Sergin.UserAccess.Domain.Scopes;

public interface IScopeRepository : IRepository<Scope, ScopeId>
{
    /// <summary>Every scope strictly below <paramref name="scope"/>, tracked: what <see cref="Scope.MoveUnder"/> re-paths.</summary>
    Task<IReadOnlyCollection<Scope>> GetSubtreeAsync(Scope scope, CancellationToken cancellationToken = default);

    Task<bool> HasChildrenAsync(ScopeId id, CancellationToken cancellationToken = default);
}
