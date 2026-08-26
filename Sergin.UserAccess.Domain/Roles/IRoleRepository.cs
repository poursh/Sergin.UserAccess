using Sergin.SharedKernel.Domain.Repositories;

namespace Sergin.UserAccess.Domain.Roles;

public interface IRoleRepository : IRepository<Role, RoleId>
{
    Task<Role?> GetByName(RoleName name, CancellationToken cancellationToken = default);
}
