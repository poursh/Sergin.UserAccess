using Sergin.SharedKernel.Application.Securities;

namespace Sergin.UserAccess.Application.Users.Commands.ProvisionExternalUser;

public sealed record ProvisionExternalUserCommandResponse(
    Guid UserId, IReadOnlyCollection<Permission> Permissions);
