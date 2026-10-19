using FluentValidation;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.SetInheritable;

namespace Sergin.UserAccess.Application.Roles.Commands.SetInheritable;

internal sealed class SetRoleInheritableCommandValidator : AbstractValidator<SetRoleInheritableCommand>
{
    public SetRoleInheritableCommandValidator(IRoleQueryRepository roles)
    {
        RuleFor(x => x.Id).NotEmpty();

        // BR-2 holds for existing assignments too: a role assigned below its owner must stay inheritable.
        RuleFor(x => x.IsInheritable)
            .MustAsync(async (command, _, cancellationToken) =>
                !await roles.HasLiveAssignmentsAwayFromOwnerAsync(command.Id, cancellationToken))
            .WithMessage("The role is assigned below its owner scope; revoke those assignments before making it non-inheritable.")
            .When(x => !x.IsInheritable && x.Id != Guid.Empty);
    }
}
