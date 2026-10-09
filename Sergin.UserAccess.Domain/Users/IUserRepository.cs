using Sergin.SharedKernel.Domain.Repositories;

namespace Sergin.UserAccess.Domain.Users;
public interface IUserRepository : IRepository<User, UserInternalId>, IUniqueKeyRepository<UserName>
{
    Task<User?> GetByUserName(UserName userName, CancellationToken cancellationToken = default);

    Task<User?> GetByExternalId(ExternalUserId externalId, CancellationToken cancellationToken = default);

    /// <summary>The user with their assignments loaded: what <see cref="User.AssignRole"/> and <see cref="User.RevokeAssignment"/> work on.</summary>
    Task<User?> GetWithAssignmentsAsync(UserInternalId id, CancellationToken cancellationToken = default);

    /// <summary>Every user holding a live assignment of the role, assignments loaded: for a cascading role delete.</summary>
    Task<IReadOnlyCollection<User>> GetWithLiveAssignmentsOfRoleAsync(Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>Every user holding a live assignment at the scope, assignments loaded: for a cascading scope delete.</summary>
    Task<IReadOnlyCollection<User>> GetWithLiveAssignmentsAtScopeAsync(Guid scopeId, CancellationToken cancellationToken = default);
}
