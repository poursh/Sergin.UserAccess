using Microsoft.EntityFrameworkCore;
using Sergin.SharedKernel.Infrastructure.Data.EFCore.Repositories;
using Sergin.UserAccess.Domain.Scopes;
using Sergin.UserAccess.Infrastructure.Data;

namespace Sergin.UserAccess.Infrastructure.Scopes.Repositories;

internal class ScopeRepository(IUserAccessDbContext dbContext)
    : EfRepository<Scope, ScopeId>(dbContext), IScopeRepository
{
    public async Task<IReadOnlyCollection<Scope>> GetSubtreeAsync(Scope scope, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);

        // <@ on the GiST-indexed path: every scope below this one in one indexed read. FromSql composes with the
        // soft-delete query filter, so a deleted scope is not re-pathed.
        return await Set
            .FromSql($"SELECT * FROM ua.scopes WHERE path <@ {scope.Path.Value}::ltree")
            .Where(candidate => candidate.Id != scope.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> HasChildrenAsync(ScopeId id, CancellationToken cancellationToken = default)
        => AnyAsync(scope => scope.ParentId == id, cancellationToken);
}
