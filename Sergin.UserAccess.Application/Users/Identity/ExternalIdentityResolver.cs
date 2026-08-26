using MediatR;
using Sergin.SharedKernel.Application.Securities.Users;
using Sergin.UserAccess.Application.Users.Commands.ProvisionExternalUser;
using Sergin.UserAccess.Domain.Users;

namespace Sergin.UserAccess.Application.Users.Identity;

/// <summary>
/// UserAccess's half of the authentication seam: SharedKernel authenticates the caller against Keycloak
/// and asks this to say who they are in Sergin's terms.
/// </summary>
/// <remarks>
/// Goes through <see cref="ISender"/> rather than touching repositories directly so provisioning stays a
/// normal feature slice — same handler, same unit of work, same pipeline as every other write.
/// </remarks>
internal sealed class ExternalIdentityResolver(ISender sender) : IExternalIdentityResolver
{
    public async Task<ExternalIdentityResult> ResolveAsync(
        ExternalIdentity identity, CancellationToken cancellationToken)
    {
        ProvisionExternalUserCommand command = new(
            new ExternalUserId(identity.Subject),
            new UserName(identity.UserName),
            string.IsNullOrWhiteSpace(identity.Email) ? null : new EmailAddress(identity.Email),
            identity.FirstName,
            identity.LastName);

        ErrorOr<ProvisionExternalUserCommandResponse> result = await sender.Send(command, cancellationToken);

        if (result.IsError)
        {
            throw new InvalidOperationException(
                $"Could not provision the signed-in user '{identity.UserName}': {result.FirstError.Description}");
        }

        return new ExternalIdentityResult(result.Value.UserId, result.Value.Permissions);
    }
}
