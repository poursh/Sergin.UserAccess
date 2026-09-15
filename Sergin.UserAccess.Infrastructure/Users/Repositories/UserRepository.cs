using Microsoft.EntityFrameworkCore;
using Sergin.SharedKernel.Infrastructure.Data.EFCore.Repositories;
using Sergin.UserAccess.Domain.Users;
using Sergin.UserAccess.Infrastructure.Data;

namespace Sergin.UserAccess.Infrastructure.Users.Repositories;

internal class UserRepository(IUserAccessDbContext dbContext)
    : EfRepository<User, UserInternalId>(dbContext), IUserRepository
{
    public Task<User?> GetByUserName(UserName userName, CancellationToken cancellationToken = default)
    {
        return Set.SingleOrDefaultAsync(u => u.UserName == userName, cancellationToken);
    }

    public Task<User?> GetByExternalId(ExternalUserId externalId, CancellationToken cancellationToken = default)
    {
        return Set.SingleOrDefaultAsync(u => u.ExternalId == externalId, cancellationToken);
    }

    public Task<bool> IsTakenAsync(UserName key, CancellationToken cancellationToken = default)
        => AnyAsync(u => u.UserName == key, cancellationToken);
}
