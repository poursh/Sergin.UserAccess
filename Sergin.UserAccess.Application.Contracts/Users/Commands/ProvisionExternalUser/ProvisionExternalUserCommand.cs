using Sergin.SharedKernel.Application.Commands;
using Sergin.UserAccess.Domain.Users;

namespace Sergin.UserAccess.Application.Users.Commands.ProvisionExternalUser;

/// <summary>
/// Find-or-create the local user for an identity the provider just authenticated, and answer with what
/// that user may do.
/// </summary>
/// <remarks>
/// Carries no <c>[RequiredPermissions]</c> on purpose, and must not gain one: it runs inside the OIDC
/// callback, before sign-in completes, when the ambient user context is still anonymous. Requiring a
/// permission here would make every login fail.
/// </remarks>
public sealed record ProvisionExternalUserCommand(
    ExternalUserId ExternalId,
    UserName UserName,
    EmailAddress? Email,
    string FirstName,
    string LastName) : ICommand<ProvisionExternalUserCommandResponse>;
