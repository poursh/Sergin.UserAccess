using Sergin.SharedKernel.Domain.Securities;
using Sergin.UserAccess.Domain.Users;

namespace Sergin.UserAccess.Application.Users.Commands.ProvisionExternalUser;

public interface IGetUserPermissionsQueryRepository
{
    /// <summary>
    /// Every permission the user holds through their roles, deduplicated. Read side, so raw SQL across
    /// the two join tables rather than loading the Role aggregates.
    /// </summary>
    Task<IReadOnlyCollection<Permission>> GetPermissions(
        UserInternalId userId, CancellationToken cancellationToken = default);
}
