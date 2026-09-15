using Microsoft.EntityFrameworkCore;
using Sergin.SharedKernel.Infrastructure.Data.EFCore.Repositories;
using Sergin.UserAccess.Domain.Roles;
using Sergin.UserAccess.Infrastructure.Data;

namespace Sergin.UserAccess.Infrastructure.Roles.Repositories;

internal class RoleRepository(IUserAccessDbContext dbContext)
    : EfRepository<Role, RoleId>(dbContext), IRoleRepository
{
    public Task<Role?> GetByName(RoleName name, CancellationToken cancellationToken = default)
    {
        return Set.SingleOrDefaultAsync(role => role.Name == name, cancellationToken);
    }
}
