using FluentValidation;

namespace Sergin.UserAccess.Application.Users.Commands.DeactivateUser;

internal sealed class DeactivateUserCommandValidator : AbstractValidator<DeactivateUserCommand>
{
    public DeactivateUserCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
