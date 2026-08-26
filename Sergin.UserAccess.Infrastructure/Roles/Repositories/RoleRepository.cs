using Microsoft.EntityFrameworkCore;
using Sergin.UserAccess.Domain.Roles;
using Sergin.UserAccess.Infrastructure.Data;

namespace Sergin.UserAccess.Infrastructure.Roles.Repositories;

internal class RoleRepository(IUserAccessDbContext dbContext) : IRoleRepository
{
    public ValueTask<Role?> GetAsync(RoleId id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Role>().FindAsync([id, cancellationToken], cancellationToken: cancellationToken);
    }

    public Task<Role?> GetByName(RoleName name, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Role>().SingleOrDefaultAsync(role => role.Name == name, cancellationToken);
    }

    public void Insert(Role entity)
    {
        dbContext.Set<Role>().Add(entity);
    }

    public void Remove(Role entity)
    {
        dbContext.Set<Role>().Remove(entity);
    }
}
