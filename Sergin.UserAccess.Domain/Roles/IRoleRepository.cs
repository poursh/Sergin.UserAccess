using Sergin.SharedKernel.Domain.Repositories;
using Sergin.UserAccess.Domain.Scopes;

namespace Sergin.UserAccess.Domain.Roles;

public interface IRoleRepository : IRepository<Role, RoleId>
{
    /// <summary>The role with this code owned by <paramref name="ownerScopeId"/>; codes are unique per owner scope.</summary>
    Task<Role?> GetByName(ScopeId ownerScopeId, RoleName name, CancellationToken cancellationToken = default);
}
