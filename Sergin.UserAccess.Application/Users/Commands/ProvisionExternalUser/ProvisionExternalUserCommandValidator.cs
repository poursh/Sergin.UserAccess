using FluentValidation;
using Sergin.UserAccess.Domain.Users;

namespace Sergin.UserAccess.Application.Users.Commands.ProvisionExternalUser;

/// <summary>
/// Deliberately lenient: this runs inside the OIDC callback, so a failure here fails the sign-in. It
/// refuses only what the database would refuse anyway — an empty subject or username, and values wider
/// than their columns — so a bad provider profile surfaces as a readable message (through
/// <c>ExternalIdentityResolver</c>'s exception) instead of a Postgres error. No length on the username,
/// whose column is unbounded and which Keycloak may fill with an email address, and no NotEmpty on the
/// name fields, which the provider may omit.
/// </summary>
internal sealed class ProvisionExternalUserCommandValidator : AbstractValidator<ProvisionExternalUserCommand>
{
    public ProvisionExternalUserCommandValidator()
    {
        RuleFor(x => x.ExternalId.Value)
            .NotEmpty()
            .MaximumLength(ExternalUserId.MaxLength)
            .OverridePropertyName(nameof(ProvisionExternalUserCommand.ExternalId));

        RuleFor(x => x.UserName.Value)
            .NotEmpty()
            .OverridePropertyName(nameof(ProvisionExternalUserCommand.UserName));

        RuleFor(x => x.Email!.Value)
            .EmailAddress()
            .MaximumLength(EmailAddress.MaxLength)
            .OverridePropertyName(nameof(ProvisionExternalUserCommand.Email))
            .When(x => x.Email is not null);

        RuleFor(x => x.FirstName).MaximumLength(User.NameMaxLength);
        RuleFor(x => x.LastName).MaximumLength(User.NameMaxLength);
    }
}
