using Sergin.SharedKernel.Application.Concurrency;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.GetAssignable;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.GetOne;

namespace Sergin.UserAccess.Application.Roles;

public interface IRoleQueryRepository
{
    Task<Versioned<RoleQueryResponse>?> GetRoleById(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AssignableRoleItem>> GetAssignableRolesAsync(Guid scopeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// BR-4: a live role with this code owned by <paramref name="ownerScopeId"/>, one of its ancestors or one of its
    /// descendants, other than <paramref name="excludeRoleId"/>.
    /// </summary>
    Task<bool> IsNameTakenInLineAsync(Guid ownerScopeId, string name, Guid? excludeRoleId, CancellationToken cancellationToken = default);

    /// <summary>BR-4 for a rename: <see cref="IsNameTakenInLineAsync"/> from the role's own owner scope.</summary>
    Task<bool> IsNameTakenInLineOfRoleAsync(Guid roleId, string name, CancellationToken cancellationToken = default);

    Task<bool> HasLiveAssignmentsAsync(Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>Live assignments of the role at a scope other than its owner: what making it non-inheritable would strand.</summary>
    Task<bool> HasLiveAssignmentsAwayFromOwnerAsync(Guid roleId, CancellationToken cancellationToken = default);
}
