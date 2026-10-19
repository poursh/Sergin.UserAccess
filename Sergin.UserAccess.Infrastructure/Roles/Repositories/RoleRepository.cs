using Microsoft.EntityFrameworkCore;
using Sergin.SharedKernel.Infrastructure.Data.EFCore.Repositories;
using Sergin.UserAccess.Domain.Roles;
using Sergin.UserAccess.Domain.Scopes;
using Sergin.UserAccess.Infrastructure.Data;

namespace Sergin.UserAccess.Infrastructure.Roles.Repositories;

internal class RoleRepository(IUserAccessDbContext dbContext)
    : EfRepository<Role, RoleId>(dbContext), IRoleRepository
{
    public async Task<IReadOnlyCollection<Role>> GetOwnedByAsync(ScopeId ownerScopeId, CancellationToken cancellationToken = default)
    {
        return await Set.Where(role => role.OwnerScopeId == ownerScopeId).ToListAsync(cancellationToken);
    }

    public Task<Role?> GetByName(ScopeId ownerScopeId, RoleName name, CancellationToken cancellationToken = default)
    {
        return Set.SingleOrDefaultAsync(role => role.OwnerScopeId == ownerScopeId && role.Name == name, cancellationToken);
    }
}
