using FluentValidation;
using Sergin.UserAccess.Domain.Users;

namespace Sergin.UserAccess.Application.Users.Commands.Create;

// Rules target the value object's Value and OverridePropertyName restores the command property name:
// that name is what ValidationPipelineBehavior puts in Error.Code and what the API groups a
// ValidationProblem by. The wrapper itself is never null — every caller constructs it — only its Value
// can be, when a request body omits the field.
internal sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.UserName.Value)
            .NotEmpty()
            .MaximumLength(UserName.MaxLength)
            .OverridePropertyName(nameof(CreateUserCommand.UserName));
    }
}
