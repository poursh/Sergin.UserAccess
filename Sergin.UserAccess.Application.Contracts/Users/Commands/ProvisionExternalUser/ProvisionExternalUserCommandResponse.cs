using Sergin.SharedKernel.Domain.Securities;

namespace Sergin.UserAccess.Application.Contracts.Users.Commands.ProvisionExternalUser;

public sealed record ProvisionExternalUserCommandResponse(
    Guid UserId, IReadOnlyCollection<Permission> Permissions);
