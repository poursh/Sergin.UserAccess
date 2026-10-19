using Sergin.SharedKernel.Domain.Repositories;
using Sergin.UserAccess.Domain.Scopes;

namespace Sergin.UserAccess.Domain.Roles;

public interface IRoleRepository : IRepository<Role, RoleId>
{
    /// <summary>The live roles <paramref name="ownerScopeId"/> owns, tracked: for a cascading scope delete.</summary>
    Task<IReadOnlyCollection<Role>> GetOwnedByAsync(ScopeId ownerScopeId, CancellationToken cancellationToken = default);

    /// <summary>The role with this code owned by <paramref name="ownerScopeId"/>; codes are unique per owner scope.</summary>
    Task<Role?> GetByName(ScopeId ownerScopeId, RoleName name, CancellationToken cancellationToken = default);
}
