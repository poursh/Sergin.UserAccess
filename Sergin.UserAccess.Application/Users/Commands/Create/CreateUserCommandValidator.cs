using FluentValidation;
using Sergin.SharedKernel.Application.Validations;
using Sergin.UserAccess.Domain.Users;

namespace Sergin.UserAccess.Application.Users.Commands.Create;

// Rules target the value object's Value and OverridePropertyName restores the command property name:
// that name is what ValidationPipelineBehavior puts in Error.Code and what the API groups a
// ValidationProblem by. The wrapper itself is never null — every caller constructs it — only its Value
// can be, when a request body omits the field.
//
// The uniqueness rule targets the wrapper itself, so its Error.Code is already UserName. It is advisory —
// the unique index on ua.users.user_name is the guarantee under a race — and the When keeps the query
// from running for a value the shape rule has already refused.
internal sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator(IUserRepository users)
    {
        RuleFor(x => x.UserName.Value)
            .NotEmpty()
            .MaximumLength(UserName.MaxLength)
            .OverridePropertyName(nameof(CreateUserCommand.UserName));

        RuleFor(x => x.UserName)
            .MustBeUniqueIn(users)
            .When(x => !string.IsNullOrWhiteSpace(x.UserName.Value));
    }
}
